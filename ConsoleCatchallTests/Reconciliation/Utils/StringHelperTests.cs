using System;
using ConsoleCatchall.Console.Reconciliation.Utils;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Utils
{
    [TestFixture]
    public class StringHelperTests
    {
        [Test]
        public void Will_pad_out_with_extra_string_fields_if_there_arent_enough()
        {
            // Arrange
            string[] values = { "One", "Two", "Three" };
            int min_values_required = 5;

            // Act
            string[] new_values = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(minValuesRequired, values);

            // Assert
            Assert.AreEqual(minValuesRequired, newValues.Length);
            Assert.AreEqual(newValues[0], "One");
            Assert.AreEqual(newValues[1], "Two");
            Assert.AreEqual(newValues[2], "Three");
            Assert.AreEqual(newValues[3], String.Empty);
            Assert.AreEqual(newValues[4], String.Empty);
        }

        [Test]
        public void Will_not_pad_out_with_extra_string_fields_if_there_are_already_enough()
        {
            // Arrange
            string[] values = { "One", "Two", "Three" };
            int min_values_required = 2;

            // Act
            string[] new_values = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(minValuesRequired, values);

            // Assert
            Assert.AreEqual(3, newValues.Length);
            Assert.AreEqual(newValues[0], "One");
            Assert.AreEqual(newValues[1], "Two");
            Assert.AreEqual(newValues[2], "Three");
        }
    }
}