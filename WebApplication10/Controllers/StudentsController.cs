using Microsoft.AspNetCore.Mvc;

namespace WebApplication10.Controllers
{
    [ApiController]
    [Route("[controller]")] // localhost:port/students
    public class StudentsController : ControllerBase
    {
        
        private readonly JwtService _jwtService;
        public StudentsController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        private static List<Student> students = new List<Student>
        {
            new Student { Id = 1, Name = "Alice", Age = 20 },
            new Student { Id = 2, Name = "Bob", Age = 22 },
            new Student { Id = 3, Name = "Charlie", Age = 23 }
        };

       [HttpGet] // GET: localhost:port/students
        public ActionResult<List<Student>> GetAllStudents()
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
                return Ok(students);
            }
            return Unauthorized("You are not authorized to access courses.");


        }


        [HttpGet("get_student_by_id")] // GET: localhost:port/students
        public ActionResult<Student> GetStudentById([FromQuery]int id)
        {

           var student = students.FirstOrDefault(x => x.Id == id);
            if (student == null)
                return NotFound("This student not found by this id");
            else
                return Ok(student);
        }

        [HttpPost]
        public ActionResult<Student> AddStudent([FromBody] Student student)
        {
            if (students.Any(x => x.Id == student.Id))
                return BadRequest("Student with this id already exists.");
            students.Add(student);
            return CreatedAtAction(nameof(GetStudentById), student);
        }

        [HttpDelete]
        public ActionResult DeleteStudent([FromQuery]int id)
        {
            var student = students.FirstOrDefault(x => x.Id == id);
            if (student == null)
                return NotFound("This student not found by this id");
            students.Remove(student);
            return NoContent();
        }
    }
}
