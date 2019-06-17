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
        public void WillCalculateNumBudgetingMonthsBasedOnStoredData(
            int nextUnplannedMonth, 
            int lastMonthForBudgetPlanning, 
            int expectedResult)
        {
            // Arrange 
            var budgeting_months = new BudgetingMonths
            {
                NextUnplannedMonth = nextUnplannedMonth,
                LastMonthForBudgetPlanning = lastMonthForBudgetPlanning
            };

            // Act
            var result = budgeting_months.NumBudgetingMonths();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void WillCalculateBudgetingStartDateUsingNextUnplannedMonthAndStartYear()
        {
            // Arrange 
            var budgeting_months = new BudgetingMonths
            {
                NextUnplannedMonth = 2,
                StartYear = 2019
            };

            // Act
            var result = budgeting_months.BudgetingStartDate();

            // Assert
            Assert.AreEqual(result.Month, budgeting_months.NextUnplannedMonth);
            Assert.AreEqual(result.Year, budgeting_months.StartYear);
        }

        [TestCase(12, 2, 2018, 2, 2019)]
        [TestCase(1, 4, 2019, 4, 2019)]
        public void WillCalculateBudgetingEndDateUsingBudgetingStartDateAndNumBudgetingMonths(
            int nextUnplannedMonth,
            int lastMonthForBudgetPlanning,
            int startYear,
            int expectedMonth,
            int expectedYear)
        {
            // Arrange 
            var budgeting_months = new BudgetingMonths
            {
                NextUnplannedMonth = nextUnplannedMonth,
                LastMonthForBudgetPlanning = lastMonthForBudgetPlanning,
                StartYear = startYear
            };
            
            // Act
            var result = budgeting_months.BudgetingEndDate();

            // Assert
            Assert.AreEqual(result.Month, expectedMonth);
            Assert.AreEqual(result.Year, expectedYear);
        }

        [TestCase(12, 2, 2018, 12, 2018, 2)]
        [TestCase(12, 2, 2018, 1, 2019, 1)]
        [TestCase(12, 2, 2018, 11, 2018, 3)]
        [TestCase(12, 2, 2018, 10, 2018, 4)]
        public void WillCalculateMonthsToPlanAfterSpecifiedDateUsingNumBudgetingMonthsAndSpecifiedDate(
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
                NextUnplannedMonth = nextUnplannedMonth,
                LastMonthForBudgetPlanning = lastMonthForBudgetPlanning,
                StartYear = startYear
            };
            var specified_date = new DateTime(specifiedYear, specifiedMonth, 1);

            // Act
            var result = budgeting_months.GetMonthsToPlanAfterSpecifiedDate(specified_date);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}