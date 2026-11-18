using Reconciliation;
using Xunit;

namespace Reconciliation.Tests;

public class ReconciliationEngineTests
{
    private static JournalEntry Entry(string reference, decimal amount, string currency = "IDR") =>
        new(reference, amount, currency, new DateOnly(2026, 11, 17));

    [Fact]
    public void Equal_entries_are_matched()
    {
        var result = ReconciliationEngine.Reconcile([Entry("A", 100)], [Entry("A", 100)]);

        var record = Assert.Single(result.Records);
        Assert.Equal(BreakStatus.Matched, record.Status);
        Assert.False(record.IsBreak);
    }

    [Fact]
    public void Amount_difference_is_flagged_with_delta()
    {
        var result = ReconciliationEngine.Reconcile([Entry("A", 100)], [Entry("A", 90)]);

        var record = Assert.Single(result.Records);
        Assert.Equal(BreakStatus.AmountMismatch, record.Status);
        Assert.Equal("delta 10.00", record.Note);
        Assert.Equal(10m, result.Difference);
    }

    [Fact]
    public void Currency_difference_wins_over_amount_check()
    {
        var result = ReconciliationEngine.Reconcile([Entry("A", 100, "USD")], [Entry("A", 100, "SGD")]);

        Assert.Equal(BreakStatus.CurrencyMismatch, Assert.Single(result.Records).Status);
    }

    [Fact]
    public void Entry_only_in_switch_is_missing_in_core()
    {
        var result = ReconciliationEngine.Reconcile([Entry("A", 100)], []);

        var record = Assert.Single(result.Records);
        Assert.Equal(BreakStatus.MissingInCore, record.Status);
        Assert.Null(record.CoreAmount);
    }

    [Fact]
    public void Entry_only_in_core_is_missing_in_switch()
    {
        var result = ReconciliationEngine.Reconcile([], [Entry("A", 100)]);

        Assert.Equal(BreakStatus.MissingInSwitch, Assert.Single(result.Records).Status);
    }

    [Fact]
    public void Duplicate_switch_rows_are_reported_with_their_sum()
    {
        var result = ReconciliationEngine.Reconcile([Entry("A", 100), Entry("A", 50)], [Entry("A", 100)]);

        var record = Assert.Single(result.Records);
        Assert.Equal(BreakStatus.DuplicateInSwitch, record.Status);
        Assert.Equal(150m, record.SwitchAmount);
        Assert.Contains("2 rows", record.Note);
    }

    [Fact]
    public void Duplicate_core_rows_are_reported()
    {
        var result = ReconciliationEngine.Reconcile([Entry("A", 100)], [Entry("A", 100), Entry("A", 100)]);

        Assert.Equal(BreakStatus.DuplicateInCore, Assert.Single(result.Records).Status);
    }

    [Fact]
    public void Records_are_ordered_by_reference_and_breaks_are_counted()
    {
        var result = ReconciliationEngine.Reconcile(
            [Entry("B", 100), Entry("A", 100), Entry("C", 100)],
            [Entry("A", 100)]);

        Assert.Equal(["A", "B", "C"], result.Records.Select(record => record.Reference));
        Assert.Equal(1, result.MatchedCount);
        Assert.Equal(2, result.BreakCount);
    }
}
