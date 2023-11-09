namespace OrderDependencyModels;

public readonly record struct FunctionalDependency(HashSet<Attribute> Determinant, HashSet<Attribute> Dependent)
{
    public static FunctionalDependency FromConstantOrderDependency(in ConstantOrderDependency od) =>
        new(Determinant: od.Context, Dependent: new HashSet<Attribute> { od.RightHandSide });

    public HashSet<Attribute> Lhs => Determinant;
    public HashSet<Attribute> Rhs => Dependent;
}