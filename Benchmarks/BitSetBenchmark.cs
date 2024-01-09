using BenchmarkDotNet.Attributes;
using BitSets;
using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
using OrderDependencyModels;

namespace Benchmarks;

[MemoryDiagnoser(false)]
public class BitSetBenchmark
{
    private object? _result = null;
    private static string Prefix => "../../../../../../../../";
    private static string SetBasedPath { get; } = $"{Prefix}/testdata/minimal1/ground-truth.csv";
    private static string ListBasedValidPath { get; } = $"{Prefix}/testdata/minimal1/to-test-valid.unique.txt";
    private static string AttributesPath { get; } = $"{Prefix}/testdata/minimal1/attributes.txt";

    private AlgoWrapper<IntegerBitSet<byte>> AlgoByte { get; } = new()
    {
        AttributesPath = AttributesPath,
        ListBasedPath = ListBasedValidPath,
        SetBasedPath = SetBasedPath
    };

    private AlgoWrapper<IntegerBitSet<uint>> AlgoUint { get; } = new()
    {
        AttributesPath = AttributesPath,
        ListBasedPath = ListBasedValidPath,
        SetBasedPath = SetBasedPath
    };

    private AlgoWrapper<IntegerBitSet<UInt128>> AlgoUint128 { get; } = new()
    {
        AttributesPath = AttributesPath,
        ListBasedPath = ListBasedValidPath,
        SetBasedPath = SetBasedPath
    };

    private AlgoWrapper<BitArrayWrapper> AlgoBitArray { get; } = new()
    {
        AttributesPath = AttributesPath,
        ListBasedPath = ListBasedValidPath,
        SetBasedPath = SetBasedPath
    };

    [IterationSetup]
    public void Setup()
    {
        AlgoByte.Setup();
        AlgoUint.Setup();
        AlgoUint128.Setup();
        AlgoBitArray.Setup();
    }

    [Benchmark]
    public void UsingByte()
    {
        _result = AlgoByte.Algo!.AreValid(AlgoByte.TestDependencies!).ToList();
    }

    [Benchmark]
    public void UsingUint()
    {
        _result = AlgoUint.Algo!.AreValid(AlgoUint.TestDependencies!).ToList();
    }

    [Benchmark]
    public void UsingUint128()
    {
        _result = AlgoUint128.Algo!.AreValid(AlgoUint128.TestDependencies!).ToList();
    }

    [Benchmark]
    public void UsingBitArray()
    {
        _result = AlgoBitArray.Algo!.AreValid(AlgoBitArray.TestDependencies!).ToList();
    }
}

internal class AlgoWrapper<TBitSet> where TBitSet : IBitSet<TBitSet>
{
    public required string SetBasedPath { get; init; }
    public required string ListBasedPath { get; init; }
    public required string AttributesPath { get; init; }

    public ListBasedOdAlgorithm<TBitSet>? Algo { get; private set; }
    public List<ListBasedOrderDependency>? TestDependencies { get; private set; }
    public void Setup()
    {
        var attributes = File.ReadAllLines(AttributesPath).Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var attributesMap = new Dictionary<string, int>(attributes.Count);
        var attributeIndex = 0;
        foreach (var attribute in attributes)
        {
            attributesMap[attribute] = attributeIndex++;
        }

        var knownDependencies = ISetBasedOrderDependency.Parse<TBitSet>(attributesMap, SetBasedPath);
        TestDependencies = ListBasedOrderDependency.Parse(attributesMap, ListBasedPath);

        if (TestDependencies.Count == 0)
        {
            Console.Error.WriteLine($"Warning: No ODs are being tested. Check your input data.");
        }


        var compatiblesTree = new ColumnsTree<OrderCompatibleDependency<TBitSet>, TBitSet>(attributes.Count);
        foreach (var compatibleOd in knownDependencies.startingCompOds)
        {
            compatiblesTree.Add(compatibleOd, compatibleOd.Context);
        }

        Algo = new ListBasedOdAlgorithm<TBitSet>
        {
            Constants = knownDependencies.startingCods,
            CompatiblesTree = compatiblesTree,
            NumberOfAttributes = attributes.Count
        };
    }
}
