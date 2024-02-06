using System.Globalization;
using BenchmarkDotNet.Reports;
using BitSets;
using OrderDependencyBenchmark;
using OrderDependencyModels;

if (args.Length != 2)
{
    Console.Error.WriteLine($"Expected 2 argument, got {args.Length}.");
    Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} [datasets.csv path: string] [runs.csv path: string]");
    return 1;
}

var datasetsCsvPath = args[0];
var runsCsvPath = args[1];
if (File.Exists(runsCsvPath) == false)
{
    File.WriteAllLines(runsCsvPath, ["time,algorithm,machine_name"]);
}

var runTime = DateTime.UtcNow;
File.AppendAllLines(runsCsvPath, [$"{runTime:o},C#,{Environment.MachineName}"]);
var measurementsCsvPath = Path.Combine(Path.GetDirectoryName(datasetsCsvPath)!, "measurements.csv");
using var measurementsCsv = File.OpenWrite(measurementsCsvPath);
measurementsCsv.Write("run_time,dataset,od,is_valid,size_lhs,size_rhs,mean_time,error_time,stddev_time,allocated\n"u8);

// Source: https://stackoverflow.com/a/33796861
using var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(datasetsCsvPath);
parser.SetDelimiters([","]);

// Skip header line
parser.ReadLine();

while (!parser.EndOfData)
{
    var fields = parser.ReadFields();
    if (fields is null || fields.Length != 6)
    {
        throw new Exception("Something went wrong.");
    }

    var dataset = new Dataset
    {
        Path = fields[0],
        NumberOfRows = int.Parse(fields[1]),
        NumberOfColumns = int.Parse(fields[2]),
        GroundTruthSize = int.Parse(fields[3]),
        NumberOfCompatibleOds = int.Parse(fields[4]),
        NumberOfConstantOds = int.Parse(fields[5]),
    };
    if (!dataset.Path.Contains("credit/")) continue;
    Console.WriteLine(dataset);
    var attributesPath = Path.Combine(Path.GetDirectoryName(datasetsCsvPath)!, "candidates",
        dataset.Path + ".attributes.txt");
    var attributes = File.ReadAllLines(attributesPath).Where(line => !string.IsNullOrWhiteSpace(line))
        .ToList();
    var worker = new BenchmarkRunner
    {
        Dataset = dataset,
        Attributes = attributes,
        BasePath = Path.GetDirectoryName(datasetsCsvPath)!,
    };
    var summaries = BitSet.WithSufficientWidth(attributes.Count, worker);
    foreach (var (od, (valid, summary)) in summaries)
    {
        measurementsCsv.Write(System.Text.Encoding.UTF8.GetBytes(
            $"""
             {runTime:o},"{dataset.Path}","{od}",{valid},{od.LeftHandSide.Count},{od.RightHandSide.Count},{summary.GetMeanTime().ToString(CultureInfo.InvariantCulture)},{summary.GetStdErrorTime().ToString(CultureInfo.InvariantCulture)},{summary.GetStdDevTime().ToString(CultureInfo.InvariantCulture)},{summary.GetAllocatedMemory()}
             
             """));
    }
}

return 0;

file class BenchmarkRunner : IBitSetOperation<Dictionary<ListBasedOrderDependency, (bool valid, Summary summary)>>
{
    public required Dataset Dataset { get; init; }
    public required List<string> Attributes { get; init; }
    public required string BasePath { get; init; }

    private string ValidsPath => Path.Combine(BasePath, "candidates", Dataset.Path + ".valids.txt");
    private string InvalidsPath => Path.Combine(BasePath, "candidates", Dataset.Path + ".invalids.txt");
    public Dictionary<ListBasedOrderDependency, (bool valid, Summary summary)> Work<TBitSet>() where TBitSet : IBitSet<TBitSet>
    {
        var attributesMap = new Dictionary<string, int>(Attributes.Count);
        var attributeIndex = 0;
        foreach (var attribute in Attributes)
        {
            attributesMap[attribute] = attributeIndex++;
        }
        var result = new Dictionary<ListBasedOrderDependency, (bool valid, Summary summary)>();
        RunFor(ValidsPath, true);
        RunFor(InvalidsPath, false);

        return result;
        void RunFor(string path, bool valid)
        {
            foreach (var validOd in File.ReadAllLines(path))
            {
                var od = ListBasedOrderDependency.Parse(attributesMap!, validOd);
                if (od is null) continue;
                File.WriteAllLines(CandidateBenchmark<TBitSet>.MagicPath, [BasePath, Dataset.Path, validOd]);
                var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<CandidateBenchmark<TBitSet>>();
                result!.Add(od.Value, (valid, summary));
            }
        }
    }
}

file readonly record struct Dataset
{
    public required string Path { get; init; }
    public required int NumberOfRows { get; init; }
    public required int NumberOfColumns { get; init; }
    public required int GroundTruthSize { get; init; }
    public required int NumberOfCompatibleOds { get; init; }
    public required int NumberOfConstantOds { get; init; }
}

file static class Extension
{
    public static double GetMeanTime(this Summary summary) => summary.Reports.Single().ResultStatistics!.Mean;
    public static double GetStdErrorTime(this Summary summary) => summary.Reports.Single().ResultStatistics!.StandardError;
    public static double GetStdDevTime(this Summary summary) => summary.Reports.Single().ResultStatistics!.StandardDeviation;
    public static long GetAllocatedMemory(this Summary summary) => summary.Reports.Single().GcStats.GetTotalAllocatedBytes(false)!.Value;
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> o) where T : struct
    {
        return o.Where(x => x is not null).Select(x => x!.Value);
    }
}