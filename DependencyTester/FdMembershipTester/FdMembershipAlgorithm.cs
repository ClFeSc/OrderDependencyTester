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

    //This can be used to check multiple FDs efficiently under the condition that the lhs of the i+1th FD is contained in the lhs of the ith FD
    //If earlyReturnAttribute is set, the algorithm will return true from all FDs onward that determine it.
    public static bool[] AreValid(IEnumerable<FunctionalDependency> fdsUnderTest, IEnumerable<FunctionalDependency> groundTruth, IEnumerable<Attribute> allAttributes, Attribute? earlyReturnAttribute = null)
    {
        var reachableDependandts = new HashSet<Attribute>();
        var numFds = fdsUnderTest.Count();
        var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Select((attribute) =>
            new KeyValuePair<Attribute, List<AnnotatedFd>>(attribute, new List<AnnotatedFd>())));
        var remainingAttributes = new Queue<Attribute>();

        var annotatedGroundTruth = groundTruth.Select((fd) => new AnnotatedFd
        {
            Fd = fd,
            RequiredAttributes = fd.Lhs.Count,
        });

        var result = new bool[numFds];
        for (var i = 0; i < result.Length; i++) result[i] = false;

        foreach (var fd in annotatedGroundTruth)
        {
            foreach (var attribute in fd.Fd.Lhs)
            {
                fdsPerAttribute[attribute].Add(fd);
            }
        }
        var fdIndex = 0;
        foreach (var fd in fdsUnderTest)
        {
            {
                fdIndex++;
                reachableDependandts.UnionWith(fd.Lhs);
                var newToBeChecked = new HashSet<Attribute>(fd.Lhs);
                newToBeChecked.ExceptWith(reachableDependandts);
                foreach (var newAttribute in newToBeChecked)
                {
                    remainingAttributes.Enqueue(newAttribute);
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
                                    if (earlyReturnAttribute is not null && earlyReturnAttribute == dependentAttribute)
                                        return result.Select((_, index) => index >= fdIndex ? true : result[index]).ToArray();
                                    reachableDependandts.Add(dependentAttribute);
                                    remainingAttributes.Enqueue(dependentAttribute);
                                }
                            }
                        }
                    }
                }
                result[fdIndex] = fd.Rhs.All(reachableDependandts.Contains);
            }
        }

        return result;
    }

    public static bool IsValid(FunctionalDependency fdUnderTest, IEnumerable<FunctionalDependency> groundTruth, IEnumerable<Attribute> allAttributes)
    {
        return AreValid(new[] { fdUnderTest }, groundTruth, allAttributes)[0];
    }
}