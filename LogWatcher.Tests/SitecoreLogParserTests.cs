using LogWatcher.Services;

namespace LogWatcher.Tests;

[TestClass]
public class SitecoreLogParserTests
{
    private readonly SitecoreLogParser _parser = new();
    private readonly Guid _logFileId = Guid.NewGuid();

    // INFO lines are filtered out
    [TestMethod]
    public void Parse_InfoLines_AreNotReturned()
    {
        var lines = new[]
        {
            "1234 12:00:00 INFO  This is informational"
        };

        var result = _parser.Parse(lines, _logFileId, "log.20260101.txt");

        Assert.IsEmpty(result);
    }

    //recognised error levels are returned
    [TestMethod]
    [DataRow("DEBUG")]
    [DataRow("WARN")]
    [DataRow("ERROR")]
    [DataRow("FATAL")]
    public void Parse_ErrorLevels_AreReturned(string level)
    {
        var lines = new[] { $"1234 10:30:00 {level}  Something went wrong" };

        var result = _parser.Parse(lines, _logFileId, "log.20260603.txt");

        Assert.HasCount(1, result);
        Assert.AreEqual(level, result[0].Level);
    }

    //message text is captured
    [TestMethod]
    public void Parse_ErrorLine_MessageIsCaptured()
    {
        var lines = new[] { "1234 08:15:00 ERROR NullReferenceException in Foo.Bar" };

        var result = _parser.Parse(lines, _logFileId, "log.20260603.txt");

        Assert.AreEqual("NullReferenceException in Foo.Bar", result[0].Message);
    }

    //LogFileId is assigned
    [TestMethod]
    public void Parse_LogFileId_IsAssignedToEveryError()
    {
        var lines = new[]
        {
            "1234 09:00:00 ERROR  First error",
            "1234 09:01:00 FATAL  Second error"
        };

        var result = _parser.Parse(lines, _logFileId, "log.20260603.txt");

        Assert.IsTrue(result.All(e => e.LogFileId == _logFileId));
    }

    //date extracted from filename
    [TestMethod]
    public void Parse_OccurredAt_UsesDateFromFilename()
    {
        var lines = new[] { "1234 14:30:00 ERROR  Something bad" };

        var result = _parser.Parse(lines, _logFileId, "log.20260315.txt");

        Assert.AreEqual(new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc), result[0].OccurredAt);
    }

    //falls back to today when filename has no date
    [TestMethod]
    public void Parse_NoDateInFilename_FallsBackToToday()
    {
        var lines = new[] { "1234 00:00:00 ERROR  Something" };

        var result = _parser.Parse(lines, _logFileId, "application.log");

        Assert.AreEqual(DateTime.UtcNow.Date, result[0].OccurredAt.Date);
    }

    //multi-line stack trace is captured
    [TestMethod]
    public void Parse_StackTrace_IsCapturedForMultiLineEntry()
    {
        var lines = new[]
        {
            "1234 11:00:00 ERROR  Unhandled exception",
            "  at Foo.Bar() in Foo.cs:line 42",
            "  at Program.Main()"
        };

        var result = _parser.Parse(lines, _logFileId, "log.20260603.txt");

        Assert.HasCount(1, result);
        StringAssert.Contains(result[0].StackTrace, "at Foo.Bar()");
        StringAssert.Contains(result[0].StackTrace, "at Program.Main()");
    }

    //no stack trace on clean single-line entry
    [TestMethod]
    public void Parse_SingleLineEntry_StackTraceIsNull()
    {
        var lines = new[] { "1234 11:00:00 WARN  Disk space low" };

        var result = _parser.Parse(lines, _logFileId, "log.20260603.txt");

        Assert.IsNull(result[0].StackTrace);
    }

    //empty input returns empty list
    [TestMethod]
    public void Parse_EmptyLines_ReturnsEmptyList()
    {
        var result = _parser.Parse([], _logFileId, "log.20260603.txt");

        Assert.IsEmpty(result);
    }

    //mixed INFO and ERROR only returns ERROR
    [TestMethod]
    public void Parse_MixedLevels_OnlyErrorLevelsReturned()
    {
        var lines = new[]
        {
            "1234 10:00:00 INFO  Startup complete",
            "1234 10:01:00 ERROR  Config missing",
            "1234 10:02:00 INFO  Retrying",
            "1234 10:03:00 WARN  Slow query"
        };

        var result = _parser.Parse(lines, _logFileId, "log.20260603.txt");

        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(new[] { "ERROR", "WARN" }, result.Select(e => e.Level).ToList());
    }

    //multiple separate entries each get own stack trace
    [TestMethod]
    public void Parse_TwoEntriesWithStackTraces_AreIndependent()
    {
        var lines = new[]
        {
            "1234 12:00:00 ERROR  First",
            "  at First.Method()",
            "1234 12:01:00 ERROR  Second",
            "  at Second.Method()"
        };

        var result = _parser.Parse(lines, _logFileId, "log.20260603.txt");

        Assert.HasCount(2, result);
        StringAssert.Contains(result[0].StackTrace, "First.Method");
        StringAssert.Contains(result[1].StackTrace, "Second.Method");
    }
}
