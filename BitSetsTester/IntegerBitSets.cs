using BitSets;

namespace BitSetsTester;

using TBase = uint;

public class IntegerBitSets
{
    private int Size { get; } = 5;
    private IntegerBitSet<TBase> BitSet { get; set; }

    public IntegerBitSets()
    {
        BitSet = IntegerBitSet<TBase>.Create(Size);
    }

    [Fact]
    public void CreatesEmpty()
    {
        for (var i = 0; i < Size; ++i)
        {
            Assert.False(BitSet.IsSet(i));
        }
    }

    [Fact]
    public void Setting()
    {
        BitSet.Set(3);
        Assert.True(BitSet.IsSet(3));
    }
}
