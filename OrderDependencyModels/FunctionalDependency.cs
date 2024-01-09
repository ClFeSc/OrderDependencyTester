using System.Diagnostics.CodeAnalysis;
using BitSets;

namespace OrderDependencyModels;

public readonly record struct FunctionalDependency<TBitSet> : ISetBasedOrderDependency where TBitSet : IBitSet<TBitSet>
{
    public required TBitSet Lhs { get; init; }
    public required TBitSet Rhs { get; init; }

    [SetsRequiredMembers]
    public FunctionalDependency(TBitSet lhs, TBitSet rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    [SetsRequiredMembers]
    public FunctionalDependency(int lhs, int rhs, int columnCount)
    {
        Lhs = TBitSet.Create(columnCount);
        Rhs = TBitSet.Create(columnCount);
        Lhs.Set(lhs);
        Rhs.Set(rhs);
    }

    private static TBitSet BitArraySetAt(int index, int size)
    {
        var bitArray = TBitSet.Create(size);
        bitArray.Set(index);
        return bitArray;
    }

    public static FunctionalDependency<TBitSet> FromConstantOrderDependency(ConstantOrderDependency<TBitSet> od) => new()
    {
        Lhs = od.Context,
        Rhs = BitArraySetAt(od.RightHandSide, od.Context.Count),
    };

    public override string ToString() => $"{(Lhs.Count > 0 ? string.Join(", ", Lhs) : "\u2205")} -> {string.Join(", ", Rhs)}";
}
