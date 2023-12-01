using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using DependencyTester.FdMembershipTester;
using OrderDependencyModels;

namespace DependencyTester.ListBasedOdMembershipTester;

public class ListBasedOdAlgorithm
{
    private ICollection<ConstantOrderDependency> _constants;
    private ColumnsTree<HashSet<OrderCompatibleDependency>> _compatiblesTree;


    public ListBasedOdAlgorithm(ICollection<ConstantOrderDependency> Constants, ColumnsTree<HashSet<OrderCompatibleDependency>> CompatiblesTree, int NumAttributes){
        _constants = Constants;
        _compatiblesTree = CompatiblesTree;
        _fdAlgo = new FdMembershipAlgorithm(Constants.Select(FunctionalDependency.FromConstantOrderDependency).ToArray(),NumAttributes);
    }

    private FdMembershipAlgorithm _fdAlgo;


    private bool SplitsExist(ListBasedOrderDependency odUnderTest)
    {
        var fd = new FunctionalDependency(
            new HashSet<int>(odUnderTest.LeftHandSide.Select(orderSpec => orderSpec.Attribute)),
            new HashSet<int>(odUnderTest.RightHandSide.Select(orderSpec => orderSpec.Attribute)));
        return !_fdAlgo.IsValid(fd);
    }

    private bool SwapsExist(ListBasedOrderDependency odUnderTest)
    {
        // This context is formed from the RHS of the list-based OD.
        // In the inner loop, the LHS attributes are added independently.
        var contextFromRight = new HashSet<int>();

        foreach (var rightOrderSpec in odUnderTest.RightHandSide)
        {
            var rightAttribute = rightOrderSpec.Attribute;
            // Context for the current iteration, includes the right context.
            var context = new HashSet<int>(contextFromRight);
            // We use Constant ODs, but interpret them as FDs.
            var fdsToTest = new List<FunctionalDependency>();
            var fdToOd = new Dictionary<FunctionalDependency, OrderCompatibleDependency>();

            foreach (var leftOrderSpec in odUnderTest.LeftHandSide)
            {
                var leftAttribute = leftOrderSpec.Attribute;

                var correspondingOd = new OrderCompatibleDependency
                {
                    Context = new HashSet<int>(context),
                    Lhs = leftOrderSpec,
                    Rhs = rightOrderSpec
                };
                if (!IsValid(correspondingOd))
                {
                    var fdToTest = new FunctionalDependency
                    {
                        Lhs = correspondingOd.Context,
                        Rhs = new HashSet<int> { leftAttribute }
                    };
                    fdToOd.Add(fdToTest, correspondingOd);
                    fdsToTest.Add(fdToTest);
                }


                context.Add(leftAttribute);
            }
            contextFromRight.Add(rightAttribute);

            if (fdsToTest.Count == 0) continue;

            var areProvenValid = _fdAlgo.AreValid(fdsToTest, rightAttribute);
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
        if (odCandidate.Any(side => odCandidate.Context.Contains(side.Attribute)))
        {
            return true;
        }

        // 3. Check if there is a subset of the Context that is known to be valid.
        return HasSupersetByAugmentation(odCandidate) ||
               // 4. Check if 3. holds for reversed directions.
               HasSupersetByAugmentation(odCandidate.Reverse());

        bool HasSupersetByAugmentation(OrderCompatibleDependency orderCompatibleDependency) =>
            _compatiblesTree.GetSubsets(orderCompatibleDependency.Context)
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
