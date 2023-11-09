using DependencyTester.FdMembershipTester;
using OrderDependencyModels;
using Attribute = OrderDependencyModels.Attribute;

// var knownDependencies = ISetBasedOrderDependency.Parse(args[0]);
// var testDependencies = ListBasedOrderDependency.Parse(args[1]);

var attributes = new List<Attribute>
{
    new("A"),
    new("B"),
    new("C"),
};
var fds = new List<FunctionalDependency>
{
    new(new HashSet<Attribute>(attributes[0].Yield()), new HashSet<Attribute>(attributes[1].Yield())),
    new(new HashSet<Attribute>(attributes[1].Yield()), new HashSet<Attribute>(attributes[2].Yield())),
};
var fdUnderTest = new FunctionalDependency(new HashSet<Attribute>(attributes[0].Yield()),
    new HashSet<Attribute>(attributes[2].Yield()));

var isValid = FdMembershipAlgorithm.IsValid(fdUnderTest, fds, attributes);

// TODO: add example for ListBasedOdAlgorithm
Console.WriteLine(isValid);

file static class Utils
{
    public static IEnumerable<T> Yield<T>(this T item) => new[] { item };
}
