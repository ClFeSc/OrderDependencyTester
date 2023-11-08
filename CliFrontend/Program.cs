namespace CliFrontend;

using Utils;
using OrderDependencyModels;

class CliFrontend
{
    static void Main(string[] args)
    {
        List<ListBasedOrderDependency> knownDependencies = parseSetBased(args[0]);
        List<ListBasedOrderDependency> testDependencies = ListBasedParser.parseListBased(args[1]);
    }
}
