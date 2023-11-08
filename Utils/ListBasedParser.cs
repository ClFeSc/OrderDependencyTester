
namespace Utils

public static List<ListBasedOrderDependency> parseListBased(string filename) {
    var dependencies = new List<ListBasedOrderDependency>();
    foreach (var line in File.ReadAllLines(filename)) {

        // parse line in format [B↑,C↑] -> [E↓,A↑]
        var match = Regex.Match(line, @"\[(.+)\] -> \[(.+)\]");
        if (match.Success) {
            var lhs = match.Groups[1].Value.Split(",").Select(x => parseOS(x)).ToList();
            var rhs = match.Groups[2].Value.Split(",").Select(x => parseOS(x)).ToList();
            dependencies.Add(new ListBasedOrderDependency(lhs, rhs));
        }
    }
    return dependencies;
}