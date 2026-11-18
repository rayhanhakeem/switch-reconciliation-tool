using Reconciliation;

var options = Parse(args);
if (options is null)
{
    Console.Error.WriteLine("usage: Reconciliation --switch <path> --core <path> [--out <report.csv>]");
    return 1;
}

try
{
    var switchEntries = JournalCsv.Read(options.Value.SwitchPath);
    var coreEntries = JournalCsv.Read(options.Value.CorePath);
    var result = ReconciliationEngine.Reconcile(switchEntries, coreEntries);
    ReportWriter.WriteCsv(result, options.Value.OutputPath);
    Console.WriteLine(ReportWriter.Summary(result));
    Console.WriteLine($"report       : {Path.GetFullPath(options.Value.OutputPath)}");
    return result.BreakCount > 0 ? 2 : 0;
}
catch (Exception exception) when (exception is IOException or InvalidDataException)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static (string SwitchPath, string CorePath, string OutputPath)? Parse(string[] arguments)
{
    string? switchPath = null;
    string? corePath = null;
    var outputPath = "reconciliation-report.csv";

    for (var index = 0; index < arguments.Length; index++)
    {
        var value = index + 1 < arguments.Length ? arguments[index + 1] : null;
        switch (arguments[index])
        {
            case "--switch" when value is not null:
                switchPath = value;
                index++;
                break;
            case "--core" when value is not null:
                corePath = value;
                index++;
                break;
            case "--out" when value is not null:
                outputPath = value;
                index++;
                break;
            default:
                return null;
        }
    }

    return switchPath is null || corePath is null ? null : (switchPath, corePath, outputPath);
}
