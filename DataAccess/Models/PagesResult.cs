using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class PagesResult<T>
    {
        public List<T> Items { get; set; } new();
        public int TotalCount { get; set; }
        public int Paged { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
