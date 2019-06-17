using System;
using Interfaces.DTOs;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Utils
{
    [TestFixture]
    public class BudgetingMonthsTests
    {
        [TestCase(1, 2, 2)]
        [TestCase(1, 5, 5)]
        [TestCase(12, 3, 4)]
        [TestCase(10, 1, 4)]
        [TestCase(6, 5, 12)]
        public void Will_calculate_num_budgeting_months_based_on_stored_data(
            int nextUnplannedMonth, 
            int lastMonthForBudgetPlanning, 
            int expectedResult)
        {
            // Arrange 
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = nextUnplannedMonth,
                Last_month_for_budget_planning = lastMonthForBudgetPlanning
            };

            // Act
            var result = budgeting_months.Num_budgeting_months();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Will_calculate_budgeting_start_date_using_next_unplanned_month_and_start_year()
        {
            // Arrange 
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = 2,
                Start_year = 2019
            };

            // Act
            var result = budgeting_months.Budgeting_start_date();

            // Assert
            Assert.AreEqual(result.Month, budgeting_months.Next_unplanned_month);
            Assert.AreEqual(result.Year, budgeting_months.Start_year);
        }

        [TestCase(12, 2, 2018, 2, 2019)]
        [TestCase(1, 4, 2019, 4, 2019)]
        public void Will_calculate_budgeting_end_date_using_budgeting_start_date_and_num_budgeting_months(
            int nextUnplannedMonth,
            int lastMonthForBudgetPlanning,
            int startYear,
            int expectedMonth,
            int expectedYear)
        {
            // Arrange 
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = nextUnplannedMonth,
                Last_month_for_budget_planning = lastMonthForBudgetPlanning,
                Start_year = startYear
            };
            
            // Act
            var result = budgeting_months.Budgeting_end_date();

            // Assert
            Assert.AreEqual(result.Month, expectedMonth);
            Assert.AreEqual(result.Year, expectedYear);
        }

        [TestCase(12, 2, 2018, 12, 2018, 2)]
        [TestCase(12, 2, 2018, 1, 2019, 1)]
        [TestCase(12, 2, 2018, 11, 2018, 3)]
        [TestCase(12, 2, 2018, 10, 2018, 4)]
        public void Will_calculate_months_to_plan_after_specified_date_using_num_budgeting_months_and_specified_date(
            int nextUnplannedMonth,
            int lastMonthForBudgetPlanning,
            int startYear,
            int specifiedMonth,
            int specifiedYear,
            int expectedResult)
        {
            // Arrange 
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = nextUnplannedMonth,
                Last_month_for_budget_planning = lastMonthForBudgetPlanning,
                Start_year = startYear
            };
            var specified_date = new DateTime(specifiedYear, specifiedMonth, 1);

            // Act
            var result = budgeting_months.Get_months_to_plan_after_specified_date(specified_date);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}