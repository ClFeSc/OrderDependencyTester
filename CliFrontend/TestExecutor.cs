using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
using OrderDependencyModels;
using Attribute = OrderDependencyModels.Attribute;

namespace CliFrontend;

public static class TestExecutor
{
    public static IEnumerable<KeyValuePair<ListBasedOrderDependency, bool>> TestDependencies(string setBasedPath, string listBasedPath,
        string attributesPath)
    {
        var knownDependencies = ISetBasedOrderDependency.Parse(setBasedPath);
        var testDependencies = ListBasedOrderDependency.Parse(listBasedPath);


        var compatiblesTree = new ColumnsTree<HashSet<OrderCompatibleDependency>>();
        foreach (var compatibleOd in knownDependencies.startingCompOds)
        {
            var set = compatiblesTree.Get(compatibleOd.Context) ?? new HashSet<OrderCompatibleDependency>();
            set.Add(compatibleOd);
            compatiblesTree.Add(set, compatibleOd.Context);
        }
        var attributes = File.ReadAllLines(attributesPath).Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => new Attribute(line)).ToList();

        var algo = new ListBasedOdAlgorithm
        {
            Constants = knownDependencies.startingCods,
            AllAttributes = attributes,
            CompatiblesTree = compatiblesTree
        };

        return algo.AreValid(testDependencies);
    }
}