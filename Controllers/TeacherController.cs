using Microsoft.AspNetCore.Mvc;
using Time_Table_Generator.Models;
using Time_Table_Generator.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Time_Table_Generator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var teachers = _context.Teachers
                .Include(t => t.User)
                .Select(t => new
                {
                    UserId = t.UserId,
                    TeacherId = t.Id,
                    firstName = t.User!.FirstName,
                    lastName = t.User.LastName,
                    displayname = t.User.Displayname,
                    phone = t.User.Phone,
                    address = t.User.Address,
                    email = t.User.Email,
                    password = t.User.Password,
                    userType = t.User.UserType,
                    // Add more teacher-specific fields if needed
                })
                .ToList();

            return Ok(new ResponseResult<object>(teachers));
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var teacher = _context.Teachers.Find(id);
            if (teacher == null) 
                return NotFound(new ResponseResult<object>(new[] { "Teacher not found." }));
            
            return Ok(new ResponseResult<object>(teacher));
        }

        [HttpPost]
        public IActionResult Create(CreateTeacherRequest request)
        {
            if (request == null) 
                return BadRequest(new ResponseResult<object>(new[] { "Teacher cannot be null." }));

            var teacherEntity = new Teacher()
            {
                UserId = request.UserId,
            };

            _context.Teachers.Add(teacherEntity);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = teacherEntity.Id }, new ResponseResult<object>(teacherEntity));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Teacher teacher)
        {
            if (teacher == null) 
                return BadRequest(new ResponseResult<object>(new[] { "Teacher cannot be null." }));
            
            if (id != teacher.Id) 
                return BadRequest(new ResponseResult<object>(new[] { "ID mismatch." }));

            _context.Teachers.Update(teacher);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{userId}")]
        public IActionResult DeleteTeacherByUserId(int userId)
        {
            // Find the teacher by UserId
            var teacher = _context.Teachers.FirstOrDefault(t => t.UserId == userId);
            if (teacher == null)
                return NotFound(new ResponseResult<object>(new[] { "Teacher not found." }));

            // Find the user
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound(new ResponseResult<object>(new[] { "User not found." }));

            // Remove the teacher and user
            _context.Teachers.Remove(teacher);
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new ResponseResult<object>("Teacher and User deleted successfully."));
        }


        /*[HttpGet("classes-and-subjects/{id?}")]
        public IActionResult GetClassesAndSubjects(int? id)
        {
            if (id.HasValue)
            {
                var teacher = _context.Teachers
                    .Where(t => t.Id == id.Value)
                    .Select(t => new 
                    {
                        t.Id,
                        t.Name,
                        Classes = t.Classes.Select(c => c.Name),
                        Subjects = t.Subjects.Select(s => s.Name)
                    })
                    .FirstOrDefault();

                if (teacher == null) 
                    return NotFound(new ResponseResult<object>(new[] { "Teacher not found." }));
                
                return Ok(new ResponseResult<object>(teacher));
            }
            else
            {
                var teachers = _context.Teachers
                    .Select(t => new 
                    {
                        t.Id,
                        t.Name,
                        Classes = t.Classes.Select(c => c.Name),
                        Subjects = t.Subjects.Select(s => s.Name)
                    })
                    .ToList();

                return Ok(new ResponseResult<object>(teachers));
            }
        }*/
    }
}
