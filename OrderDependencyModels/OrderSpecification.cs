using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct OrderSpecification(Attribute Attribute, OrderDirection Direction)
{
    public static OrderSpecification Parse(string spec)
    {
        var match = OrderSpecRegex().Match(spec);
        if (!match.Success)
        {
            throw new Exception("Invalid order specification");
        }

        var attribute = new Attribute(match.Groups[1].Value);
        var direction = match.Groups[2].Value switch
        {
            "↓" => OrderDirection.Descending,
            _ => OrderDirection.Ascending
        };
        return new OrderSpecification(attribute, direction);
    }

    [GeneratedRegex("(.*)(↑|↓)?")]
    private static partial Regex OrderSpecRegex();
}