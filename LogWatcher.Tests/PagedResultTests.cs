using DataAccess.Models;

namespace LogWatcher.Tests;

[TestClass]
public class PagedResultTests
{
    [TestMethod]
    [DataRow(10, 10, 1)]
    [DataRow(11, 10, 2)]
    [DataRow(20, 10, 2)]
    [DataRow(21, 10, 3)]
    [DataRow(0, 10, 0)]
    [DataRow(1, 1, 1)]
    public void TotalPages_CalculatesCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        var result = new PagedResult<LogError>
        {
            TotalCount = totalCount,
            PageSize = pageSize
        };

        Assert.AreEqual(expectedPages, result.TotalPages);
    }
}
