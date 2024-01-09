using System.Diagnostics;
using CliFrontend;

// args = new[]
// {
//      "/Users/paulsieben/HPI/WiSe 2023-2024 Advanced Data Profiling/Example Data and ODs/results/credit/category.csv.txt",
//      "/Users/paulsieben/Programming/OrderDependencyTester/testdata/candidates/credit/category.csv.invalids.txt",
//      "/Users/paulsieben/Programming/OrderDependencyTester/testdata/candidates/credit/category.csv.attributes.txt"
//  };
if (args.Length != 3)
{
    Console.Error.WriteLine($"Expected 3 arguments, got {args.Length}.");
    Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} [known set based dependency path: string] [list based dependencies to test path: string] [attributes path: string]");
    return 1;
}

var sw = new Stopwatch();
sw.Start();
var count = 0;
var validCount = 0;
foreach (var (dependencyToTest, isValid) in TestExecutor.TestDependencies(args[0], args[1], args[2]))
{
    count++;
    if (isValid)
    {
        validCount++;
    }
    Console.WriteLine($"{count} OD {dependencyToTest} is {(isValid ? "" : "not ")}valid");
}
sw.Stop();
Console.WriteLine(sw.Elapsed);
Console.WriteLine("Total count: " + count);
Console.WriteLine("Valid count: " + validCount);


return 0;
