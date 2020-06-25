using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Utils
{
    internal class WeekGetter
    {
        private readonly IInputOutput _input_output;
        private readonly IClock _clock;

        public WeekGetter(IInputOutput input_output, IClock clock)
        {
            _input_output = input_output;
            _clock = clock;
        }

        public Weeks Decide_num_weeks(string context_description, BudgetingMonths budgeting_months)
        {
            var start_date = Find_previous_Saturday(_clock.Today_date_time());
            _input_output.Output_line($"1. {start_date.ToShortDateString()}");
            _input_output.Output_line($"2. {start_date.AddDays(7).ToShortDateString()}");
            var choice = _input_output.Get_input($"Which Saturday do you want to start with for {context_description}? Enter 1 or 2:");

            var end_date = budgeting_months.Budgeting_end_date().AddMonths(1).AddDays(-1);
            if (end_date.DayOfWeek != DayOfWeek.Friday)
            {
                end_date = Find_previous_Friday(DateTime.Today);
                _input_output.Output_line($"1. {end_date.ToShortDateString()}");
                _input_output.Output_line($"2. {end_date.AddDays(7).ToShortDateString()}");
                _input_output.Get_input($"Which Friday do you want to end with for {context_description}? Enter 1 or 2:");
            }
            
            return new Weeks
            {
                NumWeeks = Num_weeks_between_dates(start_date, end_date)
            };
        }

        public int Num_weeks_between_dates(DateTime start_date, DateTime end_date)
        {
            Day_should_be(start_date, DayOfWeek.Saturday, "Start", "Saturday");
            Day_should_be(end_date, DayOfWeek.Friday, "End", "Friday");

            return (end_date.AddDays(1) - start_date).Duration().Days / 7;
        }

        private void Day_should_be(DateTime date, DayOfWeek expected_day, string date_description, string day_description)
        {
            if (date.DayOfWeek != expected_day)
            {
                throw new Exception($"{date_description} date should be a {day_description} when calculating num weeks.");
            }
        }

        public DateTime Find_previous_Saturday(DateTime today)
        {
            return today.AddDays(Days_to_subtract_for_Saturday(today) * -1);
        }

        private int Days_to_subtract_for_Saturday(DateTime today)
        {
            return (Convert.ToInt16(today.DayOfWeek) + 1) % 7;
        }

        public DateTime Find_previous_Friday(DateTime today)
        {
            return today.AddDays(Days_to_subtract_for_Friday(today) * -1);
        }

        private int Days_to_subtract_for_Friday(DateTime today)
        {
            return (Convert.ToInt16(today.DayOfWeek) + 2) % 7;
        }
    }
}