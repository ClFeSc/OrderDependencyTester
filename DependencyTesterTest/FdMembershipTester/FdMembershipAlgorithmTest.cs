using System.Collections;
using DependencyTester.FdMembershipTester;
using OrderDependencyModels;

namespace DependencyTesterTest.FdMembershipTester;

public class FdMembershipAlgorithmTest
{
    private record FdToTest(FunctionalDependency FunctionalDependency, bool ShouldBeValid);

    private record FdsToTestWithEarlyReturn(List<FdToTest> FdsToTest, int EarlyReturnAttribute);

    private static int NumberOfAttributes { get; } = 6;

    private static BitArray BitArraySetAt(HashSet<int> indices)
    {
        var array = new BitArray(NumberOfAttributes);
        foreach (var index in indices)
            array.Set(index, true);
        return array;
    }

    private static HashSet<FunctionalDependency> GroundTruth { get; } = new()
    {
        new FunctionalDependency(0, 1, NumberOfAttributes),
        new FunctionalDependency(1, 2, NumberOfAttributes),
        new FunctionalDependency
            {Lhs = BitArraySetAt(new HashSet<int> {0, 2}), Rhs = BitArraySetAt(new HashSet<int> {3})},
        new FunctionalDependency(1, 5, NumberOfAttributes),
    };

    public static IEnumerable<object[]> ToTestForIsValid { get; } = new List<FdToTest[]>
    {
        new [] { new FdToTest(new FunctionalDependency(0, 2, NumberOfAttributes), true) },
        new [] { new FdToTest(new FunctionalDependency(1, 2, NumberOfAttributes), true) },
        new [] { new FdToTest(new FunctionalDependency(0, 0, NumberOfAttributes), true) },
        new [] { new FdToTest(new FunctionalDependency(0, 3, NumberOfAttributes), true) },
        new [] { new FdToTest(new FunctionalDependency(3, 2, NumberOfAttributes), false) },
        new [] { new FdToTest(new FunctionalDependency(2, 1, NumberOfAttributes), false) },
    };

    private static readonly List<List<FdToTest>[]> ToTestForAreValidTyped = new()
    {
        new[]
        {
            new List<FdToTest>
            {
                new(new FunctionalDependency(BitArraySetAt(new HashSet<int>()), BitArraySetAt(new HashSet<int> {0})),
                    false),
                new(new FunctionalDependency(0, 1, NumberOfAttributes), true),
                new(
                    new FunctionalDependency(BitArraySetAt(new HashSet<int> {0, 1}),
                        BitArraySetAt(new HashSet<int> {2})), true),
                new(
                    new FunctionalDependency(BitArraySetAt(new HashSet<int> {0, 1, 2}),
                        BitArraySetAt(new HashSet<int> {3})), true),
                new(
                    new FunctionalDependency(
                        BitArraySetAt(new HashSet<int> {0, 1, 2, 3}),
                        BitArraySetAt(new HashSet<int> {4})), false),
            }
        }
    };

    public static IEnumerable<object[]> ToTestForAreValid { get; } = ToTestForAreValidTyped;

    public static IEnumerable<object[]> ToTestForAreValidWithEarlyReturn { get; } = new List<FdsToTestWithEarlyReturn[]>
    {
        new[]
        {
            new FdsToTestWithEarlyReturn(new List<FdToTest>
            {
                new(new FunctionalDependency(BitArraySetAt(new HashSet<int>()), BitArraySetAt(new HashSet<int> {0})),
                    false),
                new(new FunctionalDependency(0, 1, NumberOfAttributes), true),
                new(
                    new FunctionalDependency(BitArraySetAt(new HashSet<int> {0, 1}),
                        BitArraySetAt(new HashSet<int> {2})), true),
                new(
                    new FunctionalDependency(BitArraySetAt(new HashSet<int> {0, 1, 2}),
                BitArraySetAt(new HashSet<int> {3})), true),
                new(
                    new FunctionalDependency(
                        BitArraySetAt(new HashSet<int> {0, 1, 2, 3}),
                        BitArraySetAt(new HashSet<int> {4})), true),

            }, 5)
        }
    };

    [Theory]
    [MemberData(nameof(ToTestForIsValid))]
    private void TestIsValid(FdToTest fdToTest)
    {
        var isValid = new FdMembershipAlgorithm(GroundTruth,6).IsValid(fdToTest.FunctionalDependency);
        Assert.Equal(fdToTest.ShouldBeValid, isValid);
    }

    [Theory]
    [MemberData(nameof(ToTestForAreValid))]
    private void TestAreValidWithoutEarlyReturn(List<FdToTest> fdsToTest)
    {
        var areValid = new FdMembershipAlgorithm(GroundTruth,6).AreValid(fdsToTest.Select(fd => fd.FunctionalDependency).ToArray());

        foreach (var fd in fdsToTest)
        {
            Assert.Equal(fd.ShouldBeValid, areValid[fd.FunctionalDependency]);
        }
    }

    [Theory]
    [MemberData(nameof(ToTestForAreValidWithEarlyReturn))]
    private void TestAreValidWithEarlyReturn(FdsToTestWithEarlyReturn fdsToTest)
    {
        var areValid = new FdMembershipAlgorithm(GroundTruth,6).AreValid(
            fdsToTest.FdsToTest.Select(fd => fd.FunctionalDependency).ToArray(),
            fdsToTest.EarlyReturnAttribute);

        foreach (var fd in fdsToTest.FdsToTest)
        {
            Assert.Equal(fd.ShouldBeValid, areValid[fd.FunctionalDependency]);
        }
    }
}
