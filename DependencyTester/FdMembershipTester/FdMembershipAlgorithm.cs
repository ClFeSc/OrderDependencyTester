using System.Collections;
using OrderDependencyModels;

namespace DependencyTester.FdMembershipTester;

public class FdMembershipAlgorithm
{

    private readonly Dictionary<int, List<FunctionalDependency>> _fdsByAttribute = new();

    private int NumAttributes { get; init; }

    public FdMembershipAlgorithm(IEnumerable<FunctionalDependency> fds, int numAttributes)
    {
        NumAttributes = numAttributes;
        for (var i = 0; i < NumAttributes; i++)
        {
            _fdsByAttribute[i] = new List<FunctionalDependency>();
        }
        foreach (var fd in fds)
        {
            for (var columnIndex = 0; columnIndex < fd.Lhs.Count; columnIndex++)
            {
                if (!fd.Lhs.Get(columnIndex)) continue;
                _fdsByAttribute[columnIndex].Add(fd);
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
        var reachableDependants = new BitArray(NumAttributes);
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
            for (var lhsAttributeIndex = 0; lhsAttributeIndex < fdUnderTest.Lhs.Count; lhsAttributeIndex++)
            {
                if (!fdUnderTest.Lhs.Get(lhsAttributeIndex)) continue;
                var containedInReachableDependents = reachableDependants.Get(lhsAttributeIndex);
                if (!containedInReachableDependents) continue;
                remainingAttributes.Enqueue(lhsAttributeIndex);
            }
            reachableDependants.Or(fdUnderTest.Lhs);

            while (remainingAttributes.TryDequeue(out var attribute))
            {
                foreach (var fd in _fdsByAttribute[attribute])
                {
                    requiredAttributeCounts.TryAdd(fd,fd.Lhs.Count);
                    var requiredAttributes = requiredAttributeCounts[fd] = requiredAttributeCounts[fd] - 1;


                    if (requiredAttributes != 0) continue;
                    for (var dependentAttributeIndex = 0;
                         dependentAttributeIndex < fd.Rhs.Count;
                         dependentAttributeIndex++)
                    {
                        if (!fd.Rhs.Get(dependentAttributeIndex)) continue;
                        
                        var containedInReachableDependents = reachableDependants.Get(dependentAttributeIndex);
                        if (!containedInReachableDependents) continue;
                    // }
                    // foreach (var dependentAttribute in fd.Rhs.Where(dependentAttribute => !reachableDependants.Contains(dependentAttribute)))
                    // {
                        if (earlyReturnAttribute == dependentAttributeIndex)
                            return new Dictionary<FunctionalDependency, bool>(result.Select((pair, idx) =>
                            {
                                var isValid = idx >= fdIndex || pair.Value;
                                return new KeyValuePair<FunctionalDependency, bool>(pair.Key, isValid);
                            }));
                        reachableDependants.Set(dependentAttributeIndex, true);
                        remainingAttributes.Enqueue(dependentAttributeIndex);
                    }
                }
            }

            result[fdUnderTest] = true;
            for (var fdUnderTestRhsIndex = 0; fdUnderTestRhsIndex < fdUnderTest.Rhs.Count; fdUnderTestRhsIndex++)
            {
                if (!fdUnderTest.Rhs.Get(fdUnderTestRhsIndex)) continue;
                if (reachableDependants.Get(fdUnderTestRhsIndex)) continue;
                result[fdUnderTest] = false;
                break;
            }
        }

        return result;
    }

    public bool IsValid(FunctionalDependency fdUnderTest)
    {
        return AreValid(new[] { fdUnderTest }).Single().Value;
    }
}
