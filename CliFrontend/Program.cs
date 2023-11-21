using CliFrontend;

// args = new[]
// {
//     "/home/clemens/Dokumente/HPI/WiSe_2023-2024/Advanced_Data_Profiling/nextcloud/WiSe 2023-2024 Advanced Data Profiling/Example DISTOD Results/horse-sub-results.txt",
//     "/home/clemens/Dokumente/HPI/WiSe_2023-2024/Advanced_Data_Profiling/code/OrderDependencyTester/testdata/Horse-300-27/valid_all.unique.txt",
//     "/home/clemens/Dokumente/HPI/WiSe_2023-2024/Advanced_Data_Profiling/code/OrderDependencyTester/testdata/Horse-300-27/attributes.txt"
// };

if (args.Length != 3)
{
    Console.Error.WriteLine($"Expected 3 arguments, got {args.Length}.");
    Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} [known set based dependency path: string] [list based dependencies to test path: string] [attributes path: string]");
    return 1;
}

foreach (var (dependencyToTest, isValid) in TestExecutor.TestDependencies(args[0], args[1], args[2]))
{
    Console.WriteLine($"OD {dependencyToTest} is {(isValid ? "" : "not ")}valid");
}

return 0;
