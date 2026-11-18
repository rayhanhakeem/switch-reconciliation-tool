using Reconciliation;
using Xunit;

namespace Reconciliation.Tests;

public class JournalCsvTests
{
    private static string TempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"journal-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Reads_valid_rows_and_normalizes_currency()
    {
        var path = TempFile("reference,amount,currency,valueDate\nSW-1,1500.50,usd,2026-11-17\n");

        var entry = Assert.Single(JournalCsv.Read(path));

        Assert.Equal("SW-1", entry.Reference);
        Assert.Equal(1500.50m, entry.Amount);
        Assert.Equal("USD", entry.Currency);
        Assert.Equal(new DateOnly(2026, 11, 17), entry.ValueDate);
    }

    [Fact]
    public void Rejects_wrong_header()
    {
        var path = TempFile("ref,amount\nA,1\n");

        Assert.Throws<InvalidDataException>(() => JournalCsv.Read(path));
    }

    [Fact]
    public void Rejects_invalid_amount_with_line_number()
    {
        var path = TempFile("reference,amount,currency,valueDate\nA,abc,IDR,2026-11-17\n");

        var error = Assert.Throws<InvalidDataException>(() => JournalCsv.Read(path));
        Assert.Contains(":2:", error.Message);
    }

    [Fact]
    public void Empty_file_is_treated_as_no_entries()
    {
        Assert.Empty(JournalCsv.Read(TempFile(string.Empty)));
    }
}
