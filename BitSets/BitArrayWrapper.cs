using System.Collections;

namespace BitSets;

public class BitArrayWrapper : IBitSet<BitArrayWrapper>
{
    private BitArrayWrapper(int count)
    {
        Count = count;
        _bitArray = new BitArray(count);
    }

    private BitArrayWrapper(BitArray bitArray)
    {
        Count = bitArray.Count;
        _bitArray = bitArray;
    }
    private BitArray _bitArray;

    public static BitArrayWrapper operator &(BitArrayWrapper left, BitArrayWrapper right) =>
        new(new BitArray(left._bitArray).And(right._bitArray));

    public static BitArrayWrapper operator |(BitArrayWrapper left, BitArrayWrapper right) =>
        new(new BitArray(left._bitArray).Or(right._bitArray));

    public static BitArrayWrapper operator ^(BitArrayWrapper left, BitArrayWrapper right) =>
        new(new BitArray(left._bitArray).Xor(right._bitArray));

    public static BitArrayWrapper operator ~(BitArrayWrapper value)
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(BitArrayWrapper? left, BitArrayWrapper? right) =>
        left is not null && right is not null && (left ^ right).PopCount() == 0;

    public static bool operator !=(BitArrayWrapper? left, BitArrayWrapper? right) => !(left == right);

    public static BitArrayWrapper Create(int count) => new(count);

    public int Count { get; }
    public int PopCount()
    {
        var ones = 0;
        for (var i = 0; i < Count; ++i)
        {
            if (IsSet(i)) ones++;
        }

        return ones;
    }

    public bool IsSet(int index) => _bitArray.Get(index);

    public void Set(int index) => _bitArray.Set(index, true);

    public void Unset(int index) => _bitArray.Set(index, false);

    public BitArrayWrapper Copy() => new(new BitArray(_bitArray));

    public IEnumerable<int> Ones()
    {
        for (var i = 0; i < Count; ++i)
        {
            if (IsSet(i)) yield return i;
        }
    }
}
