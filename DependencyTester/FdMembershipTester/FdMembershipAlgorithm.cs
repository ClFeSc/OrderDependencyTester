using OrderDependencyModels;
using Attribute = OrderDependencyModels.Attribute;

namespace DependencyTester.FdMembershipTester;

public static class FdMembershipAlgorithm
{
    private record AnnotatedFd
    {
        public required FunctionalDependency Fd { get; init; }
        public required int RequiredAttributes { get; set; }
    }

    public static bool IsValid(FunctionalDependency fdUnderTest, IEnumerable<FunctionalDependency> groundTruth, IEnumerable<Attribute> allAttributes)
    {
        var reachableDependandts = fdUnderTest.Lhs;
        var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Select((attribute) =>
            new KeyValuePair<Attribute, List<AnnotatedFd>>(attribute, new List<AnnotatedFd>())));
        var remainingAttributes = new Queue<Attribute>(fdUnderTest.Lhs);

        var annotatedGroundTruth = groundTruth.Select((fd) => new AnnotatedFd
        {
            Fd = fd,
            RequiredAttributes = fd.Lhs.Count,
        });
        foreach (var fd in annotatedGroundTruth)
        {
            foreach (var attribute in fd.Fd.Lhs)
            {
                fdsPerAttribute[attribute].Add(fd);
            }
        }

        while (remainingAttributes.TryDequeue(out var attribute))
        {
            foreach (var g in fdsPerAttribute[attribute])
            {
                g.RequiredAttributes--;
                if (g.RequiredAttributes == 0)
                {
                    foreach (var dependentAttribute in g.Fd.Rhs)
                    {
                        if (!reachableDependandts.Contains(dependentAttribute))
                        {
                            reachableDependandts.Add(dependentAttribute);
                            remainingAttributes.Enqueue(dependentAttribute);
                        }
                    }
                }
            }
        }

        return fdUnderTest.Rhs.All((attribute) => reachableDependandts.Contains(attribute));
    }
}