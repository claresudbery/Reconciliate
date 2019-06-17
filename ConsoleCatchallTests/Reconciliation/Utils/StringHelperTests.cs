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
            string[] new_values = StringHelper.Make_sure_there_are_at_least_enough_string_values(min_values_required, values);

            // Assert
            Assert.AreEqual(min_values_required, new_values.Length);
            Assert.AreEqual(new_values[0], "One");
            Assert.AreEqual(new_values[1], "Two");
            Assert.AreEqual(new_values[2], "Three");
            Assert.AreEqual(new_values[3], String.Empty);
            Assert.AreEqual(new_values[4], String.Empty);
        }

        [Test]
        public void Will_not_pad_out_with_extra_string_fields_if_there_are_already_enough()
        {
            // Arrange
            string[] values = { "One", "Two", "Three" };
            int min_values_required = 2;

            // Act
            string[] new_values = StringHelper.Make_sure_there_are_at_least_enough_string_values(min_values_required, values);

            // Assert
            Assert.AreEqual(3, new_values.Length);
            Assert.AreEqual(new_values[0], "One");
            Assert.AreEqual(new_values[1], "Two");
            Assert.AreEqual(new_values[2], "Three");
        }
    }
}