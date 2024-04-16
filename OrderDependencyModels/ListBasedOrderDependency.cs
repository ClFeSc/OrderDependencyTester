using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct ListBasedOrderDependency : IListBasedOrderDependency
{
    public required List<OrderSpecification> LeftHandSide { get; init; }
    public required List<OrderSpecification> RightHandSide { get; init; }

    public override string ToString() => $"[{string.Join(",", LeftHandSide)}] -> [{string.Join(",", RightHandSide)}]";

    public static ListBasedOrderDependency? Parse(Dictionary<string, int> attributesMap, string representation)
    {
        var parseOrderSpec = (string x) => OrderSpecification.Parse(attributesMap, x);
        var match = ListBasedOdRegex().Match(representation);
        if (!match.Success) return null;
        var lhs = match.Groups[1].Value.Split(",").Select(parseOrderSpec).ToList();
        var rhs = match.Groups[2].Value.Split(",").Select(parseOrderSpec).ToList();
        return new ListBasedOrderDependency
        {
            LeftHandSide = lhs,
            RightHandSide = rhs,
        };
    }

    public static IEnumerable<ListBasedOrderDependency>
        ParseFromFile(Dictionary<string, int> attributesMap, string filename) => File.ReadAllLines(filename)
        .Select(line => Parse(attributesMap, line))
        .Where(od => od is not null)
        .Select(od => od!.Value);

    [GeneratedRegex(@"\[(.+)\] -> \[(.+)\]")]
    private static partial Regex ListBasedOdRegex();
}
