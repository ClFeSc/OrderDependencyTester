using DependencyTester.FdMembershipTester;
using OrderDependencyModels;

namespace DependencyTesterTest.FdMembershipTester;

public class FdMembershipAlgorithmTest
{
    private record FdToTest(FunctionalDependency FunctionalDependency, bool ShouldBeValid);

    private record FdsToTestWithEarlyReturn(List<FdToTest> FdsToTest, int EarlyReturnAttribute);

    private static HashSet<FunctionalDependency> GroundTruth { get; } = new()
    {
        new FunctionalDependency(0, 1),
        new FunctionalDependency(1, 2),
        new FunctionalDependency
            {Lhs = new HashSet<int> {0, 2}, Rhs = new HashSet<int> {3}},
        new FunctionalDependency
        {
            Lhs = new HashSet<int> {1},
            Rhs = new HashSet<int> {5}
        }
    };

    public static IEnumerable<object[]> ToTestForIsValid { get; } = new List<FdToTest[]>
    {
        new [] { new FdToTest(new FunctionalDependency(0, 2), true) },
        new [] { new FdToTest(new FunctionalDependency(1, 2), true) },
        new [] { new FdToTest(new FunctionalDependency(0, 0), true) },
        new [] { new FdToTest(new FunctionalDependency(0, 3), true) },
        new [] { new FdToTest(new FunctionalDependency(3, 2), false) },
        new [] { new FdToTest(new FunctionalDependency(2, 1), false) },
    };

    private static readonly List<List<FdToTest>[]> ToTestForAreValidTyped = new()
    {
        new[]
        {
            new List<FdToTest>
            {
                new(new FunctionalDependency(new HashSet<int>(), new HashSet<int> {0}),
                    false),
                new(new FunctionalDependency(0, 1), true),
                new(
                    new FunctionalDependency(new HashSet<int> {0, 1},
                        new HashSet<int> {2}), true),
                new(
                    new FunctionalDependency(new HashSet<int> {0, 1, 2},
                        new HashSet<int> {3}), true),
                new(
                    new FunctionalDependency(
                        new HashSet<int> {0, 1, 2, 3},
                        new HashSet<int> {4}), false),
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
                new(new FunctionalDependency(new HashSet<int>(), new HashSet<int> {0}),
                    false),
                new(new FunctionalDependency(0, 1), true),
                new(
                    new FunctionalDependency(new HashSet<int> {0, 1},
                        new HashSet<int> {2}), true),
                new(
                    new FunctionalDependency(new HashSet<int> {0, 1, 2},
                        new HashSet<int> {3}), true),
                new(
                    new FunctionalDependency(
                        new HashSet<int> {0, 1, 2, 3},
                        new HashSet<int> {4}), true),

            }, 5)
        }
    };

    [Theory]
    [MemberData(nameof(ToTestForIsValid))]
    private void TestIsValid(FdToTest fdToTest)
    {
        var isValid = FdMembershipAlgorithm.IsValid(fdToTest.FunctionalDependency, GroundTruth, 6);
        Assert.Equal(fdToTest.ShouldBeValid, isValid);
    }

    [Theory]
    [MemberData(nameof(ToTestForAreValid))]
    private void TestAreValidWithoutEarlyReturn(List<FdToTest> fdsToTest)
    {
        var areValid = FdMembershipAlgorithm.AreValid(fdsToTest.Select(fd => fd.FunctionalDependency).ToArray(),
            GroundTruth, 6);

        foreach (var fd in fdsToTest)
        {
            Assert.Equal(fd.ShouldBeValid, areValid[fd.FunctionalDependency]);
        }
    }

    [Theory]
    [MemberData(nameof(ToTestForAreValidWithEarlyReturn))]
    private void TestAreValidWithEarlyReturn(FdsToTestWithEarlyReturn fdsToTest)
    {
        var areValid = FdMembershipAlgorithm.AreValid(
            fdsToTest.FdsToTest.Select(fd => fd.FunctionalDependency).ToArray(), GroundTruth, 6,
            fdsToTest.EarlyReturnAttribute);

        foreach (var fd in fdsToTest.FdsToTest)
        {
            Assert.Equal(fd.ShouldBeValid, areValid[fd.FunctionalDependency]);
        }
    }
}
