using CliFrontend;
using Xunit.Abstractions;

namespace DependencyTesterTest.FrontendTester;

public class TestExecutorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestExecutorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static IEnumerable<object[]> ToTestForFrontend { get; } = new List<object[]>
    {
        // Note: This data has been created using this tool. It is therefore unsuitable for validating the algorithm.
        // However, it can be used to check that the algorithm did not change.
        new object[] { "../../../../testdata/minimal1/ground-truth.csv", "../../../../testdata/minimal1/to-test-valid.unique.txt", "../../../../testdata/minimal1/attributes.txt", true },
        new object[] { "../../../../testdata/minimal1/ground-truth.csv", "../../../../testdata/minimal1/to-test-invalid.unique.txt", "../../../../testdata/minimal1/attributes.txt", false },
        // Regression test for empty context.
        new object[] { "../../../../testdata/minimal3/ground-truth.csv", "../../../../testdata/minimal3/to-test-valid.unique.txt", "../../../../testdata/minimal3/attributes.txt", true },
    };
    
    [Theory]
    [MemberData(nameof(ToTestForFrontend))]
    public void TestFrontend(string groundTruthPath, string toTestPath, string attributesPath, bool allShouldBeValid)
    {
        var result = TestExecutor.TestDependencies(groundTruthPath, toTestPath, attributesPath).ToList();
        Assert.NotEmpty(result);
        foreach (var (od, isValid) in result)
        {
            Assert.Equal(allShouldBeValid, isValid);
        }
    }
}