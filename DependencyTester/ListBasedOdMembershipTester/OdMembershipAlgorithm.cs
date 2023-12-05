using System.Collections;
using DependencyTester.FdMembershipTester;
using OrderDependencyModels;

namespace DependencyTester.ListBasedOdMembershipTester;

public class ListBasedOdAlgorithm
{
    public required ICollection<ConstantOrderDependency> Constants { private get; init; }
    public required ColumnsTree<HashSet<OrderCompatibleDependency>> CompatiblesTree { private get; init; }
    public required int NumberOfAttributes { private get; init; }
    private FdMembershipAlgorithm? _fdAlgo;

    private FdMembershipAlgorithm FdAlgo => _fdAlgo ??=
        new FdMembershipAlgorithm(Constants.Select(FunctionalDependency.FromConstantOrderDependency).ToArray(),
            NumberOfAttributes);


    private bool SplitsExist(ListBasedOrderDependency odUnderTest)
    {
        var fd = new FunctionalDependency
        {
            Lhs = BitArrayFrom(odUnderTest.LeftHandSide.Select(orderSpec => orderSpec.Attribute), NumberOfAttributes),
            Rhs = BitArrayFrom(odUnderTest.RightHandSide.Select(orderSpec => orderSpec.Attribute), NumberOfAttributes),
        };
        return !FdAlgo.IsValid(fd);

        static BitArray BitArrayFrom(IEnumerable<int> toSet, int size)
        {
            var bitArray = new BitArray(size);
            foreach (var indexToSet in toSet)
            {
                bitArray.Set(indexToSet, true);
            }

            return bitArray;
        }
    }

    private bool SwapsExist(ListBasedOrderDependency odUnderTest)
    {
        // This context is formed from the RHS of the list-based OD.
        // In the inner loop, the LHS attributes are added independently.
        var contextFromRight = new BitArray(NumberOfAttributes);

        foreach (var rightOrderSpec in odUnderTest.RightHandSide)
        {
            var rightAttribute = rightOrderSpec.Attribute;
            // Context for the current iteration, includes the right context.
            var context = new BitArray(contextFromRight);
            // We use Constant ODs, but interpret them as FDs.
            var fdsToTest = new List<FunctionalDependency>();
            var fdToOd = new Dictionary<FunctionalDependency, OrderCompatibleDependency>();

            foreach (var leftOrderSpec in odUnderTest.LeftHandSide)
            {
                var leftAttribute = leftOrderSpec.Attribute;

                var correspondingOd = new OrderCompatibleDependency
                {
                    Context = new BitArray(context),
                    Lhs = leftOrderSpec,
                    Rhs = rightOrderSpec
                };
                if (!IsValid(correspondingOd))
                {
                    var rhs = new BitArray(NumberOfAttributes);
                    rhs.Set(leftAttribute, true);
                    var fdToTest = new FunctionalDependency
                    {
                        Lhs = correspondingOd.Context,
                        Rhs = rhs,
                    };
                    fdToOd.Add(fdToTest, correspondingOd);
                    fdsToTest.Add(fdToTest);
                }


                context.Set(leftAttribute, true);
            }
            contextFromRight.Set(rightAttribute, true);

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

    private bool IsValid(OrderCompatibleDependency odCandidate)
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

        bool HasSupersetByAugmentation(OrderCompatibleDependency orderCompatibleDependency) =>
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
