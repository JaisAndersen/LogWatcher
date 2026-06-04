using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LogWatcher.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogErrorRepository _repository;

        public IndexModel(ILogErrorRepository repository)
        {
            _repository = repository;
        }
        public PagedResult<LogError> Errors { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public async Task OnGetAsync()
        {
            Errors = await _repository.GetAsync(new LogErrorQuery
            {
                From = From,
                To = To
            });
        }
    }
}
