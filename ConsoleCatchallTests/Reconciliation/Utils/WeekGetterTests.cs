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
            var week_getter = new WeekGetter(mock_input_output.Object, new Mock<IClock>().Object);

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
            var week_getter = new WeekGetter(mock_input_output.Object, new Mock<IClock>().Object);

            // Act
            DateTime result = week_getter.Find_previous_Friday(today);

            // Assert
            Assert.AreEqual(expected_day, result.Day);
            Assert.AreEqual(expected_month, result.Month);
        }

        [Test]
        public void Will_throw_exception_if_start_date_not_Saturday_when_calculating_weeks_between_dates()
        {
            // Arrange 
            DateTime start_date = new DateTime(2020, 7, 12);
            DateTime end_date = new DateTime(2020, 7, 31);
            var mock_input_output = new Mock<IInputOutput>();
            var week_getter = new WeekGetter(mock_input_output.Object, new Mock<IClock>().Object);
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
            var week_getter = new WeekGetter(mock_input_output.Object, new Mock<IClock>().Object);
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
            var week_getter = new WeekGetter(mock_input_output.Object, new Mock<IClock>().Object);

            // Act
            int result = week_getter.Num_weeks_between_dates(start_date, end_date);

            // Assert
            Assert.AreEqual(expected_result, result, $"Expected {expected_result} but got {result}");
        }

        [Test]
        public void Will_ask_user_which_Saturday_to_start_on_when_deciding_num_weeks()
        {
            // Arrange 
            var mock_input_output = new Mock<IInputOutput>();
            var mock_clock = new Mock<IClock>();
            mock_clock.Setup(x => x.Today_date_time()).Returns(new DateTime(2020, 6, 1));
            var week_getter = new WeekGetter(mock_input_output.Object, mock_clock.Object);
            var budgeting_months_not_ending_friday = new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 };

            // Act
            week_getter.Decide_num_weeks("Testing, testing...", budgeting_months_not_ending_friday);

            // Assert
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("Saturday")), ""));
        }

        [Test]
        public void Will_ask_user_which_Friday_to_end_on_when_deciding_num_weeks()
        {
            // Arrange 
            var mock_input_output = new Mock<IInputOutput>();
            var mock_clock = new Mock<IClock>();
            mock_clock.Setup(x => x.Today_date_time()).Returns(new DateTime(2020, 6, 1));
            var week_getter = new WeekGetter(mock_input_output.Object, mock_clock.Object);
            var budgeting_months_not_ending_friday = new BudgetingMonths { Start_year = 2020, Next_unplanned_month = 6, Last_month_for_budget_planning = 6 };

            // Act
            week_getter.Decide_num_weeks("Testing, testing...", budgeting_months_not_ending_friday);

            // Assert
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("Friday")), ""));
        }
        
        [Test]
        public void Will_not_ask_user_which_Friday_to_end_on_when_month_ends_on_Friday()
        {
            // Arrange 
            var mock_input_output = new Mock<IInputOutput>();
            var mock_clock = new Mock<IClock>();
            mock_clock.Setup(x => x.Today_date_time()).Returns(new DateTime(2020, 6, 1));
            var week_getter = new WeekGetter(mock_input_output.Object, mock_clock.Object);
            var budgeting_months_ending_friday = new BudgetingMonths
            {
                Start_year = 2020,
                Next_unplanned_month = 7,
                Last_month_for_budget_planning = 7
            };

            // Act
            week_getter.Decide_num_weeks("Testing, testing...", budgeting_months_ending_friday);

            // Assert
            mock_input_output.Verify(x => x.Get_input(
                It.Is<string>(y => y.Contains("Friday")), ""), 
                Times.Never);
        }

        [TestCase(11, 7, 2020, 7, 2020, true, false, 3)]
        [TestCase(11, 7, 2020, 7, 2020, false, false, 2)]
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
            bool choose_first_friday,
            int expected_result)
        {
            // Arrange
            DateTime start_date = new DateTime(start_year, start_month, start_day);
            var mock_input_output = new Mock<IInputOutput>();
            var mock_clock = new Mock<IClock>();
            var week_getter = new WeekGetter(mock_input_output.Object, mock_clock.Object);
            var budgeting_months_not_ending_friday = new BudgetingMonths
            {
                Start_year = start_year, 
                Next_unplanned_month = start_month, 
                Last_month_for_budget_planning = end_month
            };
            mock_input_output.Setup(x => x.Get_input(
                    It.Is<string>(y => y.Contains("Sat")), 
                    It.IsAny<string>()))
                .Returns(choose_first_saturday ? "1" : "2");
            mock_input_output.Setup(x => x.Get_input(
                    It.Is<string>(y => y.Contains("Fri")),
                    It.IsAny<string>()))
                .Returns(choose_first_friday ? "1" : "2");
            mock_clock.Setup(x => x.Today_date_time()).Returns(start_date);

            // Act
            var result = week_getter.Decide_num_weeks("Testing, testing...", budgeting_months_not_ending_friday);

            // Assert
            Assert.AreEqual(expected_result, result.NumWeeks);
        }

        [Test]
        public void Dummy_test()
        {
            int start_day = 5;
            int start_month = 12;
            int start_year = 2020;
            int end_month = 1;
            int end_year = 2021;
            bool choose_first_saturday = true;
            bool choose_first_friday = false;
            int expected_result = 9;

            // Arrange
            DateTime start_date = new DateTime(start_year, start_month, start_day);
            var mock_input_output = new Mock<IInputOutput>();
            var mock_clock = new Mock<IClock>();
            var week_getter = new WeekGetter(mock_input_output.Object, mock_clock.Object);
            var budgeting_months_not_ending_friday = new BudgetingMonths
            {
                Start_year = start_year,
                Next_unplanned_month = start_month,
                Last_month_for_budget_planning = end_month
            };
            mock_input_output.Setup(x => x.Get_input(
                    It.Is<string>(y => y.Contains("Sat")),
                    It.IsAny<string>()))
                .Returns(choose_first_saturday ? "1" : "2");
            mock_input_output.Setup(x => x.Get_input(
                    It.Is<string>(y => y.Contains("Fri")),
                    It.IsAny<string>()))
                .Returns(choose_first_friday ? "1" : "2");
            mock_clock.Setup(x => x.Today_date_time()).Returns(start_date);

            // Act
            var result = week_getter.Decide_num_weeks("Testing, testing...", budgeting_months_not_ending_friday);

            // Assert
            Assert.AreEqual(expected_result, result.NumWeeks);
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