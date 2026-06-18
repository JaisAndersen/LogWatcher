using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogWatcher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogErrorsController : ControllerBase
    {
        private readonly ILogErrorRepository _repository;

        public LogErrorsController(ILogErrorRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns a paginated list of log errors, optionally filtered by date range.
        /// Returns 400 Bad Request if the from date is later than the to date.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<LogError>>> Get([FromQuery] LogErrorQuery query)
        {
            if (query.From.HasValue && query.To.HasValue && query.From > query.To)
                return BadRequest("The from date has to be before the to date");

            var result = await _repository.GetAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Marks a log error as acknowledged, indicating it has been seen and handled.
        /// Returns 204 No Content on success.
        /// </summary>
        [HttpPatch("{id}/acknowledge")]
        public async Task<IActionResult> Acknowledge(Guid id)
        {
            await _repository.AcknowledgeAsync(id);
            return NoContent();
        }
    }
}
