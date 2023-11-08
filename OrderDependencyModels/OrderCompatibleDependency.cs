using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct OrderCompatibleDependency(HashSet<Attribute> Context, OrderSpecification LeftHandSide,
    OrderSpecification RightHandSide) : ISetBasedOrderDependency
{
    public static bool TryParse(string spec, [NotNullWhen(true)] out OrderCompatibleDependency? orderCompatibleDependency)
    {
        // parse line in format {D, F, H, I}: B↑ ~ E↓ as a OrderCompatibleDependency
        var match = OrderCompatibleRegex().Match(spec);
        if (!match.Success)
        {
            orderCompatibleDependency = null;
            return false;
        }

        var context = match.Groups[1].Value.Split(", ").Select(x => new Attribute(x));
        var lhs = OrderSpecification.Parse(match.Groups[2].Value);
        var rhs = OrderSpecification.Parse(match.Groups[3].Value);
        orderCompatibleDependency = new OrderCompatibleDependency(new HashSet<Attribute>(context), lhs, rhs);
        return true;
    }

    [GeneratedRegex("{(.+)}: (.+) ~ (.+)")]
    private static partial Regex OrderCompatibleRegex();
}