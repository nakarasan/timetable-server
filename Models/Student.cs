namespace Time_Table_Generator.Models
{
    public class Student
    {
        public int Id { get; set; }
        public required int UserId { get; set; } 
        public User? User { get; set; } 
        public int? BatchId { get; set; }
        public Batch? Batch { get; set; }
        public string? RollNumber { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; } = string.Empty;
        public int? ClassId { get; set; } // Foreign key for Class
    }
}