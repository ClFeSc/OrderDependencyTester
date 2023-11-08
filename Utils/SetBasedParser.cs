namespace Utils;

public static (List<ConstantOrderDependency> startingCods, List<OrderCompatibleDependency> startingCompOds) parseSetBased(string fileName)
{
    var startingConstOds = new List<ConstantOrderDependency>{};
    var startingCompOds = new List<OrderCompatibleDependency>{};

    // read each line of input.txt
    foreach (string line in File.ReadLines(fileName))
    {
        // parse line in format {A, B, C}: [] ↦ E as a ConstantOrderDependency
        var match = Regex.Match(line, @"{(.+)}: \[\] ↦ (.+)");
        if (match.Success)
        {
            var context = match.Groups[1].Value.Split(", ").Select(x => new Attribute(x));
            var rhs = new Attribute(match.Groups[2].Value);
            var cod = new ConstantOrderDependency(new HashSet<Attribute>(context), rhs);
            startingConstOds.Add(cod);
            continue;
        }
        // parse line in format {D, F, H, I}: B↑ ~ E↓ as a OrderCompatibleDependency
        match = Regex.Match(line, @"{(.+)}: (.+) ~ (.+)");
        if (match.Success)
        {
            var context = match.Groups[1].Value.Split(", ").Select(x => new Attribute(x));
            var lhs = parseOS(match.Groups[2].Value)
            var rhs = parseOS(match.Groups[3].Value)
            var cod = new OrderCompatibleDependency(new HashSet<Attribute>(context), new List<OrderSpecification>(lhs), new List<OrderSpecification>(rhs));
            startingCompOds.Add(cod);
            continue;
        }
    }

    return (startingConstOds, startingCompOds);
}