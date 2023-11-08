using OrderDependencyModels;

List<ListBasedOrderDependency> knownDependencies = SetBasedParser.parseSetBased(args[0]);
List<ListBasedOrderDependency> testDependencies = ListBasedParser.parseListBased(args[1]);
