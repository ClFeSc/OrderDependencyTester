using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace OrderDependencyModels;

public readonly record struct FunctionalDependency
{
    public required BitArray Lhs { get; init; }
    public required BitArray Rhs { get; init; }

    [SetsRequiredMembers]
    public FunctionalDependency(BitArray lhs, BitArray rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    [SetsRequiredMembers]
    public FunctionalDependency(int lhs, int rhs, int columnCount)
    {
        Lhs = new BitArray(columnCount);
        Rhs = new BitArray(columnCount);
        Lhs.Set(lhs, true);
        Rhs.Set(rhs, true);
    }

    private static BitArray BitArraySetAt(int index, int size)
    {
        var bitArray = new BitArray(size);
        bitArray.Set(index, true);
        return bitArray;
    }

    public static FunctionalDependency FromConstantOrderDependency(ConstantOrderDependency od) => new()
    {
        Lhs = od.Context,
        Rhs = BitArraySetAt(od.RightHandSide, od.Context.Count),
    };

    public override string ToString() => $"{(Lhs.Count > 0 ? string.Join(", ", Lhs) : "\u2205")} -> {string.Join(", ", Rhs)}";
}
