namespace OrderDependencyModels;

public readonly record struct OrderCompatibleDependency(HashSet<Attribute> Context, OrderSpecification LeftHandSide, OrderSpecification RightHandSide) : ISetBasedOrderDependency;