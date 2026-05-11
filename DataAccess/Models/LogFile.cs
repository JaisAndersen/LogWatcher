using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class LogFile
    {
        public Guid Id { get; set; }        
        public string FilePath { get; set; } = string.Empty;
        public long ByteOffset { get; set; }
        public DateTime LastPolled { get; set; }
        public ICollection<LogError> Errors { get; set; } = new List<LogError>();
    }
}
