
using System.Net;
using DependencyTester.FdMembershipTester;
using OrderDependencyModels;
using Attribute = OrderDependencyModels.Attribute;

namespace DependencyTester.OdMembershipTester;

public static class ListBasedOdAlgorithm
{

    public static bool SplitsExist(ListBasedOrderDependency odUnderTest, IEnumerable<ConstantOrderDependency> constants, IEnumerable<Attribute> allAttributes)
    {
        var fds = constants.Select(cOD => FunctionalDependency.FromConstantOrderDependency(cOD));
        var fd = new FunctionalDependency(
            new HashSet<Attribute>(odUnderTest.LeftHandSide.Select(orderSpec => orderSpec.Attribute)),
            new HashSet<Attribute>(odUnderTest.RightHandSide.Select(orderSpec => orderSpec.Attribute)));
        return !FdMembershipAlgorithm.IsValid(fd, fds, allAttributes);
    }

    public static bool SwapsExist(ListBasedOrderDependency odUnderTest, IEnumerable<ConstantOrderDependency> constants, ICollection<OrderCompatibleDependency> compatibles, ICollection<Attribute> allAttributes)
    {
        // TODO: add comment
        var knownFds = constants.Select(cOD => FunctionalDependency.FromConstantOrderDependency(cOD)).ToList();
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
            var isProvenValid = FdMembershipAlgorithm.AreValid(fdsToTest, knownFds, allAttributes, rightAttribute);
            foreach (var (fd, isValid) in isProvenValid)
            {
                if (isValid) continue;
                // run actual set-based OD algorithm on this
                // Note: When collecting ODs from more than one iteration of the RHS loop, make sure to copy the right order spec.
                var toTest = fdToOd[fd];
                var isNowValid = IsValid(toTest, compatibles);
                if (!isNowValid) return true;
            }
            contextFromRight.Add(rightAttribute);
        }
        // No Swaps have been found.
        return false;
    }

    private static bool IsValid(OrderCompatibleDependency odCandidate, IEnumerable<OrderCompatibleDependency> compatibles)
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
        var hasSupersetByAugmentation = (OrderCompatibleDependency orderCompatibleDependency) => compatibles.Any(other =>
            orderCompatibleDependency.All(os => other.Contains(os)) &&
            other.Context.IsSubsetOf(orderCompatibleDependency.Context));
        return hasSupersetByAugmentation(odCandidate) ||
               // 4. Check if 3. holds for reversed directions.
               hasSupersetByAugmentation(odCandidate.Reverse());
    }
    public static bool IsValid(ListBasedOrderDependency odUnderTest, ICollection<ConstantOrderDependency> constants, ICollection<OrderCompatibleDependency> compatibles, ICollection<Attribute> allAttributes)
    {
        // map the constants to fds
        if (SplitsExist(odUnderTest, constants, allAttributes))
            return false;
        return !SwapsExist(odUnderTest, constants, compatibles, allAttributes);
    }
}