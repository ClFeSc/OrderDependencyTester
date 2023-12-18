using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace OrderDependencyModels;

public readonly partial record struct ConstantOrderDependency : ISetBasedOrderDependency
{
    public required BitArray Context { get; init; }
    public required int RightHandSide { get; init; }

    public override string ToString() => $"{{{string.Join(", ", Context)}}}: [] ↦ {RightHandSide}";

    public static bool TryParse(Dictionary<string,int> attributesMap, string spec, [NotNullWhen(true)] out ConstantOrderDependency? constantOrderDependency)
    {
        // parse line in format {A, B, C}: [] ↦ E as a ConstantOrderDependency
        var match = ConstantOdRegex().Match(spec);
        if (!match.Success)
        {
            constantOrderDependency = null;
            return false;
        }

        var contextIndices = match.Groups[1].Value.Split(", ").Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x => attributesMap[x]);
        var context = new BitArray(attributesMap.Count);
        foreach (var contextIndex in contextIndices)
        {
            context.Set(contextIndex, true);
        }
        var rhs = attributesMap[match.Groups[2].Value];
        constantOrderDependency = new ConstantOrderDependency
        {
            Context = context,
            RightHandSide = rhs
        };
        return true;
    }

    [GeneratedRegex("{(.*?)}: \\[\\] ↦ (.+)")]
    private static partial Regex ConstantOdRegex();
}
