using LogWatcher.Services;

namespace LogWatcher.Tests;

[TestClass]
public class LogFileReaderTests
{
    private readonly LogFileReader _reader = new();
    private string _tempFile = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempFile = Path.GetTempFileName();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    //reads all lines from offset 0
    [TestMethod]
    public async Task ReadNewLinesAsync_FromStart_ReturnsAllLines()
    {
        File.WriteAllLines(_tempFile, ["line1", "line2", "line3"]);

        var (lines, _) = await _reader.ReadNewLinesAsync(_tempFile, 0);

        CollectionAssert.AreEqual(new[] { "line1", "line2", "line3" }, lines.ToList());
    }

    //empty file returns empty list and offset 0
    [TestMethod]
    public async Task ReadNewLinesAsync_EmptyFile_ReturnsEmptyAndZeroOffset()
    {
        File.WriteAllText(_tempFile, "");

        var (lines, newOffset) = await _reader.ReadNewLinesAsync(_tempFile, 0);

        Assert.IsEmpty(lines);
        Assert.AreEqual(0L, newOffset);
    }

    //returned offset equals file length
    [TestMethod]
    public async Task ReadNewLinesAsync_ReturnsOffsetEqualToFileLength()
    {
        File.WriteAllText(_tempFile, "hello\nworld\n");
        var expectedLength = new FileInfo(_tempFile).Length;

        var (_, newOffset) = await _reader.ReadNewLinesAsync(_tempFile, 0);

        Assert.AreEqual(expectedLength, newOffset);
    }

    //reading from a non-zero offset skips already-read content
    [TestMethod]
    public async Task ReadNewLinesAsync_FromOffset_SkipsPreviousContent()
    {
        File.WriteAllLines(_tempFile, ["first", "second"]);

        // First read to get the offset after "first"
        var (_, offsetAfterFirst) = await _reader.ReadNewLinesAsync(_tempFile, 0);

        // Append a new line
        File.AppendAllLines(_tempFile, ["third"]);

        var (lines, _) = await _reader.ReadNewLinesAsync(_tempFile, offsetAfterFirst);

        Assert.HasCount(1, lines);
        Assert.AreEqual("third", lines[0]);
    }

    //incremental reads return only new content
    [TestMethod]
    public async Task ReadNewLinesAsync_IncrementalRead_OnlyReturnsNewLines()
    {
        File.WriteAllLines(_tempFile, ["line1", "line2"]);
        var (_, offset) = await _reader.ReadNewLinesAsync(_tempFile, 0);

        File.AppendAllLines(_tempFile, ["line3", "line4"]);
        var (lines, _) = await _reader.ReadNewLinesAsync(_tempFile, offset);

        CollectionAssert.AreEqual(new[] { "line3", "line4" }, lines.ToList());
    }
}
