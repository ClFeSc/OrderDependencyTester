using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct OrderSpecification
{
    public required Attribute Attribute { get; init; }
    public required OrderDirection Direction { get; init; }

    public override string ToString() => $"{Attribute}{OrderDirectionHelper.ToString(Direction)}";

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
        return new OrderSpecification
        {
            Attribute = attribute,
            Direction = direction
        };
    }

    [GeneratedRegex("(.*)(↑|↓)?")]
    private static partial Regex OrderSpecRegex();

    public OrderSpecification Reverse() => this with
    {
        Direction = Direction is OrderDirection.Ascending ? OrderDirection.Descending : OrderDirection.Ascending,
    };
}
