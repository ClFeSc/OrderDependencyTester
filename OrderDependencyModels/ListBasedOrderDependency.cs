using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct ListBasedOrderDependency : IListBasedOrderDependency
{
    public required List<OrderSpecification> LeftHandSide { get; init; }
    public required List<OrderSpecification> RightHandSide { get; init; }

    public override string ToString() => $"{string.Join(", ", LeftHandSide)} -> {string.Join(", ", RightHandSide)}";

    public static List<ListBasedOrderDependency> Parse(string filename)
    {
        var list = new List<ListBasedOrderDependency>();
        foreach (var line in File.ReadAllLines(filename))
        {
            var match = ListBasedOdRegex().Match(line);
            if (!match.Success) continue;
            var lhs = match.Groups[1].Value.Split(",").Select(OrderSpecification.Parse).ToList();
            var rhs = match.Groups[2].Value.Split(",").Select(OrderSpecification.Parse).ToList();
            list.Add(new ListBasedOrderDependency
            {
                LeftHandSide = lhs,
                RightHandSide = rhs,
            });
        }
        return list;
    }

    [GeneratedRegex(@"\[(.+)\] -> \[(.+)\]")]
    private static partial Regex ListBasedOdRegex();
}
