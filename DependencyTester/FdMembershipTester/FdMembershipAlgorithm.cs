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

    /// <summary>
    /// Check whether multiple FDs are valid.
    /// </summary>
    /// <param name="fdsUnderTest">The FDs to check for validity given the <see cref="groundTruth"/>. <b>Note</b>: The LHS of the (i+1)-th FD <b>MUST</b> be a superset of the i-th FD!</param>
    /// <param name="groundTruth">The given FDs that are known to hold.</param>
    /// <param name="allAttributes">All the attributes present in the relation for which the FDs are defined.</param>
    /// <param name="earlyReturnAttribute">If present, and it is reachable as a RHS while testing the i-th FD, this one and all following FDs will be assumed to hold.</param>
    /// <returns>A mapping from each FD from <see cref="fdsUnderTest"/> to whether they hold given the <see cref="groundTruth"/>.</returns>
    public static Dictionary<FunctionalDependency, bool> AreValid(IList<FunctionalDependency> fdsUnderTest,
        IEnumerable<FunctionalDependency> groundTruth, IEnumerable<Attribute> allAttributes,
        Attribute? earlyReturnAttribute = null)
    {
        var reachableDependandts = new HashSet<Attribute>();
        var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Select((attribute) =>
            new KeyValuePair<Attribute, List<AnnotatedFd>>(attribute, new List<AnnotatedFd>())));
        var remainingAttributes = new Queue<Attribute>();

        var annotatedGroundTruth = groundTruth.Select((fd) => new AnnotatedFd
        {
            Fd = fd,
            RequiredAttributes = fd.Lhs.Count,
        });

        var result =
            new Dictionary<FunctionalDependency, bool>(fdsUnderTest.Select(fd =>
                new KeyValuePair<FunctionalDependency, bool>(fd, false)));

        foreach (var fd in annotatedGroundTruth)
        {
            foreach (var attribute in fd.Fd.Lhs)
            {
                fdsPerAttribute[attribute].Add(fd);
            }
        }

        var fdIndex = -1;
        foreach (var fd in fdsUnderTest)
        {
            {
                fdIndex++;
                var newToBeChecked = new HashSet<Attribute>(fd.Lhs);
                newToBeChecked.ExceptWith(reachableDependandts);
                reachableDependandts.UnionWith(fd.Lhs);
                foreach (var newAttribute in newToBeChecked)
                {
                    remainingAttributes.Enqueue(newAttribute);
                }

                while (remainingAttributes.TryDequeue(out var attribute))
                {
                    if (fdsPerAttribute.ContainsKey(attribute))
                        foreach (var g in fdsPerAttribute[attribute])
                        {
                            g.RequiredAttributes--;
                            if (g.RequiredAttributes != 0) continue;
                            foreach (var dependentAttribute in g.Fd.Rhs)
                            {
                                if (reachableDependandts.Contains(dependentAttribute)) continue;
                                if (earlyReturnAttribute == dependentAttribute)
                                    return new Dictionary<FunctionalDependency, bool>(result.Select((pair, idx) =>
                                    {
                                        var isValid = idx >= fdIndex || pair.Value;
                                        return new KeyValuePair<FunctionalDependency, bool>(pair.Key, isValid);
                                    }));
                                // return result.Select((value, index) => index >= fdIndex || value.Value);
                                reachableDependandts.Add(dependentAttribute);
                                remainingAttributes.Enqueue(dependentAttribute);
                            }
                        }
                }

                result[fd] = fd.Rhs.All(reachableDependandts.Contains);
            }
        }

        return result;
    }

    public static bool IsValid(FunctionalDependency fdUnderTest, IEnumerable<FunctionalDependency> groundTruth, IEnumerable<Attribute> allAttributes)
    {
        return AreValid(new[] { fdUnderTest }, groundTruth, allAttributes).Single().Value;
    }
}
