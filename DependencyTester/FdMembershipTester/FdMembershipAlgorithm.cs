using BitSets;
using OrderDependencyModels;

namespace DependencyTester.FdMembershipTester;

public class FdMembershipAlgorithm<TBitSet> where TBitSet : IBitSet<TBitSet>
{
    private readonly Dictionary<int, List<int>> _fdsByAttribute = new();

    private readonly List<FunctionalDependency<TBitSet>> _fdsByIndex = new();
    private readonly List<int> _lhsSizes = new();

    private readonly TBitSet _alwaysReachable;

    private int NumAttributes { get; init; }

    public FdMembershipAlgorithm(IEnumerable<FunctionalDependency<TBitSet>> fds, int numAttributes)
    {
        NumAttributes = numAttributes;
        _alwaysReachable = TBitSet.Create(numAttributes);
        for (var i = 0; i < NumAttributes; i++)
        {
            _fdsByAttribute[i] = new List<int>();
        }

        int index = 0;
        foreach (var fd in fds)
        {
            _fdsByIndex.Add(fd);
            _lhsSizes.Add(fd.Lhs.PopCount());
            if (fd.Lhs.PopCount() == 0)
            {
                _alwaysReachable |= fd.Rhs;
            }
            foreach (var columnIndex in fd.Lhs.Ones())
            {
                _fdsByAttribute[columnIndex].Add(index);
            }
            index++;
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
    public Dictionary<FunctionalDependency<TBitSet>, bool> AreValid(IList<FunctionalDependency<TBitSet>> fdsUnderTest,
        int? earlyReturnAttribute = null)
    {
        var reachableDependants = _alwaysReachable.Copy();
        // var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Select(attribute =>
        //     new KeyValuePair<Attribute, List<AnnotatedFd>>(attribute, new List<AnnotatedFd>(groundTruth.Count()))));
        var remainingAttributes = new Queue<int>();

        Dictionary<FunctionalDependency<TBitSet>, bool> result =
            new(fdsUnderTest.Select(fd => new KeyValuePair<FunctionalDependency<TBitSet>, bool>(fd, false)));

        var requiredAttributeCounts = new List<int>(_lhsSizes);
        var candidateIndex = -1;
        foreach (var fdUnderTest in fdsUnderTest)
        {
            candidateIndex++;
            // enqueue all in lhs that are not in reachableDependants
            var newRemainings = (fdUnderTest.Lhs ^ reachableDependants) & fdUnderTest.Lhs;
            foreach (var index in newRemainings.Ones())
                remainingAttributes.Enqueue(index);

            reachableDependants |= fdUnderTest.Lhs;

            while (remainingAttributes.TryDequeue(out var attribute))
            {
                foreach (var otherIndex in _fdsByAttribute[attribute])
                {
                    if (--requiredAttributeCounts[otherIndex] > 0) continue;
                    var fd = _fdsByIndex[otherIndex];
                    // enqueue all in rhs that are not in reachableDependants
                    var newlyReachable = (fd.Rhs ^ reachableDependants) & fd.Rhs;
                    foreach (var col in newlyReachable.Ones())
                        remainingAttributes.Enqueue(col);
                    reachableDependants |= fd.Rhs;

                    if (earlyReturnAttribute != null && reachableDependants.Get((int)earlyReturnAttribute))
                        return new Dictionary<FunctionalDependency<TBitSet>, bool>(result.Select((pair, idx) =>
                        {
                            var isValid = idx >= candidateIndex || pair.Value;
                            return new KeyValuePair<FunctionalDependency<TBitSet>, bool>(pair.Key, isValid);
                        }));
                }
            }

            var coveredColumns = reachableDependants & fdUnderTest.Rhs;
            result[fdUnderTest] = coveredColumns == fdUnderTest.Rhs;
            if (earlyReturnAttribute != null && reachableDependants.Get((int)earlyReturnAttribute))
                return new Dictionary<FunctionalDependency<TBitSet>, bool>(result.Select((pair, idx) =>
                {
                    var isValid = idx >= candidateIndex || pair.Value;
                    return new KeyValuePair<FunctionalDependency<TBitSet>, bool>(pair.Key, isValid);
                }));
        }

        return result;
    }

    public bool IsValid(FunctionalDependency<TBitSet> fdUnderTest)
    {
        return AreValid(new[] { fdUnderTest }).Single().Value;
    }
}
