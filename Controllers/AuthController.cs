using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Time_Table_Generator.Models;
using Time_Table_Generator.Models.Request;
using Time_Table_Generator.Helpers;
using System.Collections.Generic;

namespace Time_Table_Generator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        // Constructor for dependency injection
        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Register method
        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Displayname) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ResponseResult<object>(new[] { "Missing required fields." }));
            }

            var newUser = new User
            {
                FirstName = request.FirstName ?? "",
                LastName = request.LastName ?? "",
                Displayname = request.Displayname,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email,
                Password = Helpers.PasswordHelper.HashPassword(request.Password),
                UserType = request.UserType ?? UserType.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            if (newUser.UserType == UserType.Teacher)
            {
                var teacher = new Teacher
                {
                    UserId = newUser.Id
                };
                _context.Teachers.Add(teacher);
                _context.SaveChanges();
            }
            else if (newUser.UserType == UserType.Student)
            {
                var student = new Student
                {
                    UserId = newUser.Id,
                    RollNumber = request.RollNumber ?? string.Empty,
                    RegistrationNumber = request.RegistrationNumber ?? string.Empty,
                    BatchId = request.BatchId
                };
                _context.Students.Add(student);
                _context.SaveChanges();
            }

            // Respond with the newly created user and their ID
            var response = new ResponseResult<object>(new
            {
                User = newUser,
                UserId = newUser.Id
            });

            return Ok(response);
        }


        // Login method
        [HttpPost("login")]
        public ActionResult<ResponseResult<object>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ResponseResult<object>(new[] { "Email and password are required." }));
            }

            // Check if the user exists in the database
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !Helpers.PasswordHelper.VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized(new ResponseResult<object>(new[] { "Invalid email or password." }));
            }

            // Get JWT secret from configuration
            var secret = _configuration.GetValue<string>("Jwt:Key");
            if (string.IsNullOrWhiteSpace(secret))
            {
                return StatusCode(500, new ResponseResult<object>(new[] { "JWT secret key is missing." }));
            }

            // Generate JWT token
            var token = JwtHelper.GenerateToken(user ?? throw new ArgumentNullException(nameof(user)), secret);

            // Create success response with token and user info
            var loginResult = new
            {
                Token = token,
                Id = user.Id,
                Displayname = user.Displayname,
                Email = user.Email,
                UserType = user.UserType
            };

            return Ok(new ResponseResult<object>(loginResult));
        }

        [HttpGet("Admins")]
        public IActionResult GetAllAdmins()
        {
            var admins = _context.Users
                .Where(u => u.UserType == UserType.Admin)
                .Select(u => new
                {
                    userId = u.Id,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    displayname = u.Displayname,
                    phone = u.Phone,
                    address = u.Address,
                    email = u.Email,
                    password = u.Password,
                    userType = u.UserType,
                    status = u.Status,
                    createdAt = u.CreatedAt,
                    updatedAt = u.UpdatedAt
                })
                .ToList();

            return Ok(new ResponseResult<object>(admins));
        }

        [HttpGet("user/{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new ResponseResult<object>(new[] { "User not found." }));
            }

            // Base user info
            var userInfo = new
            {
                userId = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                displayname = user.Displayname,
                phone = user.Phone,
                address = user.Address,
                email = user.Email,
                userType = user.UserType,
                status = user.Status,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt
            };

            object extendedInfo = user.UserType switch
            {
                UserType.Student => _context.Students
                        .Where(s => s.UserId == id)
                        .Select(s => new
                        {
                            batchId = s.BatchId, // keep this as is
                            rollNumber = s.RollNumber,
                            registrationNumber = s.RegistrationNumber,
                            batchInfo = new // renamed from BatchId and BatchName
                            {
                                id = s.Batch.Id,
                                name = s.Batch.Name
                            }
                        })
                        .FirstOrDefault(),

                UserType.Teacher => _context.Teachers
                    .Where(t => t.UserId == id)
                    .Select(t => new
                    {
                        teacherId = t.Id,
                    })
                    .FirstOrDefault(),

                _ => null
            };

            return Ok(new ResponseResult<object>(new
            {
                user = userInfo,
                details = extendedInfo
            }));
        }


        [HttpDelete("Admin/{userId}")]
        public IActionResult DeleteAdmin(int userId)
        {
            // Find the user
            var user = _context.Users.FirstOrDefault(u => u.Id == userId && u.UserType == UserType.Admin);
            if (user == null)
                return NotFound(new ResponseResult<object>(new[] { "Admin user not found." }));

            // Delete the user
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new ResponseResult<object>("Admin user deleted successfully."));
        }


    }
}