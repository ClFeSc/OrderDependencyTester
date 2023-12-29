using BitSets;
using OrderDependencyModels;

namespace DependencyTester.FdMembershipTester;

// public static class BitArrayExtensions
// {
//     public static void PrintValues(this BitArray self, int itemsPerLine = 10)
//     {
//         int curerntItems = itemsPerLine;
//         foreach (var obj in self)
//         {
//             if (curerntItems <= 0)
//             {
//                 curerntItems = itemsPerLine;
//                 Console.WriteLine();
//             }
//
//             curerntItems--;
//             Console.Write("{0,8}", obj);
//         }
//
//         Console.WriteLine();
//     }
//
//     public static IEnumerable<int> Ones(this BitArray self)
//     {
//         for (int i = 0; i < self.Length; i++)
//             if (self[i])
//                 yield return i;
//     }
//
//     public static int OnesCount(this BitArray self)
//     {
//         int onesCount = 0;
//         foreach (var bit in self.Ones()) onesCount++;
//         return onesCount;
//     }
//
//     public static bool EqualBitsSet(this BitArray self, BitArray other)
//     {
//         var xored = self.Xor(other);
//         return xored.OnesCount() == 0;
//     }
// }

public class FdMembershipAlgorithm<TBitSet> where TBitSet : IBitSet<TBitSet>
{
    private readonly Dictionary<int, List<FunctionalDependency<TBitSet>>> _fdsByAttribute = new();

    private int NumAttributes { get; init; }

    public FdMembershipAlgorithm(IEnumerable<FunctionalDependency<TBitSet>> fds, int numAttributes)
    {
        NumAttributes = numAttributes;
        for (var i = 0; i < NumAttributes; i++)
        {
            _fdsByAttribute[i] = new List<FunctionalDependency<TBitSet>>();
        }

        foreach (var fd in fds)
        {
            foreach (var columnIndex in fd.Lhs.Ones())
            {
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
    public Dictionary<FunctionalDependency<TBitSet>, bool> AreValid(IList<FunctionalDependency<TBitSet>> fdsUnderTest,
        int? earlyReturnAttribute = null)
    {
        var reachableDependants = TBitSet.Create(NumAttributes);
        // var fdsPerAttribute = new Dictionary<Attribute, List<AnnotatedFd>>(allAttributes.Select(attribute =>
        //     new KeyValuePair<Attribute, List<AnnotatedFd>>(attribute, new List<AnnotatedFd>(groundTruth.Count()))));
        var remainingAttributes = new Queue<int>();

        Dictionary<FunctionalDependency<TBitSet>, bool> result =
            new(fdsUnderTest.Select(fd => new KeyValuePair<FunctionalDependency<TBitSet>, bool>(fd, false)));

        var requiredAttributeCounts = new Dictionary<FunctionalDependency<TBitSet>, int>();
        var fdIndex = -1;
        foreach (var fdUnderTest in fdsUnderTest)
        {
            fdIndex++;
            // enqueue all in lhs that are not in reachableDependants
            var newRemainings = (fdUnderTest.Lhs ^ reachableDependants) & fdUnderTest.Lhs;
            foreach (var index in newRemainings.Ones())
                remainingAttributes.Enqueue(index);

            reachableDependants |= fdUnderTest.Lhs;
            
            while (remainingAttributes.TryDequeue(out var attribute))
            {
                foreach (var fd in _fdsByAttribute[attribute])
                {
                    requiredAttributeCounts.TryAdd(fd, fd.Lhs.PopCount());
                    var requiredAttributes = requiredAttributeCounts[fd] -= 1;
                    if (requiredAttributes != 0) continue;

                    // enqueue all in rhs that are not in reachableDependants
                    var newlyReachable = (fd.Rhs ^ reachableDependants) & fd.Rhs;
                    foreach (var col in newlyReachable.Ones())
                        remainingAttributes.Enqueue(col);
                    reachableDependants |= fd.Rhs;

                    if (earlyReturnAttribute != null && reachableDependants.Get((int)earlyReturnAttribute))
                        return new Dictionary<FunctionalDependency<TBitSet>, bool>(result.Select((pair, idx) =>
                        {
                            var isValid = idx >= fdIndex || pair.Value;
                            return new KeyValuePair<FunctionalDependency<TBitSet>, bool>(pair.Key, isValid);
                        }));
                }
            }

            var coveredColumns = reachableDependants & fdUnderTest.Rhs;
            result[fdUnderTest] = coveredColumns == fdUnderTest.Rhs;
        }

        return result;
    }

    public bool IsValid(FunctionalDependency<TBitSet> fdUnderTest)
    {
        return AreValid(new[] { fdUnderTest }).Single().Value;
    }
}
