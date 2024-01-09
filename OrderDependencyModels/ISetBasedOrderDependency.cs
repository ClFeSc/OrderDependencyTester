using BitSets;

namespace OrderDependencyModels;

public interface ISetBasedOrderDependency : IOrderDependency
{
    public static (List<ConstantOrderDependency<TBitSet>> startingCods, List<OrderCompatibleDependency<TBitSet>> startingCompOds) Parse<TBitSet>(Dictionary<string, int> attributesMap, string fileName) where TBitSet : IBitSet<TBitSet>
    {
        var startingConstOds = new List<ConstantOrderDependency<TBitSet>>();
        var startingCompOds = new List<OrderCompatibleDependency<TBitSet>>();

        // read each line of input.txt
        foreach (var line in File.ReadLines(fileName))
        {
            if (ConstantOrderDependency<TBitSet>.TryParse(attributesMap,line, out var constantOrderDependency))
            {
                startingConstOds.Add(constantOrderDependency.Value);
                continue;
            }
            else if (OrderCompatibleDependency<TBitSet>.TryParse(attributesMap, line, out var orderCompatibleDependency))
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
