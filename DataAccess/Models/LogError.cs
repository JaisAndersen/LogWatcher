namespace DataAccess.Models
{
    public class LogError
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
