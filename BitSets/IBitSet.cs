using System.Numerics;

namespace BitSets;

public interface IBitSet<TSelf> : IBitwiseOperators<TSelf, TSelf, TSelf>, IEqualityOperators<TSelf, TSelf, bool> where TSelf : IBitSet<TSelf>
{
    static abstract TSelf Create(int count);

    /// <summary>
    /// The number of bits that was specified when creating the bit set.
    /// </summary>
    int Count { get; }
    int PopCount();
    bool IsSet(int index);
    void Set(int index);
    void Unset(int index);
    TSelf Copy();
    IEnumerable<int> Ones();

    bool this[int index] => IsSet(index);
    bool Get(int index) => IsSet(index);
}
