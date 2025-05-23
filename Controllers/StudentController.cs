using Microsoft.AspNetCore.Mvc;
using Time_Table_Generator.Models;
using Time_Table_Generator.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Time_Table_Generator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var students = _context.Students
                .Include(s => s.User)
                .Include(s => s.Batch)
                .Select(s => new
                {
                    UserId = s.UserId,
                    firstName = s.User!.FirstName,
                    lastName = s.User.LastName,
                    displayname = s.User.Displayname,
                    phone = s.User.Phone,
                    address = s.User.Address,
                    email = s.User.Email,
                    password = s.User.Password,
                    userType = s.User.UserType,
                    rollNumber = s.RollNumber,
                    registrationNumber = s.RegistrationNumber,
                    batchName = s.Batch != null ? s.Batch.Name : null
                })
                .ToList();

            return Ok(new ResponseResult<object>(students));
        }


        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
                return NotFound(new ResponseResult<object>(new[] { "Student not found." }));

            return Ok(new ResponseResult<object>(student));
        }

        [HttpPost]
        public IActionResult Create(CreateStudentRequest request)
        {
            if (request == null)
                return BadRequest(new ResponseResult<object>(new[] { "Student cannot be null." }));

            var studentEntity = new Student()
            {
                UserId = request.UserId,
                BatchId = request.BatchId,
                RollNumber = request.RollNumber,
                RegistrationNumber = request.RegistrationNumber,
            };

            _context.Students.Add(studentEntity);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = studentEntity.Id }, new ResponseResult<object>(studentEntity));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Student student)
        {
            if (student == null || id != student.Id)
                return BadRequest(new ResponseResult<object>(new[] { "Invalid student data or ID mismatch." }));

            _context.Students.Update(student);
            _context.SaveChanges();

            return Ok(new ResponseResult<object>(student));
        }

        [HttpDelete("{userId}")]
        public IActionResult DeleteByUserId(int userId)
        {
            // Find the student by UserId
            var student = _context.Students.FirstOrDefault(s => s.UserId == userId);
            if (student == null)
                return NotFound(new ResponseResult<object>(new[] { "Student not found." }));

            // Find the user
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound(new ResponseResult<object>(new[] { "User not found." }));

            // Remove the student and user
            _context.Students.Remove(student);
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new ResponseResult<object>("Student and User deleted successfully."));
        }
    }
}
