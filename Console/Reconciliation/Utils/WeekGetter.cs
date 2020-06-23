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

        public DateTime Find_previous_Saturday(DateTime today)
        {
            return today.AddDays(Days_to_subtract(today) * -1);
        }

        private int Days_to_subtract(DateTime today)
        {
            return (Convert.ToInt16(today.DayOfWeek) + 1) % 7;
        }
    }
}