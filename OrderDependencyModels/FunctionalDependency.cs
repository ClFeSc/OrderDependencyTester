using System.Diagnostics.CodeAnalysis;

namespace OrderDependencyModels;

public readonly record struct FunctionalDependency
{
    public required HashSet<Attribute> Lhs { get; init; }
    public required HashSet<Attribute> Rhs { get; init; }

    [SetsRequiredMembers]
    public FunctionalDependency(HashSet<Attribute> determinant, HashSet<Attribute> dependent)
    {
        Lhs = determinant;
        Rhs = dependent;
    }

    [SetsRequiredMembers]
    public FunctionalDependency(Attribute lhs, Attribute rhs)
    {
        Lhs = new HashSet<Attribute> {lhs};
        Rhs = new HashSet<Attribute> {rhs};
    }

    public static FunctionalDependency FromConstantOrderDependency(in ConstantOrderDependency od) => new()
    {
        Lhs = od.Context,
        Rhs = new HashSet<Attribute> {od.RightHandSide}
    };

    public override string ToString() => $"{(Lhs.Count > 0 ? string.Join(", ", Lhs) : "\u2205")} -> {string.Join(", ", Rhs)}";
}
