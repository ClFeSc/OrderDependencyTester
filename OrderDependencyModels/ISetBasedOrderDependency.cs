namespace OrderDependencyModels;

public interface ISetBasedOrderDependency : IOrderDependency
{
    public static (List<ConstantOrderDependency> startingCods, List<OrderCompatibleDependency> startingCompOds) Parse(string fileName)
    {
        var startingConstOds = new List<ConstantOrderDependency>();
        var startingCompOds = new List<OrderCompatibleDependency>();

        // read each line of input.txt
        foreach (var line in File.ReadLines(fileName))
        {
            if (ConstantOrderDependency.TryParse(line, out var constantOrderDependency))
            {
                startingConstOds.Add(constantOrderDependency.Value);
                continue;
            }
            else if (OrderCompatibleDependency.TryParse(line, out var orderCompatibleDependency))
            {
                startingCompOds.Add(orderCompatibleDependency.Value);
            }
            else if (!string.IsNullOrWhiteSpace(line)) {
                throw new FormatException($"OD String not recognized: {line}");
            }
        }

        return (startingConstOds, startingCompOds);
    }
}
