using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct ConstantOrderDependency : ISetBasedOrderDependency
{
    public required HashSet<Attribute> Context { get; init; }
    public required Attribute RightHandSide { get; init; }

    public override string ToString() => $"{{{string.Join(", ", Context)}}}: [] ↦ {RightHandSide}";

    public static bool TryParse(string spec, [NotNullWhen(true)] out ConstantOrderDependency? constantOrderDependency)
    {
        // parse line in format {A, B, C}: [] ↦ E as a ConstantOrderDependency
        var match = ConstantOdRegex().Match(spec);
        if (!match.Success)
        {
            constantOrderDependency = null;
            return false;
        }

        var context = match.Groups[1].Value.Split(", ").Select(x => new Attribute(x));
        var rhs = new Attribute(match.Groups[2].Value);
        constantOrderDependency = new ConstantOrderDependency
        {
            Context = new HashSet<Attribute>(context),
            RightHandSide = rhs
        };
        return true;
    }

    [GeneratedRegex("{(.*)}: \\[\\] ↦ (.+)")]
    private static partial Regex ConstantOdRegex();
}
