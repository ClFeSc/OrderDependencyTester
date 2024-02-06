using BenchmarkDotNet.Attributes;
using BitSets;
using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
using OrderDependencyModels;

namespace OrderDependencyBenchmark;

[MemoryDiagnoser(false)]
public class CandidateBenchmark<TBitSet> where TBitSet : IBitSet<TBitSet>
{
    public const string MagicPath = "/tmp/583bf4a8ab0800e72129f69e8556251ec1b187e9-magic.txt";
    public ListBasedOrderDependency Candidate { get; set; }
    public ListBasedOdAlgorithm<TBitSet> Algorithm { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var lines = File.ReadAllLines(MagicPath);
        Console.WriteLine(string.Join(", ", lines));
        var basePath = Path.Combine("..", "..", "..", "..", "..", "..", "..", "..", lines[0]);
        var datasetPath = lines[1];
        var candidateString = lines[2];
        var attributesPath = Path.Combine(basePath, "candidates", datasetPath + ".attributes.txt");
        var attributes = File.ReadAllLines(attributesPath).Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        var attributesMap = new Dictionary<string, int>(attributes.Count);
        var attributeIndex = 0;
        foreach (var attribute in attributes)
        {
            attributesMap[attribute] = attributeIndex++;
        }
        
        var setBasedPath = Path.Combine(basePath, "results", datasetPath + ".txt");
        var knownDependencies = ISetBasedOrderDependency.Parse<TBitSet>(attributesMap, setBasedPath);

        var compatiblesTree = new ColumnsTree<OrderCompatibleDependency<TBitSet>, TBitSet>(attributes.Count);
        foreach (var compatibleOd in knownDependencies.startingCompOds)
        {
            compatiblesTree.Add(compatibleOd, compatibleOd.Context);
        }

        Algorithm = new ListBasedOdAlgorithm<TBitSet>
        {
            Constants = knownDependencies.startingCods,
            CompatiblesTree = compatiblesTree,
            NumberOfAttributes = attributes.Count
        };
        Candidate = ListBasedOrderDependency.Parse(attributesMap, candidateString)!.Value;
    }

    [Benchmark]
    public void Benchmark()
    {
        Algorithm.IsValid(Candidate);
    }
}