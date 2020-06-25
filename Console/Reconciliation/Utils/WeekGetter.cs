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

        public WeekGetter(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public Weeks Decide_num_weeks()
        {
            _input_output.Output_line(
                "Mathematical dude! Let's do some reconciliating. Type Exit at any time to leave (although to be honest I'm not sure that actually works...)");

            return new Weeks();
        }

        public int Num_weeks_between_dates(DateTime start_date, DateTime end_date)
        {
            Day_should_be(start_date, DayOfWeek.Saturday, "Start", "Saturday");
            Day_should_be(end_date, DayOfWeek.Friday, "End", "Friday");

            return 0;
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