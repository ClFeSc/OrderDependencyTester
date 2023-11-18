using DependencyTester;
using DependencyTester.ListBasedOdMembershipTester;
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

var algo = new ListBasedOdAlgorithm
{
    Constants = knownDependencies.startingCods,
    AllAttributes = attributes,
    CompatiblesTree = compatiblesTree
};

foreach (var dependencyToTest in testDependencies)
{
    var isValid = algo.IsValid(dependencyToTest);
    Console.WriteLine($"OD {dependencyToTest} is {(isValid ? "" : "not ")}valid");
}

return 0;
