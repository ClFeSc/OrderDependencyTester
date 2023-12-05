using OrderDependencyModels;

namespace DependencyTester.FdMembershipTester;

public class FdMembershipAlgorithm
{

    private Dictionary<int, List<FunctionalDependency>> _fdsByAttribute = new Dictionary<int, List<FunctionalDependency>>();

    public FdMembershipAlgorithm(IEnumerable<FunctionalDependency> fds, int NumAttributes)
    {
        for (int i = 0; i < NumAttributes; i++)
        {
            _fdsByAttribute[i] = new List<FunctionalDependency>();
        }
        foreach (var fd in fds)
        {
            foreach (var column in fd.Lhs)
            {
                _fdsByAttribute[column].Add(fd);
            }
        }
    }

    /// <summary>
    /// Check whether multiple FDs are valid.
    /// </summary>
    /// <param name="fdsUnderTest">The FDs to check for validity given the <see cref="groundTruth"/>. <b>Note</b>: The LHS of the (i+1)-th FD <b>MUST</b> be a superset of the i-th FD!</param>
    /// <param name="groundTruth">The given FDs that are known to hold.</param>
    /// <param name="allAttributes">All the attributes present in the relation for which the FDs are defined.</param>
    /// <param name="earlyReturnAttribute">If present, and it is reachable as a RHS while testing the i-th FD, this one and all following FDs will be assumed to hold.</param>
    /// <returns>A mapping from each FD from <see cref="fdsUnderTest"/> to whether they hold given the <see cref="groundTruth"/>.</returns>
    public Dictionary<FunctionalDependency, bool> AreValid(IList<FunctionalDependency> fdsUnderTest,
        int? earlyReturnAttribute = null)
    {
        var reachableDependants = new HashSet<int>();
        // var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Select(attribute =>
        //     new KeyValuePair<Attribute, List<AnnotatedFd>>(attribute, new List<AnnotatedFd>(groundTruth.Count()))));
        var remainingAttributes = new Queue<int>();

        Dictionary<FunctionalDependency, bool> result =
            new(fdsUnderTest.Select(fd => new KeyValuePair<FunctionalDependency, bool>(fd, false)));

        var requiredAttributeCounts = new Dictionary<FunctionalDependency, int>();
        var fdIndex = -1;
        foreach (var fdUnderTest in fdsUnderTest)
        {
            fdIndex++;
            foreach (var lhsAttribute in fdUnderTest.Lhs.Where(lhsAttribute => !reachableDependants.Contains(lhsAttribute)))
            {
                remainingAttributes.Enqueue(lhsAttribute);
            }
            reachableDependants.UnionWith(fdUnderTest.Lhs);

            while (remainingAttributes.TryDequeue(out var attribute))
            {
                foreach (var fd in _fdsByAttribute[attribute])
                {
                    requiredAttributeCounts.TryAdd(fd,fd.Lhs.Count);
                    var RequiredAttributes = requiredAttributeCounts[fd] = requiredAttributeCounts[fd] - 1;


                    if (RequiredAttributes != 0) continue;
                    foreach (var dependentAttribute in fd.Rhs.Where(dependentAttribute => !reachableDependants.Contains(dependentAttribute)))
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

            result[fdUnderTest] = fdUnderTest.Rhs.All(reachableDependants.Contains);
        }

        return result;
    }

    public bool IsValid(FunctionalDependency fdUnderTest)
    {
        return AreValid(new[] { fdUnderTest }).Single().Value;
    }
}
