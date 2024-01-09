namespace BitSets;

public interface IBitSetOperation<out TResult>
{
    TResult Work<TBitSet>() where TBitSet : IBitSet<TBitSet>;
}

public static class BitSet
{
    public static TResult WithSufficientWidth<TResult>(int requiredWidth, IBitSetOperation<TResult> operation) =>
        requiredWidth switch
        {
            <= 8 => operation.Work<IntegerBitSet<byte>>(),
            <= 16 => operation.Work<IntegerBitSet<ushort>>(),
            <= 32 => operation.Work<IntegerBitSet<uint>>(),
            <= 64 => operation.Work<IntegerBitSet<ulong>>(),
            <= 128 => operation.Work<IntegerBitSet<UInt128>>(),
            _ => operation.Work<BitArrayWrapper>(),
        };
}
