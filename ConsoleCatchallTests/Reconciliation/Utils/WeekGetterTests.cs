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
        public void Will_find_previous_Saturday(
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

        [TestCase(10, 7, 10, 7)]
        [TestCase(11, 7, 10, 7)]
        [TestCase(12, 7, 10, 7)]
        [TestCase(13, 7, 10, 7)]
        [TestCase(14, 7, 10, 7)]
        [TestCase(15, 7, 10, 7)]
        [TestCase(16, 7, 10, 7)]
        [TestCase(17, 7, 17, 7)]
        public void Will_find_previous_Friday(
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
            DateTime result = week_getter.Find_previous_Friday(today);

            // Assert
            Assert.AreEqual(expected_day, result.Day);
            Assert.AreEqual(expected_month, result.Month);
        }

        [TestCase(4, 7, 2020, 10, 7, 2020, 1)]
        [TestCase(4, 7, 2020, 17, 7, 2020, 2)]
        [TestCase(4, 7, 2020, 24, 7, 2020, 3)]
        [TestCase(4, 7, 2020, 31, 7, 2020, 4)]
        [TestCase(4, 7, 2020, 7, 8, 2020, 5)]
        [TestCase(4, 7, 2020, 4, 9, 2020, 9)]
        [TestCase(5, 12, 2020, 1, 1, 2021, 4)]
        public void Will_calculate_num_weeks_between_dates(
            int start_day,
            int start_month,
            int start_year,
            int end_day,
            int end_month,
            int end_year,
            int expected_result)
        {
            // Arrange 
            DateTime start_date = new DateTime(start_year, start_month, start_day);
            DateTime end_date = new DateTime(end_year, end_month, end_day);
            var mock_input_output = new Mock<IInputOutput>();
            var week_getter = new WeekGetter(mock_input_output.Object);

            // Act
            int result = week_getter.Num_weeks_between_dates(start_date, end_date);

            // Assert
            Assert.AreEqual(expected_result, result);
        }

        [Test]
        public void Will_throw_exception_if_start_date_not_Saturday_when_calculating_weeks_between_dates()
        {
            // Arrange 
            DateTime start_date = new DateTime(2020, 7, 12);
            DateTime end_date = new DateTime(2020, 7, 31);
            var mock_input_output = new Mock<IInputOutput>();
            var week_getter = new WeekGetter(mock_input_output.Object);
            bool exception_thrown = false;
            var exception_message = "";

            // Act
            try
            {
                week_getter.Num_weeks_between_dates(start_date, end_date);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                exception_message = exception.Message;
            }

            // Assert
            Assert.AreEqual(true, exception_thrown);
            Assert.IsTrue(exception_message.Contains("Saturday"), "Exception message should refer to Saturday");
        }

        [Test]
        public void Will_throw_exception_if_end_date_not_Friday_when_calculating_weeks_between_dates()
        {
            // Arrange 
            DateTime start_date = new DateTime(2020, 7, 11);
            DateTime end_date = new DateTime(2020, 7, 30);
            var mock_input_output = new Mock<IInputOutput>();
            var week_getter = new WeekGetter(mock_input_output.Object);
            bool exception_thrown = false;
            var exception_message = "";

            // Act
            try
            {
                week_getter.Num_weeks_between_dates(start_date, end_date);
            }
            catch (Exception exception)
            {
                exception_thrown = true;
                exception_message = exception.Message;
            }

            // Assert
            Assert.AreEqual(true, exception_thrown);
            Assert.IsTrue(exception_message.Contains("Friday"), "Exception message should refer to Friday");
        }

        [Test]
        public void Will_ask_user_which_Saturday_to_start_on_when_deciding_num_weeks()
        {
        }

        [Test]
        public void Will_ask_user_which_Friday_to_end_on_when_deciding_num_weeks()
        {
        }
        
        [Test]
        public void Will_not_ask_user_which_Friday_to_end_on_when_month_ends_on_Friday()
        {
        }

        [TestCase(11, 7, 2020, 7, 2020, true, null, 3)]
        [TestCase(11, 7, 2020, 7, 2020, false, null, 2)]
        [TestCase(11, 7, 2020, 8, 2020, true, true, 7)]
        [TestCase(11, 7, 2020, 8, 2020, false, true, 6)]
        [TestCase(11, 7, 2020, 8, 2020, true, false, 8)]
        [TestCase(11, 7, 2020, 8, 2020, false, false, 7)]
        [TestCase(5, 12, 2020, 1, 2021, true, true, 8)]
        [TestCase(5, 12, 2020, 1, 2021, false, true, 7)]
        [TestCase(5, 12, 2020, 1, 2021, true, false, 9)]
        [TestCase(5, 12, 2020, 1, 2021, false, false, 8)]
        public void Will_calculate_correct_num_weeks_according_to_user_input(
            int start_day,
            int start_month,
            int start_year,
            int end_month,
            int end_year,
            bool choose_first_saturday,
            bool? choose_first_friday,
            int expected_result)
        {
        }

        [TestCase(11, 7, true, 11, 7)]
        [TestCase(13, 7, true, 11, 7)]
        [TestCase(11, 7, false, 18, 7)]
        [TestCase(13, 7, false, 18, 7)]
        public void Will_calculate_correct_start_date_according_to_user_input(
            int start_day,
            int start_month,
            bool choose_first_saturday,
            int expected_day,
            int expected_month)
        {
        }
    }
}