using System;
using ConsoleCatchall.Console.Reconciliation.Utils;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Utils
{
    [TestFixture]
    public class StringHelperTests
    {
        [Test]
        public void WillPadOutWithExtraStringFieldsIfThereArentEnough()
        {
            // Arrange
            string[] values = { "One", "Two", "Three" };
            int minValuesRequired = 5;

            // Act
            string[] newValues = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(minValuesRequired, values);

            // Assert
            Assert.AreEqual(minValuesRequired, newValues.Length);
            Assert.AreEqual(newValues[0], "One");
            Assert.AreEqual(newValues[1], "Two");
            Assert.AreEqual(newValues[2], "Three");
            Assert.AreEqual(newValues[3], String.Empty);
            Assert.AreEqual(newValues[4], String.Empty);
        }

        [Test]
        public void WillNotPadOutWithExtraStringFieldsIfThereAreAlreadyEnough()
        {
            // Arrange
            string[] values = { "One", "Two", "Three" };
            int minValuesRequired = 2;

            // Act
            string[] newValues = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(minValuesRequired, values);

            // Assert
            Assert.AreEqual(3, newValues.Length);
            Assert.AreEqual(newValues[0], "One");
            Assert.AreEqual(newValues[1], "Two");
            Assert.AreEqual(newValues[2], "Three");
        }
    }
}