using Reconciliation;
using Xunit;

namespace Reconciliation.Tests;

public class ReportWriterTests
{
    [Fact]
    public void Writes_header_and_escapes_notes_containing_commas()
    {
        var path = Path.Combine(Path.GetTempPath(), $"report-{Guid.NewGuid():N}.csv");
        var result = new ReconciliationResult(
            [new BreakRecord("A,1", BreakStatus.Matched, 10m, 10m, "ok")],
            10m, 10m);

        ReportWriter.WriteCsv(result, path);
        var lines = File.ReadAllLines(path);

        Assert.Equal("reference,status,switchAmount,coreAmount,note", lines[0]);
        Assert.Equal("\"A,1\",Matched,10.00,10.00,ok", lines[1]);
    }

    [Fact]
    public void Summary_reports_break_count_and_difference()
    {
        var result = new ReconciliationResult(
            [new BreakRecord("A", BreakStatus.AmountMismatch, 100m, 90m, "delta 10.00")],
            100m, 90m);

        var summary = ReportWriter.Summary(result);

        Assert.Contains("breaks       : 1", summary);
        Assert.Contains("difference   : 10.00", summary);
    }
}
