using System;
using System.Collections.Generic;

namespace Interfaces.DTOs
{
    public class BudgetingMonths
    {
        public int NextUnplannedMonth { get; set; }
        public int LastMonthForBudgetPlanning { get; set; }
        public int StartYear { get; set; }

        public int NumBudgetingMonths()
        {
            return LastMonthForBudgetPlanning >= NextUnplannedMonth
                ? (LastMonthForBudgetPlanning - NextUnplannedMonth) + 1
                : (LastMonthForBudgetPlanning + 13) - NextUnplannedMonth;
        }

        public DateTime BudgetingStartDate()
        {
            return new DateTime(StartYear, NextUnplannedMonth, 1);
        }

        public DateTime BudgetingEndDate()
        {
            return BudgetingStartDate().AddMonths(NumBudgetingMonths() - 1);
        }

        public int GetMonthsToPlanAfterSpecifiedDate(DateTime previousDate)
        {
            int result = 0;
            var budgetingEndDate = BudgetingEndDate();

            while (previousDate < budgetingEndDate)
            {
                previousDate = previousDate.AddMonths(1);
                result++;
            }

            return result;
        }
    }
}