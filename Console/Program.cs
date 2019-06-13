using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleCatchall.Console.Reconciliation;
using Interfaces.Constants;

namespace ConsoleCatchall.Console
{
    class Program
    {
        private static InputOutput _inputOutput = new InputOutput();
        static void Main(string[] args)
        {
            SetCorrectDateFormatting();

            // If running in .Net Core mode, you'll use args to pass in the path to your main config.
            // (particularly important on a Mac, where C:/Config will not work)
            if (args.Length > 0)
            {
                FilePathConsts.ConfigFilePath = args[0];
            }

            try
            {
                var options = new List<string>
                {
                    "1. Reconciliate!",
                    "2. Exit",
                };
                _inputOutput.OutputOptions(options);
                string input = System.Console.ReadLine();

                while (input != "2" && input.ToUpper() != "EXIT")
                {
                    switch (input)
                    {
                        case "1": new ReconciliationIntro(_inputOutput).Start(); break;
                    }

                    _inputOutput.OutputLine("");
                    _inputOutput.OutputOptions(options);
                    input = System.Console.ReadLine();
                }

                _inputOutput.OutputLine("Goodbye!");

                Thread.Sleep(System.TimeSpan.FromSeconds(5));
            }
            catch (Exception exception)
            {
                _inputOutput.OutputLine(exception.Message);
                _inputOutput.OutputLine("I'm just going to leave this screen up for a few seconds so you can read this message [drums fingers].");
                Thread.Sleep(System.TimeSpan.FromSeconds(10));
            }
        }

        public static void SetCorrectDateFormatting()
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-GB");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}
