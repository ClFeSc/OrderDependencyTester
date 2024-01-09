using System.Diagnostics;
using CliFrontend;

// args = new[]
// {
// "/Users/paulsieben/HPI/WiSe 2023-2024 Advanced Data Profiling/Example DISTOD Results/horse-sub-results.txt",
//  "/Users/paulsieben/Programming/OrderDependencyTester/testdata/Horse-300-27/invalid_2_2.txt",
//   "/Users/paulsieben/Programming/OrderDependencyTester/testdata/Horse-300-27/attributes.txt"
// };

if (args.Length != 3)
{
    Console.Error.WriteLine($"Expected 3 arguments, got {args.Length}.");
    Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} [known set based dependency path: string] [list based dependencies to test path: string] [attributes path: string]");
    return 1;
}

var sw = new Stopwatch();
sw.Start();
var count = 0;
foreach (var (dependencyToTest, isValid) in TestExecutor.TestDependencies(args[0], args[1], args[2]))
{
    count++;
    // Console.WriteLine($"OD {dependencyToTest} is {(isValid ? "" : "not ")}valid");
}
sw.Stop();
Console.WriteLine(sw.Elapsed);
Console.WriteLine(count);

return 0;
