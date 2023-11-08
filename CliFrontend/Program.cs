using OrderDependencyModels;

var knownDependencies = ISetBasedOrderDependency.Parse(args[0]);
var testDependencies = ListBasedOrderDependency.Parse(args[1]);
