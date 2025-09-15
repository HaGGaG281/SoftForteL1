using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApplication10.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        private static List<User> users = new List<User>
        {
            new User { Id = 1, Username = "Alice", Email = "alice@test.com", Password = "12345", Role="Admin" },
            new User { Id = 2, Username = "Bob", Email = "bob@test.com", Password = "12345", Role="User" },
        };

        // ✅ list of courses
        private static List<Course> courses = new List<Course>
        {
            new Course { Id = 1, Title = "C# Basics", Description = "Learn C# from scratch" },
            new Course { Id = 2, Title = "ASP.NET Core", Description = "Build APIs with ASP.NET Core" },
            new Course { Id = 3, Title = "Entity Framework", Description = "Learn EF Core for data access" }
        };

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDTO login)
        {
            var user = users.FirstOrDefault(x => x.Username == login.Username && x.Password == login.Password);
            if (user == null)
                return Unauthorized("Invalid username or password");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequestDTO request)
        {
            if (users.Any(u => u.Username == request.Username))
                return BadRequest(new { message = "Username already exists." });

            var newUser = new User
            {
                Id = users.Count + 1,
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Role = "User"
            };

            users.Add(newUser);

            return Ok(new { message = "Registration successful.", user = newUser });
        }

        // Protected endpoint (needs token)
        [HttpGet("profile")]
        [Authorize]
        public IActionResult Profile()
        {
            return Ok(new
            {
                Username = User.Identity?.Name,
                Role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }

        // Admin-only endpoint
        [HttpGet("admin-area")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminArea()
        {
            return Ok("Welcome, Admin! ");
        }



        [HttpGet("courses")]
        [Authorize(Roles = "Admin")]

        public IActionResult GetCourses()
        {
            
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
                return Unauthorized("Missing or invalid token.");

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var role = "";
            if (token != null)
            {
                role = _jwtService.ValidateTokenRole(token);
            }
            
             

            if (role == "Admin")
            {
                return Ok(courses);
            }

            return Unauthorized("You are not authorized to access courses.");
        }
    }

    // ✅ Course Model
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
