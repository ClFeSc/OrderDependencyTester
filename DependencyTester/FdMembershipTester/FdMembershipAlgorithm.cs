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
        ICollection<FunctionalDependency> groundTruth, ICollection<Attribute> allAttributes,
        Attribute? earlyReturnAttribute = null)
    {
        var reachableDependants = new HashSet<Attribute>();
        var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Count);
        foreach (var attribute in allAttributes)
        {
            fdsPerAttribute.Add(attribute, new List<AnnotatedFd>(groundTruth.Count));
        }
        // var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Select(attribute =>
        //     new KeyValuePair<Attribute, List<AnnotatedFd>>(attribute, new List<AnnotatedFd>(groundTruth.Count()))));
        var remainingAttributes = new Queue<Attribute>();

        var annotatedGroundTruth = groundTruth.Select((fd) => new AnnotatedFd
        {
            Fd = fd,
            RequiredAttributes = fd.Lhs.Count,
        });

        Dictionary<FunctionalDependency, bool> result =
            new(fdsUnderTest.Select(fd => new KeyValuePair<FunctionalDependency, bool>(fd, false)));

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
            fdIndex++;
            foreach (var lhsAttribute in fd.Lhs.Where(lhsAttribute => !reachableDependants.Contains(lhsAttribute)))
            {
                remainingAttributes.Enqueue(lhsAttribute);
            }
            reachableDependants.UnionWith(fd.Lhs);

            while (remainingAttributes.TryDequeue(out var attribute))
            {
                if (!fdsPerAttribute.TryGetValue(attribute, out var fdsOfAttribute)) continue;
                foreach (var annotatedFd in fdsOfAttribute)
                {
                    annotatedFd.RequiredAttributes--;
                    if (annotatedFd.RequiredAttributes != 0) continue;
                    foreach (var dependentAttribute in annotatedFd.Fd.Rhs.Where(dependentAttribute => !reachableDependants.Contains(dependentAttribute)))
                    {
                        if (earlyReturnAttribute == dependentAttribute)
                            return new Dictionary<FunctionalDependency, bool>(result.Select((pair, idx) =>
                            {
                                var isValid = idx >= fdIndex || pair.Value;
                                return new KeyValuePair<FunctionalDependency, bool>(pair.Key, isValid);
                            }));
                        reachableDependants.Add(dependentAttribute);
                        remainingAttributes.Enqueue(dependentAttribute);
                    }
                }
            }

            result[fd] = fd.Rhs.All(reachableDependants.Contains);
        }

        return result;
    }

    public static bool IsValid(FunctionalDependency fdUnderTest, ICollection<FunctionalDependency> groundTruth, ICollection<Attribute> allAttributes)
    {
        return AreValid(new[] { fdUnderTest }, groundTruth, allAttributes).Single().Value;
    }
}
