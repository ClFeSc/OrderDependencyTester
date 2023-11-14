
using System.Net;
using DependencyTester.FdMembershipTester;
using OrderDependencyModels;
using Attribute = OrderDependencyModels.Attribute;

namespace DependencyTester.OdMembershipTester;

public class ListBasedOdAlgorithm
{
    private ICollection<ConstantOrderDependency> _constants;
    private ColumnsTree<HashSet<OrderCompatibleDependency>> _compatiblesTree;
    private ICollection<Attribute> _allAttributes;

    public ListBasedOdAlgorithm(ICollection<ConstantOrderDependency> constants, ColumnsTree<HashSet<OrderCompatibleDependency>> compatiblesTree, ICollection<Attribute> allAttributes)
    {
        _allAttributes = allAttributes;
        _constants = constants;
        _compatiblesTree = compatiblesTree;
    }


    private bool SplitsExist(ListBasedOrderDependency odUnderTest)
    {
        var fds = _constants.Select(cOD => FunctionalDependency.FromConstantOrderDependency(cOD));
        var fd = new FunctionalDependency(
            new HashSet<Attribute>(odUnderTest.LeftHandSide.Select(orderSpec => orderSpec.Attribute)),
            new HashSet<Attribute>(odUnderTest.RightHandSide.Select(orderSpec => orderSpec.Attribute)));
        return !FdMembershipAlgorithm.IsValid(fd, fds, _allAttributes);
    }

    private bool SwapsExist(ListBasedOrderDependency odUnderTest)
    {
        // TODO: add comment
        var knownFds = _constants.Select(cOD => FunctionalDependency.FromConstantOrderDependency(cOD)).ToList();
        // This context is formed from the RHS of the list-based OD.
        // In the inner loop, the LHS attributes are added independently.
        var contextFromRight = new HashSet<Attribute>();

        foreach (var rightOrderSpec in odUnderTest.RightHandSide)
        {
            var rightAttribute = rightOrderSpec.Attribute;
            // Context for the current iteration, includes the right context.
            var context = new HashSet<Attribute>(contextFromRight);
            // We use Constant ODs, but interpret them as FDs.
            var fdsToTest = new List<FunctionalDependency>();
            var fdToOd = new Dictionary<FunctionalDependency, OrderCompatibleDependency>();

            foreach (var leftOrderSpec in odUnderTest.LeftHandSide)
            {
                var leftAttribute = leftOrderSpec.Attribute;
                var fdToTest = new FunctionalDependency(new HashSet<Attribute>(context),
                    new HashSet<Attribute>(new[] { leftAttribute }));
                var correspondingOd = new OrderCompatibleDependency
                {
                    Context = fdToTest.Lhs,
                    Sides = new[] { leftOrderSpec, rightOrderSpec },
                };
                fdToOd.Add(fdToTest, correspondingOd);
                fdsToTest.Add(fdToTest);
                context.Add(leftAttribute);
            }
            var isProvenValid = FdMembershipAlgorithm.AreValid(fdsToTest, knownFds, _allAttributes, rightAttribute);
            foreach (var (fd, isValid) in isProvenValid)
            {
                if (isValid) continue;
                // run actual set-based OD algorithm on this
                // Note: When collecting ODs from more than one iteration of the RHS loop, make sure to copy the right order spec.
                var toTest = fdToOd[fd];
                var isNowValid = IsValid(toTest);
                if (!isNowValid) return true;
            }
            contextFromRight.Add(rightAttribute);
        }
        // No Swaps have been found.
        return false;
    }

    private bool IsValid(OrderCompatibleDependency odCandidate)
    {
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
        bool hasSupersetByAugmentation(OrderCompatibleDependency orderCompatibleDependency) => 
                _compatiblesTree.GetSubsets(orderCompatibleDependency.Context)
                    .Any(set => set.
                    Any(other => orderCompatibleDependency
                    .All(os => other.Contains(os)))
            );
        return hasSupersetByAugmentation(odCandidate) ||
               // 4. Check if 3. holds for reversed directions.
               hasSupersetByAugmentation(odCandidate.Reverse());
    }
    public bool IsValid(ListBasedOrderDependency odUnderTest)
    {
        // map the constants to fds
        if (SplitsExist(odUnderTest))
            return false;
        return !SwapsExist(odUnderTest);
    }
}