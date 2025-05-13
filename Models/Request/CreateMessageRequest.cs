namespace Time_Table_Generator.Models.Request
{
    public class CreateMessageRequest
    {
        public string? Text { get; set; }
        public int? BatchId { get; set; }
        public int? SenderId { get; set; }
    }
}
