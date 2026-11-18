namespace Reconciliation;

public sealed record JournalEntry(string Reference, decimal Amount, string Currency, DateOnly ValueDate);

public enum BreakStatus
{
    Matched,
    AmountMismatch,
    CurrencyMismatch,
    MissingInCore,
    MissingInSwitch,
    DuplicateInSwitch,
    DuplicateInCore
}

public sealed record BreakRecord(
    string Reference,
    BreakStatus Status,
    decimal? SwitchAmount,
    decimal? CoreAmount,
    string Note)
{
    public bool IsBreak => Status != BreakStatus.Matched;
}

public sealed record ReconciliationResult(
    IReadOnlyList<BreakRecord> Records,
    decimal SwitchTotal,
    decimal CoreTotal)
{
    public int BreakCount => Records.Count(record => record.IsBreak);
    public int MatchedCount => Records.Count(record => record.Status == BreakStatus.Matched);
    public decimal Difference => SwitchTotal - CoreTotal;
}
