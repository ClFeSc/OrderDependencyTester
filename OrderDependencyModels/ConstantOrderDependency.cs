namespace OrderDependencyModels;

public readonly record struct ConstantOrderDependency(HashSet<Attribute> Context, Attribute RightHandSide) : ISetBasedOrderDependency;