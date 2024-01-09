using BitSets;
using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
using OrderDependencyModels;

namespace CliFrontend;

public static class TestExecutor
{
    private class SpecificTester : IBitSetOperation<IEnumerable<KeyValuePair<ListBasedOrderDependency, bool>>>
    {
        public required string SetBasedPath { get; init; }
        public required string ListBasedPath { get; init; }
        public required List<string> Attributes { get; init; }
        public IEnumerable<KeyValuePair<ListBasedOrderDependency, bool>> Work<TBitSet>() where TBitSet : IBitSet<TBitSet>
        {
            var attributesMap = new Dictionary<string, int>(Attributes.Count);
            var attributeIndex = 0;
            foreach (var attribute in Attributes)
            {
                attributesMap[attribute] = attributeIndex++;
            }

            var knownDependencies = ISetBasedOrderDependency.Parse<TBitSet>(attributesMap, SetBasedPath);
            var testDependencies = ListBasedOrderDependency.Parse(attributesMap, ListBasedPath);

            if (testDependencies.Count == 0)
            {
                Console.Error.WriteLine($"Warning: No ODs are being tested. Check your input data.");
            }


            var compatiblesTree = new ColumnsTree<OrderCompatibleDependency<TBitSet>, TBitSet>(Attributes.Count);
            foreach (var compatibleOd in knownDependencies.startingCompOds)
            {
                compatiblesTree.Add(compatibleOd, compatibleOd.Context);
            }

            // var algo = new ListBasedOdAlgorithm(knownDependencies.startingCods,compatiblesTree,attributes.Count);
            var algo = new ListBasedOdAlgorithm<TBitSet>
            {
                Constants = knownDependencies.startingCods,
                CompatiblesTree = compatiblesTree,
                NumberOfAttributes = Attributes.Count
            };

            return algo.AreValid(testDependencies);
        }
    }
    public static IEnumerable<KeyValuePair<ListBasedOrderDependency, bool>> TestDependencies(string setBasedPath, string listBasedPath,
        string attributesPath)
    {
        var attributes = File.ReadAllLines(attributesPath).Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var worker = new SpecificTester
        {
            Attributes = attributes,
            ListBasedPath = listBasedPath,
            SetBasedPath = setBasedPath
        };

        return BitSet.WithSufficientWidth(attributes.Count, worker);
    }
}
