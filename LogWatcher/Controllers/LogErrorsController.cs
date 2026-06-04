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

        [HttpGet]
        public async Task<ActionResult<PagedResult<LogError>>> Get([FromQuery] LogErrorQuery query)
        {
            if (query.From.HasValue && query.To.HasValue && query.From > query.To)
                return BadRequest("The from date has to be before the to date");

            var result = await _repository.GetAsync(query);
            return Ok(result);
        }

        [HttpPatch("{id}/acknowledge")]
        public async Task<IActionResult> Acknowledge(Guid id)
        {
            await _repository.AcknowledgeAsync(id);
            return NoContent();
        }
    }
}
