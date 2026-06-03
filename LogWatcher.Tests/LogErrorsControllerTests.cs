using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using LogWatcher.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LogWatcher.Tests;

[TestClass]
public class LogErrorsControllerTests
{
    private Mock<ILogErrorRepository> _repo = null!;
    private LogErrorsController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new Mock<ILogErrorRepository>();
        _controller = new LogErrorsController(_repo.Object);
    }

    //returns 200 with paged result
    [TestMethod]
    public async Task Get_ValidQuery_Returns200WithResult()
    {
        var expected = new PagedResult<LogError> { TotalCount = 1, Page = 1, PageSize = 20, Items = [new LogError()] };
        _repo.Setup(r => r.GetAsync(It.IsAny<LogErrorQuery>())).ReturnsAsync(expected);

        var result = await _controller.Get(new LogErrorQuery());

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(expected, ok.Value);
    }

    //From > To returns 400
    [TestMethod]
    public async Task Get_FromAfterTo_Returns400()
    {
        var query = new LogErrorQuery
        {
            From = new DateTime(2026, 6, 2),
            To = new DateTime(2026, 6, 1)
        };

        var result = await _controller.Get(query);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result.Result);
        _repo.Verify(r => r.GetAsync(It.IsAny<LogErrorQuery>()), Times.Never);
    }

    //From == To is valid
    [TestMethod]
    public async Task Get_FromEqualsTo_Returns200()
    {
        var date = new DateTime(2026, 6, 1);
        var query = new LogErrorQuery { From = date, To = date };
        _repo.Setup(r => r.GetAsync(It.IsAny<LogErrorQuery>())).ReturnsAsync(new PagedResult<LogError>());

        var result = await _controller.Get(query);

        Assert.IsInstanceOfType<OkObjectResult>(result.Result);
    }

    //only From specified is valid
    [TestMethod]
    public async Task Get_OnlyFromSpecified_Returns200()
    {
        var query = new LogErrorQuery { From = DateTime.UtcNow };
        _repo.Setup(r => r.GetAsync(It.IsAny<LogErrorQuery>())).ReturnsAsync(new PagedResult<LogError>());

        var result = await _controller.Get(query);

        Assert.IsInstanceOfType<OkObjectResult>(result.Result);
    }

    //repository is called with the same query object
    [TestMethod]
    public async Task Get_PassesQueryToRepository()
    {
        var query = new LogErrorQuery { Page = 2, PageSize = 5 };
        _repo.Setup(r => r.GetAsync(query)).ReturnsAsync(new PagedResult<LogError>());

        await _controller.Get(query);

        _repo.Verify(r => r.GetAsync(query), Times.Once);
    }

    //acknowledge: returns 204
    [TestMethod]
    public async Task Acknowledge_ValidId_Returns204()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.AcknowledgeAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Acknowledge(id);

        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    //repository is called with correct id
    [TestMethod]
    public async Task Acknowledge_CallsRepositoryWithCorrectId()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.AcknowledgeAsync(id)).Returns(Task.CompletedTask);

        await _controller.Acknowledge(id);

        _repo.Verify(r => r.AcknowledgeAsync(id), Times.Once);
    }
}
