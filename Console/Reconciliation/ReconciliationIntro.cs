using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation
{
    internal class ReconciliationIntro
    {
        private ISpreadsheetRepoFactory _spreadsheet_factory;
        private readonly IInputOutput _input_output;

        public ReconciliationIntro(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public void Start()
        {
            _input_output.Output_line("");
            _input_output.Output_options(new List<string>
            {
                ReconConsts.Load_pending_csvs,
                "2. Do actual reconciliation."
            });

            string input = _input_output.Get_generic_input(ReconConsts.PendingOrReconciliate);

            switch (input)
            {
                case "1":
                {
                    Create_pending_csvs();
                } 
                break;
                case "2":
                {
                    Do_actual_reconciliation();
                } 
                break;
            }
        }

        public void Do_actual_reconciliation()
        {
            try
            {
                ISpreadsheetRepoFactory spreadsheet_factory = new DebugModeSwitcher(_input_output).Decide_on_debug();
                Do_matching(spreadsheet_factory);
            }
            catch (Exception exception)
            {
                if (exception.Message.ToUpper() == "EXIT")
                {
                    _input_output.Output_line("Taking you back to the main screen so you can start again if you want.");
                }
                else
                {
                    _input_output.Show_error(exception);
                }
            }
        }

        private void Do_matching(ISpreadsheetRepoFactory spreadsheet_factory)
        {
            var main_file_paths = new PathSetter(_input_output, spreadsheet_factory).Set_path_and_file_names();
            main_file_paths.Matcher.Do_matching(main_file_paths);
        }

        private void Create_pending_csvs()
        {
            try
            {
                ISpreadsheetRepoFactory spreadsheet_factory = new FakeSpreadsheetRepoFactory();
                var path = new PathSetter(_input_output, spreadsheet_factory).Set_path();
                var pending_csv_file_creator = new PendingCsvFileCreator(path);
                pending_csv_file_creator.Create_and_populate_all_csvs();
            }
            catch (Exception e)
            {
                _input_output.Output_line(e.Message);
            }
        }
    }
}
