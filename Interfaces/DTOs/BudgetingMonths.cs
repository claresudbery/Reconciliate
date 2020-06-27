using System;

namespace Interfaces.DTOs
{
    public class BudgetingMonths
    {
        public int Next_unplanned_month { get; set; }
        public int Last_month_for_budget_planning { get; set; }
        public int Start_year { get; set; }

        public bool Budgeting_not_required()
        {
            return Last_month_for_budget_planning == 0;
        }

        public bool Budgeting_required()
        {
            return Last_month_for_budget_planning != 0;
        }

        public int Num_budgeting_months()
        {
            return Budgeting_not_required() 
                ? 0
                : Last_month_for_budget_planning >= Next_unplanned_month
                    ? (Last_month_for_budget_planning - Next_unplanned_month) + 1
                    : (Last_month_for_budget_planning + 13) - Next_unplanned_month;
        }

        public DateTime Budgeting_start_date()
        {
            return new DateTime(Start_year, Next_unplanned_month, 1);
        }

        public DateTime Budgeting_end_date()
        {
            return Budgeting_start_date().AddMonths(Num_budgeting_months() - 1);
        }

        public int Get_months_to_plan_after_specified_date(DateTime previous_date)
        {
            int result = 0;

            if (Budgeting_required())
            {
                var budgeting_end_date = Budgeting_end_date();

                while (previous_date < budgeting_end_date)
                {
                    previous_date = previous_date.AddMonths(1);
                    result++;
                }
            }

            return result;
        }
    }
}