using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace DataAccess.Models
{
    public class LogError
    {
        public Guid Id { get; set; }
        public Guid LogFileId { get; set; }
        public LogFile LogFile { get; set; } = null!;
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public bool Acknowledged { get; set; } = false;
        public DateTime OccurredAt { get; set; }
        public DateTime Created { get; set; }   
        public DateTime? Updated { get; set; }
    }
}
