namespace OrderDependencyModels;

public readonly record struct ListBasedOrderDependency(List<OrderSpecification> LeftHandSide, List<OrderSpecification> RightHandSide) : IListBasedOrderDependency;