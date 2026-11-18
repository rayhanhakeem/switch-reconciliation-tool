using System.Globalization;
using System.Text;

namespace Reconciliation;

public static class ReportWriter
{
    public static void WriteCsv(ReconciliationResult result, string path)
    {
        var builder = new StringBuilder();
        builder.AppendLine("reference,status,switchAmount,coreAmount,note");
        foreach (var record in result.Records)
        {
            builder.AppendLine(string.Join(',',
                Escape(record.Reference),
                record.Status,
                Format(record.SwitchAmount),
                Format(record.CoreAmount),
                Escape(record.Note)));
        }

        File.WriteAllText(path, builder.ToString());
    }

    public static string Summary(ReconciliationResult result) =>
        $"""
         records      : {result.Records.Count}
         matched      : {result.MatchedCount}
         breaks       : {result.BreakCount}
         switch total : {Format(result.SwitchTotal)}
         core total   : {Format(result.CoreTotal)}
         difference   : {Format(result.Difference)}
         """;

    private static string Format(decimal? value) =>
        value?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
