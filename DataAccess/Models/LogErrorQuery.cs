using System;
using System.Collections.Generic;
using System.Text;

namespace LogWatcher.Models
{
    public class LogErrorQuery
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
