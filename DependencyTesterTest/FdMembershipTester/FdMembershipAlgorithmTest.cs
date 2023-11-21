using DependencyTester.FdMembershipTester;
using OrderDependencyModels;
using Attribute = OrderDependencyModels.Attribute;

namespace DependencyTesterTest.FdMembershipTester;

public class FdMembershipAlgorithmTest
{
    private record FdToTest(FunctionalDependency FunctionalDependency, bool ShouldBeValid);

    private record FdsToTestWithEarlyReturn(List<FdToTest> FdsToTest, Attribute EarlyReturnAttribute);

    private static List<Attribute> Attributes { get; } = new()
    {
        new Attribute("A"),
        new Attribute("B"),
        new Attribute("C"),
        new Attribute("D"),
        new Attribute("E"),
        new Attribute("F"),
    };

    private static HashSet<FunctionalDependency> GroundTruth { get; } = new()
    {
        new FunctionalDependency(Attributes[0], Attributes[1]),
        new FunctionalDependency(Attributes[1], Attributes[2]),
        new FunctionalDependency
            {Lhs = new HashSet<Attribute> {Attributes[0], Attributes[2]}, Rhs = new HashSet<Attribute> {Attributes[3]}},
        new FunctionalDependency
        {
            Lhs = new HashSet<Attribute> {Attributes[1]},
            Rhs = new HashSet<Attribute> {Attributes[5]}
        }
    };

    public static IEnumerable<object[]> ToTestForIsValid { get; } = new List<FdToTest[]>
    {
        new [] { new FdToTest(new FunctionalDependency(Attributes[0], Attributes[2]), true) },
        new [] { new FdToTest(new FunctionalDependency(Attributes[1], Attributes[2]), true) },
        new [] { new FdToTest(new FunctionalDependency(Attributes[0], Attributes[0]), true) },
        new [] { new FdToTest(new FunctionalDependency(Attributes[0], Attributes[3]), true) },
        new [] { new FdToTest(new FunctionalDependency(Attributes[3], Attributes[2]), false) },
        new [] { new FdToTest(new FunctionalDependency(Attributes[2], Attributes[1]), false) },
    };

    private static readonly List<List<FdToTest>[]> ToTestForAreValidTyped = new()
    {
        new[]
        {
            new List<FdToTest>
            {
                new(new FunctionalDependency(new HashSet<Attribute>(), new HashSet<Attribute> {Attributes[0]}),
                    false),
                new(new FunctionalDependency(Attributes[0], Attributes[1]), true),
                new(
                    new FunctionalDependency(new HashSet<Attribute> {Attributes[0], Attributes[1]},
                        new HashSet<Attribute> {Attributes[2]}), true),
                new(
                    new FunctionalDependency(new HashSet<Attribute> {Attributes[0], Attributes[1], Attributes[2]},
                        new HashSet<Attribute> {Attributes[3]}), true),
                new(
                    new FunctionalDependency(
                        new HashSet<Attribute> {Attributes[0], Attributes[1], Attributes[2], Attributes[3]},
                        new HashSet<Attribute> {Attributes[4]}), false),
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
                new(new FunctionalDependency(new HashSet<Attribute>(), new HashSet<Attribute> {Attributes[0]}),
                    false),
                new(new FunctionalDependency(Attributes[0], Attributes[1]), true),
                new(
                    new FunctionalDependency(new HashSet<Attribute> {Attributes[0], Attributes[1]},
                        new HashSet<Attribute> {Attributes[2]}), true),
                new(
                    new FunctionalDependency(new HashSet<Attribute> {Attributes[0], Attributes[1], Attributes[2]},
                        new HashSet<Attribute> {Attributes[3]}), true),
                new(
                    new FunctionalDependency(
                        new HashSet<Attribute> {Attributes[0], Attributes[1], Attributes[2], Attributes[3]},
                        new HashSet<Attribute> {Attributes[4]}), true),

            }, Attributes[5])
        }
    };

    [Theory]
    [MemberData(nameof(ToTestForIsValid))]
    private void TestIsValid(FdToTest fdToTest)
    {
        var isValid = FdMembershipAlgorithm.IsValid(fdToTest.FunctionalDependency, GroundTruth, Attributes);
        Assert.Equal(fdToTest.ShouldBeValid, isValid);
    }

    [Theory]
    [MemberData(nameof(ToTestForAreValid))]
    private void TestAreValidWithoutEarlyReturn(List<FdToTest> fdsToTest)
    {
        var areValid = FdMembershipAlgorithm.AreValid(fdsToTest.Select(fd => fd.FunctionalDependency).ToArray(),
            GroundTruth, Attributes);

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
            fdsToTest.FdsToTest.Select(fd => fd.FunctionalDependency).ToArray(), GroundTruth, Attributes,
            fdsToTest.EarlyReturnAttribute);

        foreach (var fd in fdsToTest.FdsToTest)
        {
            Assert.Equal(fd.ShouldBeValid, areValid[fd.FunctionalDependency]);
        }
    }
}
