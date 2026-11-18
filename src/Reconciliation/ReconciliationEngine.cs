using System.Globalization;

namespace Reconciliation;

public static class ReconciliationEngine
{
    public static ReconciliationResult Reconcile(
        IReadOnlyList<JournalEntry> switchEntries,
        IReadOnlyList<JournalEntry> coreEntries)
    {
        var switchGroups = switchEntries.ToLookup(entry => entry.Reference);
        var coreGroups = coreEntries.ToLookup(entry => entry.Reference);

        var records = new List<BreakRecord>();
        foreach (var reference in switchGroups.Select(group => group.Key)
                     .Union(coreGroups.Select(group => group.Key))
                     .OrderBy(reference => reference, StringComparer.Ordinal))
        {
            var switchRows = switchGroups[reference].ToList();
            var coreRows = coreGroups[reference].ToList();
            var switchAmount = switchRows.Count > 0 ? switchRows[0].Amount : (decimal?)null;
            var coreAmount = coreRows.Count > 0 ? coreRows[0].Amount : (decimal?)null;

            if (switchRows.Count > 1)
            {
                records.Add(new BreakRecord(reference, BreakStatus.DuplicateInSwitch, switchRows.Sum(row => row.Amount), coreAmount,
                    $"{switchRows.Count} rows in switch file"));
            }
            else if (coreRows.Count > 1)
            {
                records.Add(new BreakRecord(reference, BreakStatus.DuplicateInCore, switchAmount, coreRows.Sum(row => row.Amount),
                    $"{coreRows.Count} rows in core file"));
            }
            else if (switchRows.Count == 1 && coreRows.Count == 0)
            {
                records.Add(new BreakRecord(reference, BreakStatus.MissingInCore, switchAmount, null, "Debited by switch, absent in core journal"));
            }
            else if (switchRows.Count == 0)
            {
                records.Add(new BreakRecord(reference, BreakStatus.MissingInSwitch, null, coreAmount, "Posted in core, absent in switch file"));
            }
            else if (switchRows[0].Currency != coreRows[0].Currency)
            {
                records.Add(new BreakRecord(reference, BreakStatus.CurrencyMismatch, switchAmount, coreAmount,
                    $"{switchRows[0].Currency} vs {coreRows[0].Currency}"));
            }
            else if (switchRows[0].Amount != coreRows[0].Amount)
            {
                var delta = (switchRows[0].Amount - coreRows[0].Amount).ToString("0.00", CultureInfo.InvariantCulture);
                records.Add(new BreakRecord(reference, BreakStatus.AmountMismatch, switchAmount, coreAmount,
                    $"delta {delta}"));
            }
            else
            {
                records.Add(new BreakRecord(reference, BreakStatus.Matched, switchAmount, coreAmount, string.Empty));
            }
        }

        return new ReconciliationResult(
            records,
            switchEntries.Sum(entry => entry.Amount),
            coreEntries.Sum(entry => entry.Amount));
    }
}
