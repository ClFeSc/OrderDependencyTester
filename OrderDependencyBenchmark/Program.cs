using BenchmarkDotNet.Reports;
using BitSets;
using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
using OrderDependencyBenchmark;
using OrderDependencyModels;

if (args.Length != 1)
{
    Console.Error.WriteLine($"Expected 1 argument, got {args.Length}.");
    Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} [candidate.csv path: string]");
    return 1;
}

var datasetsCsvPath = args[0];

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
    BitSet.WithSufficientWidth(attributes.Count, worker);
}

return 0;

file class BenchmarkRunner : IBitSetOperation<Dictionary<ListBasedOrderDependency, (bool valid, Summary summary)>>
{
    public required Dataset Dataset { get; init; }
    public required List<string> Attributes { get; init; }
    public required string BasePath { get; init; }

    private string ValidsPath => Path.Combine(BasePath, "candidates", Dataset.Path + ".valids.txt");
    private string InvalidsPath => Path.Combine(BasePath, "candidates", Dataset.Path + ".invalids.txt");
    // private string SetBasedPath => Path.Combine(BasePath, "results", Dataset.Path + ".txt");
    public Dictionary<ListBasedOrderDependency, (bool valid, Summary summary)> Work<TBitSet>() where TBitSet : IBitSet<TBitSet>
    {
        var attributesMap = new Dictionary<string, int>(Attributes.Count);
        var attributeIndex = 0;
        foreach (var attribute in Attributes)
        {
            attributesMap[attribute] = attributeIndex++;
        }
        var result = new Dictionary<ListBasedOrderDependency, (bool valid, Summary summary)>();
        foreach (var validOd in File.ReadAllLines(ValidsPath))
        {
            var od = ListBasedOrderDependency.Parse(attributesMap, validOd);
            if (od is null) continue;
            File.WriteAllLines(CandidateBenchmark<TBitSet>.MagicPath, [BasePath, Dataset.Path, validOd]);
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<CandidateBenchmark<TBitSet>>();
            result.Add(od.Value, (true, summary));
        }

        return result;
        // var attributesMap = new Dictionary<string, int>(Attributes.Count);
        // var attributeIndex = 0;
        // foreach (var attribute in Attributes)
        // {
        //     attributesMap[attribute] = attributeIndex++;
        // }
        //
        // var knownDependencies = ISetBasedOrderDependency.Parse<TBitSet>(attributesMap, SetBasedPath);
        //
        // var compatiblesTree = new ColumnsTree<OrderCompatibleDependency<TBitSet>, TBitSet>(Attributes.Count);
        // foreach (var compatibleOd in knownDependencies.startingCompOds)
        // {
        //     compatiblesTree.Add(compatibleOd, compatibleOd.Context);
        // }
        //
        // var algorithm = new ListBasedOdAlgorithm<TBitSet>
        // {
        //     Constants = knownDependencies.startingCods,
        //     CompatiblesTree = compatiblesTree,
        //     NumberOfAttributes = Attributes.Count
        // };
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