using System.Diagnostics.CodeAnalysis;

namespace OrderDependencyModels;

public readonly record struct FunctionalDependency
{
    public required HashSet<int> Lhs { get; init; }
    public required HashSet<int> Rhs { get; init; }

    [SetsRequiredMembers]
    public FunctionalDependency(HashSet<int> lhs, HashSet<int> rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    [SetsRequiredMembers]
    public FunctionalDependency(int lhs, int rhs)
    {
        Lhs = new HashSet<int> {lhs};
        Rhs = new HashSet<int> {rhs};
    }

    public static FunctionalDependency FromConstantOrderDependency(ConstantOrderDependency od) => new()
    {
        Lhs = od.Context,
        Rhs = new HashSet<int> {od.RightHandSide}
    };

    public override string ToString() => $"{(Lhs.Count > 0 ? string.Join(", ", Lhs) : "\u2205")} -> {string.Join(", ", Rhs)}";
}
