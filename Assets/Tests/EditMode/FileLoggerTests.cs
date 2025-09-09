using System.IO;
using NUnit.Framework;
using RogueLike2D.Core;

public class FileLoggerTests
{
    [Test]
    public void FileLogger_WritesBaseline_OnInitializeAndManualBaseline()
    {
        FileLogger.Initialize();
        FileLogger.EnsureBaselineMarkers("UnitTest");

        var path = FileLogger.GetLogFilePath();
        Assert.IsFalse(string.IsNullOrEmpty(path), "Log file path should not be null or empty.");
        Assert.IsTrue(File.Exists(path), $"Log file should exist at: {path}");

        string contents = File.ReadAllText(path);
        StringAssert.Contains("BASELINE", contents, "Expected baseline marker not found in log file.");
    }
}
