namespace Time_Table_Generator.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public bool? IsRead { get; set; }
        public int? BatchId { get; set; }
        public int? SenderId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
