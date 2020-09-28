using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ConsoleCatchall.Console.Reconciliation;

namespace ConsoleCatchall.Console
{
    class Program
    {
        private static InputOutput _inputOutput = new InputOutput();
        static void Main(string[] args)
        {
            Set_correct_date_formatting();

            for (int n = 0; n <= 4; n++)
            {
                _inputOutput.Output($"Prime Factors of {n}: {getfactors(n)}\n");
            }

            string input = System.Console.ReadLine();

            // If running in .Net Core mode, you'll use args to pass in the path to your main config.
            // (particularly important on a Mac, where C:/Config will not work)
            //if (args.Length > 0)
            //{
            //    FilePathConsts.ConfigFilePath = args[0];
            //}

            //try
            //{
            //    var options = new List<string>
            //    {
            //        "1. Reconciliate!",
            //        "2. Exit",
            //    };
            //    _inputOutput.Output_options(options);
            //    string input = System.Console.ReadLine();

            //    while (input != "2" && input.ToUpper() != "EXIT")
            //    {
            //        switch (input)
            //        {
            //            case "1": new ReconciliationIntro(_inputOutput).Start(); break;
            //        }

            //        _inputOutput.Output_line("");
            //        _inputOutput.Output_options(options);
            //        input = System.Console.ReadLine();
            //    }

            //    _inputOutput.Output_line("Goodbye!");

            //    Thread.Sleep(System.TimeSpan.FromSeconds(5));
            //}
            //catch (Exception exception)
            //{
            //    _inputOutput.Output_line(exception.Message);
            //    _inputOutput.Output_line("I'm just going to leave this screen up for a few seconds so you can read this message [drums fingers].");
            //    Thread.Sleep(System.TimeSpan.FromSeconds(10));
            //}
        }

        public static void Set_correct_date_formatting()
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-GB");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }

        private static string getfactors(int n)
        {
            string factors = "";
            if (n > 1)
            {
                if (n % 2 == 0)
                {
                    factors += ", 2";
                    n /= 2;
                }
                if (n > 1)
                    factors += $", {n}";
            }
            return factors;
        }
    }
}
