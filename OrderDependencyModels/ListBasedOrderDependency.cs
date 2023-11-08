using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct ListBasedOrderDependency(List<OrderSpecification> LeftHandSide,
    List<OrderSpecification> RightHandSide) : IListBasedOrderDependency
{
    public static List<ListBasedOrderDependency> Parse(string filename)
    {
        var list = new List<ListBasedOrderDependency>();
        foreach (var line in File.ReadAllLines(filename))
        {
            var match = ListBasedOdRegex().Match(line);
            if (!match.Success) continue;
            var lhs = match.Groups[1].Value.Split(",").Select(x => OrderSpecification.Parse(x)).ToList();
            var rhs = match.Groups[2].Value.Split(",").Select(x => OrderSpecification.Parse(x)).ToList();
            list.Add(new ListBasedOrderDependency(lhs, rhs));
        }
        return list;
    }

    [GeneratedRegex(@"\[(.+)\] -> \[(.+)\]")]
    private static partial Regex ListBasedOdRegex();
}