using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Reports;
using BitSets;
using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
using OrderDependencyBenchmark;
using OrderDependencyModels;

if (args.Length != 3)
{
    Console.Error.WriteLine($"Expected 3 argument, got {args.Length}.");
    Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} [datasets.csv path: string] [runs.csv path: string] [measurements.csv path: string]");
    return 1;
}

var datasetsCsvPath = args[0];
var runsCsvPath = args[1];
var measurementsCsvPath = args[2];
if (File.Exists(runsCsvPath) == false)
{
    File.WriteAllLines(runsCsvPath, ["time,algorithm,machine_name"]);
}

var runTime = DateTime.UtcNow;
File.AppendAllLines(runsCsvPath, [$"{runTime:o},C#-Axioms,{Environment.MachineName}"]);
if (File.Exists(measurementsCsvPath) == false)
{
    File.WriteAllLines(measurementsCsvPath, ["run_time,dataset,od,is_valid,size_lhs,size_rhs,mean_time,min_time,quart25_time,median_time,quart75_time,max_time"]);
}

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
    // if (!dataset.Path.Contains("credit/")) continue;
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
    var observations = BitSet.WithSufficientWidth(attributes.Count, worker);
    foreach (var (od, (valid, observation)) in observations)
    {
        File.AppendAllLines(measurementsCsvPath, [
            $"{runTime:o},\"{dataset.Path}\",\"{od}\",{valid},{od.LeftHandSide.Count},{od.RightHandSide.Count},{observation.Mean},{observation.Min},{observation.Quantile25},{observation.Median},{observation.Quantile75},{observation.Max}"
        ]);
    }
}

return 0;

file class BenchmarkRunner : IBitSetOperation<Dictionary<ListBasedOrderDependency, (bool valid, Observation observation)>>
{
    public required Dataset Dataset { get; init; }
    public required List<string> Attributes { get; init; }
    public required string BasePath { get; init; }

    private string ValidsPath => Path.Combine(BasePath, "candidates", Dataset.Path + ".valids.txt");
    private string InvalidsPath => Path.Combine(BasePath, "candidates", Dataset.Path + ".invalids.txt");
    public Dictionary<ListBasedOrderDependency, (bool valid, Observation observation)> Work<TBitSet>() where TBitSet : IBitSet<TBitSet>
    {
        var attributesMap = new Dictionary<string, int>(Attributes.Count);
        var attributeIndex = 0;
        foreach (var attribute in Attributes)
        {
            attributesMap[attribute] = attributeIndex++;
        }

        var algo = new Func<ListBasedOdAlgorithm<TBitSet>>(() =>
        {
            var setBasedPath = Path.Combine(BasePath, "results", Dataset.Path + ".txt");
            var knownDependencies = ISetBasedOrderDependency.Parse<TBitSet>(attributesMap, setBasedPath);

            var compatiblesTree = new ColumnsTree<OrderCompatibleDependency<TBitSet>, TBitSet>(Attributes.Count);
            foreach (var compatibleOd in knownDependencies.startingCompOds)
            {
                compatiblesTree.Add(compatibleOd, compatibleOd.Context);
            }

            var algorithm = new ListBasedOdAlgorithm<TBitSet>
            {
                Constants = knownDependencies.startingCods,
                CompatiblesTree = compatiblesTree,
                NumberOfAttributes = Attributes.Count
            };
            return algorithm;
        })();
        // var result = new Dictionary<ListBasedOrderDependency, (bool valid, Observation observation)>();
        List<(bool valid, ListBasedOrderDependency od)> ods = File.ReadAllLines(ValidsPath)
            .Select(validOd => (true, ListBasedOrderDependency.Parse(attributesMap, validOd)))
            .Concat(File.ReadAllLines(InvalidsPath)
                .Select(validOd => (false, ListBasedOrderDependency.Parse(attributesMap, validOd))))
            .Where(entry => entry.Item2 is not null).Select(entry => (entry.Item1, entry.Item2!.Value)).ToList();
        var odResults = ods.Select(entry =>
            new KeyValuePair<ListBasedOrderDependency, List<TimeSpan>>(entry.od, new List<TimeSpan>())).ToDictionary();
        const int iterationCount = 1000;
        for (var i = 0; i < iterationCount; i++)
        {
            Console.Error.WriteLine($"Iteration {i} of {iterationCount}");
            // Shuffle ODs
            Random.Shared.Shuffle(CollectionsMarshal.AsSpan(ods));
            // Run each OD
            foreach (var (_, od) in ods)
            {
                var sw = new Stopwatch();
                sw.Start();
                algo.IsValid(od);
                sw.Stop();
                odResults[od].Add(sw.Elapsed);
            }
        }

        var result = ods.Select(entry =>
            new KeyValuePair<ListBasedOrderDependency, (bool valid, Observation observation)>(entry.od,
                (entry.valid, Observation.FromResults(odResults[entry.od])))).ToDictionary();

        return result;
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

file record Observation
{
    public required TimeSpan Min { get; init; }
    public required TimeSpan Quantile25 { get; init; }
    public required TimeSpan Mean { get; init; }
    public required TimeSpan Median { get; init; }
    public required TimeSpan Quantile75 { get; init; }
    public required TimeSpan Max { get; init; }

    public static Observation FromResults(IEnumerable<TimeSpan> results)
    {
        var list = results.ToList();
        list.Sort();
        var mean = TimeSpan.FromTicks((long)list.Average(timeSpan => timeSpan.Ticks));
        var min = list[0];
        var quantile25 = list[(int)(list.Count * 0.25)];
        var median = list[(int)(list.Count * 0.5)];
        var quantile75 = list[(int)(list.Count * 0.75)];
        var max = list[^1];
        return new Observation
        {
            Max = max,
            Mean = mean,
            Median = median,
            Min = min,
            Quantile25 = quantile25,
            Quantile75 = quantile75,
        };
    }
}

file static class Extension
{
    public static double GetMeanTime(this Summary summary) => summary.Reports.Single().ResultStatistics!.Mean;
    public static double GetStdErrorTime(this Summary summary) => summary.Reports.Single().ResultStatistics!.StandardError;
    public static double GetStdDevTime(this Summary summary) => summary.Reports.Single().ResultStatistics!.StandardDeviation;
    public static long GetAllocatedMemory(this Summary summary) => summary.Reports.Single().GcStats.GetTotalAllocatedBytes(false)!.Value;
}