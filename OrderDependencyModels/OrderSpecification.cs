namespace OrderDependencyModels;

public readonly partial record struct OrderSpecification
{
    public required Attribute Attribute { get; init; }
    public required OrderDirection Direction { get; init; }

    public override string ToString() => $"{Attribute}{OrderDirectionHelper.ToString(Direction)}";

    public static OrderSpecification Parse(string spec)
    {

        var lastChar = spec.Last();

        var attribute = new Attribute(lastChar switch
        {
            '↓' => spec.Remove(spec.Length - 1),
            '↑' => spec.Remove(spec.Length - 1),
            _ => spec
        });
        
        var direction = lastChar switch
        {
            '↓' => OrderDirection.Descending,
            _ => OrderDirection.Ascending
        };
        return new OrderSpecification
        {
            Attribute = attribute,
            Direction = direction
        };
    }

    public OrderSpecification Reverse() => this with
    {
        Direction = Direction is OrderDirection.Ascending ? OrderDirection.Descending : OrderDirection.Ascending,
    };
}
