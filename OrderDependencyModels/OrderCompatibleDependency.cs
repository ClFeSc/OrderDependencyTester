using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct OrderCompatibleDependency : IEnumerable<OrderSpecification>, ISetBasedOrderDependency
{
    public required HashSet<Attribute> Context { get; init; }
    private readonly HashSet<OrderSpecification> _sides;
    public required OrderSpecification Lhs
    {
        init => _sides.Add(value);
    }
    public required OrderSpecification Rhs
    {
        init => _sides.Add(value);
    }

    private IEnumerable<OrderSpecification> Sides
    {
        init => _sides = new HashSet<OrderSpecification>(value);
    }

    public OrderCompatibleDependency()
    {
        _sides = new HashSet<OrderSpecification>(2);
    }

    public override string ToString()
    {
        var sides = _sides.ToList();
        return $"{{{string.Join(", ", Context)}}}: {sides[0]} ~ {sides[1]}";
    }

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
        orderCompatibleDependency = new OrderCompatibleDependency
        {
            Context = new HashSet<Attribute>(context),
            Lhs = lhs,
            Rhs = rhs,
        };
        return true;
    }

    [GeneratedRegex("{(.+)}: (.+) ~ (.+)")]
    private static partial Regex OrderCompatibleRegex();

    public OrderCompatibleDependency Reverse() => this with
    {
        Sides = _sides.Select(os => os.Reverse()),
    };

    public IEnumerator<OrderSpecification> GetEnumerator() => _sides.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
