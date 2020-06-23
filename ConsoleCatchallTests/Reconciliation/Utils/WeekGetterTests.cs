using System;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Utils
{
    [TestFixture]
    public class WeekGetterTests
    {
        [TestCase(11, 7, 11, 7)]
        [TestCase(12, 7, 11, 7)]
        [TestCase(13, 7, 11, 7)]
        [TestCase(14, 7, 11, 7)]
        [TestCase(15, 7, 11, 7)]
        [TestCase(16, 7, 11, 7)]
        [TestCase(17, 7, 11, 7)]
        [TestCase(18, 7, 18, 7)]
        public void Will_find_previous_Saturday_in_2020(
            int this_day,
            int this_month,
            int expected_day,
            int expected_month)
        {
            // Arrange 
            DateTime today = new DateTime(2020, this_month, this_day);
            var mock_input_output = new Mock<IInputOutput>();
            var week_getter = new WeekGetter(mock_input_output.Object);

            // Act
            DateTime result = week_getter.Find_previous_Saturday(today);

            // Assert
            Assert.AreEqual(expected_day, result.Day);
            Assert.AreEqual(expected_month, result.Month);
        }
    }
}