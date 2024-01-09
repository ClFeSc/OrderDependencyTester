using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BitSets;

public class IntegerBitSet<TBase> : IBitSet<IntegerBitSet<TBase>>, IEquatable<IntegerBitSet<TBase>> where TBase : IBinaryInteger<TBase>, IUnsignedNumber<TBase>
{
    public bool Equals(IntegerBitSet<TBase>? other)
    {
        return other is not null && EqualityComparer<TBase>.Default.Equals(_value, other._value) &&
               Count == other.Count;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntegerBitSet<TBase> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value, Count);
    }

    [SetsRequiredMembers]
    private IntegerBitSet(TBase value, int count)
    {
        _value = value;
        Count = count;
    }

    private TBase _value;

    public static IntegerBitSet<TBase> operator &(IntegerBitSet<TBase> left, IntegerBitSet<TBase> right) =>
        new(left._value & right._value, left.Count);

    public static IntegerBitSet<TBase> operator |(IntegerBitSet<TBase> left, IntegerBitSet<TBase> right) =>
        new(left._value | right._value, left.Count);

    public static IntegerBitSet<TBase> operator ^(IntegerBitSet<TBase> left, IntegerBitSet<TBase> right) =>
        new(left._value ^ right._value, left.Count);

    public static IntegerBitSet<TBase> operator ~(IntegerBitSet<TBase> value) => new(~value._value, value.Count);

    public static IntegerBitSet<TBase> Create(int count) => new(TBase.Zero, count);

    /// <summary>
    /// The maximal width of the bit set.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    public static int Width { get; } = int.CreateChecked(TBase.PopCount(TBase.AllBitsSet));
    public required int Count { get; init; }
    public int PopCount() => int.CreateChecked(TBase.PopCount(_value));

    public bool IsSet(int index) => ((_value >> index) & TBase.One) == TBase.One;

    public void Set(int index) => _value |= TBase.One << index;

    public void Unset(int index) => _value &= ~(TBase.One << index);

    public IntegerBitSet<TBase> Copy() => new(_value, Count);

    public IEnumerable<int> Ones()
    {
        for (var i = 0; i < Count; ++i)
        {
            if (IsSet(i)) yield return i;
        }
    }

    public static bool operator ==(IntegerBitSet<TBase>? left, IntegerBitSet<TBase>? right) =>
        left is not null && right is not null && left._value == right._value;

    public static bool operator !=(IntegerBitSet<TBase>? left, IntegerBitSet<TBase>? right) => !(left == right);

    public bool Get(int index) => IsSet(index);

    public bool this[int index]
    {
        get => IsSet(index);
        set
        {
            if (value)
                Set(index);
            else
                Unset(index);
        }
    }
}
