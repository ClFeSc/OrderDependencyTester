
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
        return FdMembershipAlgorithm.IsValid(fd, fds, allAttributes);
    }

    public static bool? SwapsExist(ListBasedOrderDependency odUnderTest, IEnumerable<ConstantOrderDependency> constants, IEnumerable<OrderCompatibleDependency> compatibles, IEnumerable<Attribute> allAttributes)
    {
        // TODO: add comment
        var knownFds = constants.Select(cOD => FunctionalDependency.FromConstantOrderDependency(cOD));
        var contextFromRight = new HashSet<Attribute>();

        foreach (var rightOrderSpec in odUnderTest.RightHandSide)
        {
            var rightAttribute = rightOrderSpec.Attribute;
            var context = new HashSet<Attribute>(contextFromRight);
            var fdsToTest = new List<FunctionalDependency>();

            foreach (var leftOrderSpec in odUnderTest.LeftHandSide)
            {
                var leftAttribute = leftOrderSpec.Attribute;
                fdsToTest.Add(new FunctionalDependency(context, new HashSet<Attribute>(new[] { leftAttribute })));
                context.Add(leftAttribute);
            }
            var isProvenValid = FdMembershipAlgorithm.AreValid(fdsToTest, knownFds, allAttributes, rightAttribute);
            for (var i = 0; i < isProvenValid.Length; i++)
            {
                if (isProvenValid[i]) continue;
                // run actual set-based OD algorithm on this
                var toTest = new OrderCompatibleDependency(context, odUnderTest.LeftHandSide[i], rightOrderSpec);
            }
            contextFromRight.Add(rightAttribute);
        }
        // since not all aximos are being used, we cannot know if the od is invalid
        return null;
    }
    public static bool? IsValid(ListBasedOrderDependency odUnderTest, IEnumerable<ConstantOrderDependency> constants, IEnumerable<OrderCompatibleDependency> compatibles, IEnumerable<Attribute> allAttributes)
    {
        // map the constants to fds
        if (SplitsExist(odUnderTest, constants, allAttributes))
            return false;
        return SwapsExist(odUnderTest, constants, compatibles, allAttributes);
    }
}