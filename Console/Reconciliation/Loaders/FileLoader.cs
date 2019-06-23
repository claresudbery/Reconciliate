using System;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class FileLoader
    {
        private readonly IInputOutput _input_output;
        private readonly ISpreadsheetRepoFactory _spreadsheet_factory;

        public FileLoader(IInputOutput input_output, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            _input_output = input_output;
            _spreadsheet_factory = spreadsheet_factory;
        }

        public void Create_pending_csvs(string path)
        {
            try
            {
                var pending_csv_file_creator = new PendingCsvFileCreator(path);
                pending_csv_file_creator.Create_and_populate_all_csvs();
            }
            catch (Exception e)
            {
                _input_output.Output_line(e.Message);
            }
        }

        public ReconciliationInterface Load_specific_files_for_reconciliation_type(
            FilePaths main_file_paths,
            ReconciliationType reconciliation_type)
        {
            _input_output.Output_line("Loading data...");

            ReconciliationInterface reconciliation_interface = null;

            try
            {
                // NB This is the only function the spreadsheet is used in, until the very end (Reconciliator.Finish, called from
                // ReconciliationInterface), when another spreadsheet instance gets created by FileIO so it can call 
                // WriteBackToMainSpreadsheet. Between now and then, everything is done using csv files.
                var spreadsheet_repo = _spreadsheet_factory.Create_spreadsheet_repo();
                var spreadsheet = new Spreadsheet(spreadsheet_repo);
                var budgeting_month_service = new BudgetingMonthService(_input_output);
                BudgetingMonths budgeting_months = budgeting_month_service.Recursively_ask_for_budgeting_months(spreadsheet);

                switch (reconciliation_type)
                {
                    case ReconciliationType.BankAndBankIn:
                        {
                            var pending_file_io = new FileIO<BankRecord>(_spreadsheet_factory);
                            var pending_file = new CSVFile<BankRecord>(pending_file_io);
                            var third_party_file_io = new FileIO<ActualBankRecord>(_spreadsheet_factory);
                            var owned_file_io = new FileIO<BankRecord>(_spreadsheet_factory);
                            var data_loading_info = BankAndBankInData.LoadingInfo;
                            var loader = new BankAndBankInLoader(_input_output, _spreadsheet_factory);
                            reconciliation_interface = loader.Load(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths,
                                    pending_file_io,
                                    pending_file,
                                    third_party_file_io,
                                    owned_file_io,
                                    data_loading_info);
                        }
                        break;
                    case ReconciliationType.BankAndBankOut:
                        {
                            var pending_file_io = new FileIO<BankRecord>(_spreadsheet_factory);
                            var pending_file = new CSVFile<BankRecord>(pending_file_io);
                            var third_party_file_io = new FileIO<ActualBankRecord>(_spreadsheet_factory);
                            var owned_file_io = new FileIO<BankRecord>(_spreadsheet_factory);
                            var data_loading_info = BankAndBankOutData.LoadingInfo;
                            var loader = new BankAndBankOutLoader(_input_output, _spreadsheet_factory);
                            reconciliation_interface = loader.Load(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths,
                                    pending_file_io,
                                    pending_file,
                                    third_party_file_io,
                                    owned_file_io,
                                    data_loading_info);
                        }
                        break;
                    case ReconciliationType.CredCard1AndCredCard1InOut:
                        {
                            var pending_file_io = new FileIO<CredCard1InOutRecord>(_spreadsheet_factory);
                            var pending_file = new CSVFile<CredCard1InOutRecord>(pending_file_io);
                            var third_party_file_io = new FileIO<CredCard1Record>(_spreadsheet_factory);
                            var owned_file_io = new FileIO<CredCard1InOutRecord>(_spreadsheet_factory);
                            var data_loading_info = CredCard1AndCredCard1InOutData.LoadingInfo;
                            var loader = new CredCard1AndCredCard1InOutLoader(_input_output, _spreadsheet_factory);
                            reconciliation_interface = loader.Load(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths,
                                    pending_file_io,
                                    pending_file,
                                    third_party_file_io,
                                    owned_file_io,
                                    data_loading_info);
                        }
                        break;
                    case ReconciliationType.CredCard2AndCredCard2InOut:
                        {
                            var pending_file_io = new FileIO<CredCard2InOutRecord>(_spreadsheet_factory);
                            var pending_file = new CSVFile<CredCard2InOutRecord>(pending_file_io);
                            var third_party_file_io = new FileIO<CredCard2Record>(_spreadsheet_factory);
                            var owned_file_io = new FileIO<CredCard2InOutRecord>(_spreadsheet_factory);
                            var data_loading_info = CredCard2AndCredCard2InOutData.LoadingInfo;
                            var loader = new CredCard2AndCredCard2InOutLoader(_input_output, _spreadsheet_factory);
                            reconciliation_interface = loader.Load(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths,
                                    pending_file_io,
                                    pending_file,
                                    third_party_file_io,
                                    owned_file_io,
                                    data_loading_info);
                        }
                        break;
                    default:
                        {
                            _input_output.Output_line("I don't know what files to load! Terminating now.");
                        }
                        break;
                }
            }
            finally
            {
                _spreadsheet_factory.Dispose_of_spreadsheet_repo();
            }

            _input_output.Output_line("");
            _input_output.Output_line("");

            return reconciliation_interface;
        }
    }
}