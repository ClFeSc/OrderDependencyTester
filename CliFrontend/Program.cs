using DependencyTester;
using DependencyTester.OdMembershipTester;
using OrderDependencyModels;
using Attribute = OrderDependencyModels.Attribute;

if (args.Length != 3)
{
    Console.Error.WriteLine($"Expected 3 arguments, got {args.Length}.");
    Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} [known set based dependency path: string] [list based dependencies to test path: string] [attributes path: string]");
    return 1;
}

var knownDependencies = ISetBasedOrderDependency.Parse(args[0]);
var testDependencies = ListBasedOrderDependency.Parse(args[1]);


var compatiblesTree = new ColumnsTree<HashSet<OrderCompatibleDependency>>();
foreach (var compatibleOd in knownDependencies.startingCompOds)
{
    var set = compatiblesTree.Get(compatibleOd.Context) ?? new HashSet<OrderCompatibleDependency>();
    set.Add(compatibleOd);
    compatiblesTree.Add(set, compatibleOd.Context);
}
var attributes = File.ReadAllLines(args[2]).Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line => new Attribute(line)).ToList();

foreach (var dependencyToTest in testDependencies)
{
    var isValid = ListBasedOdAlgorithm.IsValid(dependencyToTest, knownDependencies.startingCods,
        compatiblesTree, attributes);
    Console.WriteLine($"OD {dependencyToTest} is {(isValid ? "" : "not ")}valid");
}

return 0;
//
// var attributes = new List<Attribute>
// {
//     new("A"),
//     new("B"),
//     new("C"),
// };
// var fds = new List<FunctionalDependency>
// {
//     new(new HashSet<Attribute>(attributes[0].Yield()), new HashSet<Attribute>(attributes[1].Yield())),
//     new(new HashSet<Attribute>(attributes[1].Yield()), new HashSet<Attribute>(attributes[2].Yield())),
// };
// var fdUnderTest = new FunctionalDependency(new HashSet<Attribute>(attributes[0].Yield()),
//     new HashSet<Attribute>(attributes[2].Yield()));
//
// var isValid = FdMembershipAlgorithm.IsValid(fdUnderTest, fds, attributes);
//
// // TODO: add example for ListBasedOdAlgorithm
// Console.WriteLine(isValid);
//
// file static class Utils
// {
//     public static IEnumerable<T> Yield<T>(this T item) => new[] { item };
// }
