using Microsoft.AspNetCore.Mvc;
using Time_Table_Generator.Models.Request;
using Time_Table_Generator.Models;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly AppDbContext _context;

    public MessageController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { message = "Message text is required." });
        }

        var message = new Message
        {
            Text = request.Text,
            IsRead = false,
            BatchId = request.BatchId,
            SenderId = request.SenderId,
            CreatedAt = DateTime.Now
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Message created successfully", data = message });
    }


    [HttpGet("batch/{batchId}")]
    public IActionResult GetMessagesByBatchId(int batchId)
    {
        var messages = _context.Messages
                .Where(m => m.BatchId == batchId)
                .Join(_context.Users,
                    m => m.SenderId,
                    u => u.Id,
                    (m, u) => new
                    {
                        id = m.Id,
                        text = m.Text,
                        isRead = m.IsRead,
                        batchId = m.BatchId,
                        senderId = m.SenderId,
                        senderName = u.Displayname,
                        createdAt = m.CreatedAt
                    })
                .OrderByDescending(m => m.createdAt)
                .ToList();


        return Ok( messages );
    }


}
