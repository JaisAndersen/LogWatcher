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

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Handles the initial page load and any subsequent filtering by date range. 
        /// Validates that the "From" date is not after the "To" date and retrieves the relevant log errors based on the specified criteria.
        /// </summary>
        public async Task OnGetAsync()
        {
            if (From.HasValue && To.HasValue && From > To)
            {
                ModelState.AddModelError(nameof(From), "Fra-dato skal være før til-dato");
                return;
            }

            Errors = await _repository.GetAsync(new LogErrorQuery
            {
                From = From,
                To = To,
                Page = CurrentPage,
                PageSize = 10
            });
        }

        /// <summary>
        /// Handles AJAX requests to retrieve log errors without a full page reload.
        /// </summary>
        public async Task<IActionResult> OnGetPollAsync()
        {
            var result = await _repository.GetAsync(new LogErrorQuery
            {
                Page = 1,
                PageSize = 10
            });
            return new JsonResult(result);
        }

        /// <summary>
        /// Handles AJAX requests to acknowledge a specific log error.
        /// </summary>
        public async Task<IActionResult> OnPostAcknowledgeAsync(Guid id)
        {
            await _repository.AcknowledgeAsync(id);
            return new JsonResult(new { success = true });
        }
    }
}
