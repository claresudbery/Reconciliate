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
                    var path = new PathSetter(_input_output).Set_path();
                    ISpreadsheetRepoFactory spreadsheet_factory = new FakeSpreadsheetRepoFactory();
                    var file_loader = new FileLoader(_input_output, spreadsheet_factory);
                    file_loader.Create_pending_csvs(path);
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
            var file_loader = new FileLoader(_input_output, spreadsheet_factory);
            var reconciliation_interface = file_loader.Load_specific_files_for_reconciliation_type();
            reconciliation_interface?.Do_the_matching();
        }
    }
}
