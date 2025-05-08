using Microsoft.AspNetCore.Mvc;
using Time_Table_Generator.Models;
using Time_Table_Generator.Models.Request;

namespace Time_Table_Generator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var subjects = (from subject in _context.Subjects
                            join hour in _context.SubjectHours
                            on subject.Id equals hour.SubjectId
                            orderby subject.Id descending
                            select new
                            {
                                SubjectId = subject.Id,
                                SubjectHourId = hour.Id,
                                Name = subject.Name,
                                HoursInWeek = hour.HoursInWeek,
                                HoursInDay = hour.HoursInDay
                            }).ToList();

            return Ok(new ResponseResult<object>(subjects));
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var subject = _context.Subjects.Find(id);
            if (subject == null)
                return NotFound(new ResponseResult<object>(new[] { "Subject not found." }));

            return Ok(new ResponseResult<object>(subject));
        }

        [HttpPost]
        public IActionResult Create(CreateSubjectRequest request)
        {
            if (request == null)
                return BadRequest(new ResponseResult<object>(new[] { "Subject cannot be null." }));

            var subjectEntity = new Subject()
            {
                Name = request.Name,
            };

            _context.Subjects.Add(subjectEntity);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = subjectEntity.Id }, new ResponseResult<object>(subjectEntity));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Subject subject)
        {
            if (subject == null || id != subject.Id)
                return BadRequest(new ResponseResult<object>(new[] { "Invalid subject data or ID mismatch." }));

            _context.Subjects.Update(subject);
            _context.SaveChanges();

            return Ok(new ResponseResult<object>(subject));
        }

        /*  [HttpDelete("{id}")]
          public IActionResult Delete(int id)
          {
              var subject = _context.Subjects.Find(id);
              if (subject == null)
                  return NotFound(new ResponseResult<object>(new[] { "Subject not found." }));

              _context.Subjects.Remove(subject);
              _context.SaveChanges();

              return Ok(new ResponseResult<object>("Subject deleted successfully."));
          }*/

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var subject = _context.Subjects.Find(id);
            if (subject == null)
                return NotFound(new ResponseResult<object>(new[] { "Subject not found." }));

            // Delete related SubjectHours first
            var subjectHours = _context.SubjectHours.Where(sh => sh.SubjectId == id).ToList();
            _context.SubjectHours.RemoveRange(subjectHours);

            _context.Subjects.Remove(subject);
            _context.SaveChanges();

            return Ok(new ResponseResult<object>("Subject deleted successfully."));
        }
    }
}
