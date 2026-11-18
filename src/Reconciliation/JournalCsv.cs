using System.Globalization;

namespace Reconciliation;

/// <summary>Reads the journal export: reference,amount,currency,valueDate.</summary>
public static class JournalCsv
{
    private static readonly string[] Header = ["reference", "amount", "currency", "valueDate"];

    public static IReadOnlyList<JournalEntry> Read(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0)
        {
            return [];
        }

        var columns = lines[0].Split(',').Select(column => column.Trim()).ToArray();
        if (!columns.SequenceEqual(Header, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"{path}: expected header {string.Join(',', Header)}");
        }

        var entries = new List<JournalEntry>();
        for (var lineNumber = 2; lineNumber <= lines.Length; lineNumber++)
        {
            var line = lines[lineNumber - 1];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = line.Split(',');
            if (fields.Length != Header.Length)
            {
                throw new InvalidDataException($"{path}:{lineNumber}: expected {Header.Length} fields, found {fields.Length}");
            }

            if (!decimal.TryParse(fields[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                throw new InvalidDataException($"{path}:{lineNumber}: invalid amount '{fields[1]}'");
            }

            if (!DateOnly.TryParseExact(fields[3], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var valueDate))
            {
                throw new InvalidDataException($"{path}:{lineNumber}: invalid date '{fields[3]}'");
            }

            entries.Add(new JournalEntry(fields[0].Trim(), amount, fields[2].Trim().ToUpperInvariant(), valueDate));
        }

        return entries;
    }
}
