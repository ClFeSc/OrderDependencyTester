using System.Diagnostics.CodeAnalysis;

namespace OrderDependencyModels;

public readonly record struct Attribute
{
    public required string Name { get; init; }

    [SetsRequiredMembers]
    public Attribute(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
