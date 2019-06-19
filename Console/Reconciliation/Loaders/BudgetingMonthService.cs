using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BudgetingMonthService
    {
        private readonly IInputOutput _input_output;

        public BudgetingMonthService(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public BudgetingMonths Recursively_ask_for_budgeting_months(ISpreadsheet spreadsheet)
        {
            DateTime next_unplanned_month = Get_next_unplanned_month(spreadsheet);
            int last_month_for_budget_planning = Get_last_month_for_budget_planning(spreadsheet, next_unplanned_month.Month);
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = next_unplanned_month.Month,
                Last_month_for_budget_planning = last_month_for_budget_planning,
                Start_year = next_unplanned_month.Year
            };
            if (last_month_for_budget_planning != 0)
            {
                budgeting_months.Last_month_for_budget_planning = Confirm_budgeting_month_choices_with_user(budgeting_months, spreadsheet);
            }
            return budgeting_months;
        }

        private DateTime Get_next_unplanned_month(ISpreadsheet spreadsheet)
        {
            DateTime default_month = DateTime.Today;
            DateTime next_unplanned_month = default_month;
            bool bad_input = false;
            try
            {
                next_unplanned_month = spreadsheet.Get_next_unplanned_month();
            }
            catch (Exception)
            {
                string new_month = _input_output.Get_input(ReconConsts.CantFindMortgageRow);
                try
                {
                    if (!String.IsNullOrEmpty(new_month) && Char.IsDigit(new_month[0]))
                    {
                        int actual_month = Convert.ToInt32(new_month);
                        if (actual_month < 1 || actual_month > 12)
                        {
                            bad_input = true;
                        }
                        else
                        {
                            var year = default_month.Year;
                            if (actual_month < default_month.Month)
                            {
                                year++;
                            }
                            next_unplanned_month = new DateTime(year, actual_month, 1);
                        }
                    }
                    else
                    {
                        bad_input = true;
                    }
                }
                catch (Exception)
                {
                    bad_input = true;
                }
            }

            if (bad_input)
            {
                _input_output.Output_line(ReconConsts.DefaultUnplannedMonth);
                next_unplanned_month = default_month;
            }

            return next_unplanned_month;
        }

        private int Get_last_month_for_budget_planning(ISpreadsheet spreadsheet, int next_unplanned_month)
        {
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(next_unplanned_month);
            var request_to_enter_month = String.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            string month = _input_output.Get_input(request_to_enter_month);
            int result = 0;

            try
            {
                if (!String.IsNullOrEmpty(month) && Char.IsDigit(month[0]))
                {
                    result = Convert.ToInt32(month);
                    if (result < 1 || result > 12)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore it and return zero by default.
            }

            result = Handle_zero_month_choice_result(result, spreadsheet, next_unplanned_month);
            return result;
        }

        private int Confirm_budgeting_month_choices_with_user(BudgetingMonths budgeting_months, ISpreadsheet spreadsheet)
        {
            var new_result = budgeting_months.Last_month_for_budget_planning;
            string input = Get_response_to_budgeting_months_confirmation_message(budgeting_months);

            if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
            {
                // I know this doesn't really do anything but I found the if statement easier to parse this way round.
                new_result = budgeting_months.Last_month_for_budget_planning;
            }
            else
            {
                // Recursion ftw!
                new_result = Get_last_month_for_budget_planning(spreadsheet, budgeting_months.Next_unplanned_month);
            }

            return new_result;
        }

        private string Get_response_to_budgeting_months_confirmation_message(BudgetingMonths budgeting_months)
        {
            string first_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgeting_months.Next_unplanned_month);
            string second_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgeting_months.Last_month_for_budget_planning);

            int month_span = budgeting_months.Num_budgeting_months();

            var confirmation_text = String.Format(ReconConsts.ConfirmMonthInterval, first_month, second_month, month_span);

            return _input_output.Get_input(confirmation_text);
        }

        private int Handle_zero_month_choice_result(int chosen_month, ISpreadsheet spreadsheet, int next_unplanned_month)
        {
            var new_result = chosen_month;
            if (chosen_month == 0)
            {
                var input = _input_output.Get_input(ReconConsts.ConfirmBadMonth);

                if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    new_result = 0;
                    _input_output.Output_line(ReconConsts.ConfirmNoMonthlyBudgeting);
                }
                else
                {
                    // Recursion ftw!
                    new_result = Get_last_month_for_budget_planning(spreadsheet, next_unplanned_month);
                }
            }
            return new_result;
        }
    }
}