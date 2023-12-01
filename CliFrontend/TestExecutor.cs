using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
using OrderDependencyModels;

namespace CliFrontend;

public static class TestExecutor
{
    public static IEnumerable<KeyValuePair<ListBasedOrderDependency, bool>> TestDependencies(string setBasedPath, string listBasedPath,
        string attributesPath)
    {

        var attributes = File.ReadAllLines(attributesPath).Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var attributesMap = new Dictionary<string, int>(attributes.Count);
        var attributeIndex = 0;
        foreach (var attribute in attributes)
        {
            attributesMap[attribute] = attributeIndex++;
        }

        var knownDependencies = ISetBasedOrderDependency.Parse(attributesMap,setBasedPath);
        var testDependencies = ListBasedOrderDependency.Parse(attributesMap,listBasedPath);

        if (testDependencies.Count == 0)
        {
            Console.Error.WriteLine($"Warning: No ODs are being tested. Check your input data.");
        }


        var compatiblesTree = new ColumnsTree<HashSet<OrderCompatibleDependency>>(attributes.Count);
        foreach (var compatibleOd in knownDependencies.startingCompOds)
        {
            var set = compatiblesTree.Get(compatibleOd.Context) ?? new HashSet<OrderCompatibleDependency>();
            set.Add(compatibleOd);
            compatiblesTree.Add(set, compatibleOd.Context);
        }

        var algo = new ListBasedOdAlgorithm
        {
            Constants = knownDependencies.startingCods,
            NumAttributes = attributes.Count,
            CompatiblesTree = compatiblesTree
        };

        return algo.AreValid(testDependencies);
    }
}