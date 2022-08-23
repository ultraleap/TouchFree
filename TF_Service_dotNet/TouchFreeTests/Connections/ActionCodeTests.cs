using NUnit.Framework;
using System;
using Ultraleap.TouchFree.Library.Connections;

namespace TouchFreeTests.Connections
{
    public class ActionCodeTests
    {
        public static ActionCode[] GetActionCodes()
        {
            return Enum.GetValues<ActionCode>();
        }

        [TestCaseSource(nameof(GetActionCodes))]
        public void AllActionCodesShouldBeHandledOrUnexpected(ActionCode actionCode)
        {
            // Given

            // When
            var inHandledCodes = actionCode.ExpectedToBeHandled();
            var inUnexpectedCodes = actionCode.UnexpectedByTheService();

            // Then
            Assert.IsTrue(inHandledCodes || inUnexpectedCodes);
        }
    }
}
