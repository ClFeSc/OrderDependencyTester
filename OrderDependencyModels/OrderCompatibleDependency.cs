namespace OrderDependencyModels;

public readonly record struct OrderCompatibleDependency(HashSet<Attribute> Context, Attribute LeftHandSide, Attribute RightHandSide) : ISetBasedOrderDependency;