using BitSets;
using DependencyTester.FdMembershipTester;
using OrderDependencyModels;

namespace DependencyTester.ListBasedOdMembershipTester;

public class ListBasedOdAlgorithm<TBitSet> where TBitSet : IBitSet<TBitSet>
{
    public required ICollection<ConstantOrderDependency<TBitSet>> Constants { private get; init; }
    public required ColumnsTree<HashSet<OrderCompatibleDependency<TBitSet>>, TBitSet> CompatiblesTree { private get; init; }
    public required int NumberOfAttributes { private get; init; }
    private FdMembershipAlgorithm<TBitSet>? _fdAlgo;

    private FdMembershipAlgorithm<TBitSet> FdAlgo => _fdAlgo ??=
        new FdMembershipAlgorithm<TBitSet>(Constants.Select(FunctionalDependency<TBitSet>.FromConstantOrderDependency).ToArray(),
            NumberOfAttributes);


    private bool SplitsExist(ListBasedOrderDependency odUnderTest)
    {
        var fd = new FunctionalDependency<TBitSet>
        {
            Lhs = BitArrayFrom(odUnderTest.LeftHandSide.Select(orderSpec => orderSpec.Attribute), NumberOfAttributes),
            Rhs = BitArrayFrom(odUnderTest.RightHandSide.Select(orderSpec => orderSpec.Attribute), NumberOfAttributes),
        };
        return !FdAlgo.IsValid(fd);

        static TBitSet BitArrayFrom(IEnumerable<int> toSet, int size)
        {
            var bitArray = TBitSet.Create(size);
            foreach (var indexToSet in toSet)
            {
                bitArray.Set(indexToSet);
            }

            return bitArray;
        }
    }

    private bool SwapsExist(ListBasedOrderDependency odUnderTest)
    {
        // This context is formed from the RHS of the list-based OD.
        // In the inner loop, the LHS attributes are added independently.
        var contextFromRight = TBitSet.Create(NumberOfAttributes);

        foreach (var rightOrderSpec in odUnderTest.RightHandSide)
        {
            var rightAttribute = rightOrderSpec.Attribute;
            // Context for the current iteration, includes the right context.
            var context = contextFromRight.Copy();
            // We use Constant ODs, but interpret them as FDs.
            var fdsToTest = new List<FunctionalDependency<TBitSet>>();
            var fdToOd = new Dictionary<FunctionalDependency<TBitSet>, OrderCompatibleDependency<TBitSet>>();

            foreach (var leftOrderSpec in odUnderTest.LeftHandSide)
            {
                var leftAttribute = leftOrderSpec.Attribute;

                var correspondingOd = new OrderCompatibleDependency<TBitSet>
                {
                    Context = context.Copy(),
                    Lhs = leftOrderSpec,
                    Rhs = rightOrderSpec
                };
                if (!IsValid(correspondingOd))
                {
                    var rhs = TBitSet.Create(NumberOfAttributes);
                    rhs.Set(leftAttribute);
                    var fdToTest = new FunctionalDependency<TBitSet>
                    {
                        Lhs = correspondingOd.Context,
                        Rhs = rhs,
                    };
                    fdToOd.Add(fdToTest, correspondingOd);
                    fdsToTest.Add(fdToTest);
                }


                context.Set(leftAttribute);
            }
            contextFromRight.Set(rightAttribute);

            if (fdsToTest.Count == 0) continue;

            var areProvenValid = FdAlgo.AreValid(fdsToTest, rightAttribute);
            foreach (var (fd, isValid) in areProvenValid)
            {
                if (!isValid) return true;
                // if (isValid) continue;
                // run actual set-based OD algorithm on this
                // Note: When collecting ODs from more than one iteration of the RHS loop, make sure to copy the right order spec.
                // var odToTest = fdToOd[fd];
                // var isNowValid = IsValid(odToTest);
                // There is no way this OD still holds. Since all ODs have to hold, there exists a Swap somewhere.
                // if (!isNowValid) return true;
            }

        }
        // No Swaps have been found.
        return false;
    }

    private bool IsValid(OrderCompatibleDependency<TBitSet> odCandidate)
    {
        // Note: If this becomes a performance problem, we can look into directly implementing the methods used here.
        // 1. Check if LHS == RHS.
        if (odCandidate.Distinct().Count() == 1)
        {
            return true;
        }

        // 2. Check if LHS or RHS is part of Context.
        if (odCandidate.Any(side => odCandidate.Context.Get(side.Attribute)))
        {
            return true;
        }

        // 3. Check if there is a subset of the Context that is known to be valid.
        return HasSupersetByAugmentation(odCandidate) ||
               // 4. Check if 3. holds for reversed directions.
               HasSupersetByAugmentation(odCandidate.Reverse());

        bool HasSupersetByAugmentation(OrderCompatibleDependency<TBitSet> orderCompatibleDependency) =>
            CompatiblesTree.GetSubsets(orderCompatibleDependency.Context)
                .Any(set => set.Any(other => orderCompatibleDependency
                    .All(os => other.Contains(os)))
                );
    }

    private bool IsValid(ListBasedOrderDependency odUnderTest)
    {
        if (SwapsExist(odUnderTest))
            return false;
        return !SplitsExist(odUnderTest);
    }

    public IEnumerable<KeyValuePair<ListBasedOrderDependency, bool>>
        AreValid(IEnumerable<ListBasedOrderDependency> odsUnderTest) =>
        odsUnderTest.Select(od => new KeyValuePair<ListBasedOrderDependency, bool>(od, IsValid(od)));
}
