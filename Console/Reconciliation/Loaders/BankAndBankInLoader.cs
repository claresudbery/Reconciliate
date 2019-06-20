using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Reconciliators;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankInLoader
    {
        private readonly IInputOutput _input_output;
        private readonly ISpreadsheetRepoFactory _spreadsheet_factory;

        public BankAndBankInLoader(IInputOutput input_output, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            _input_output = input_output;
            _spreadsheet_factory = spreadsheet_factory;
        }

        public ReconciliationInterface
            Load(ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                FilePaths main_file_paths)
        {
            var data_loading_info = BankAndBankInData.LoadingInfo;
            data_loading_info.File_paths = main_file_paths;

            var pending_file_io = new FileIO<BankRecord>(_spreadsheet_factory);
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);
            var pending_file = new CSVFile<BankRecord>(pending_file_io);

            _input_output.Output_line(ReconConsts.LoadingDataFromPendingFile);
            pending_file.Load(true, data_loading_info.Default_separator);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            _input_output.Output_line("Converting source line separators...");
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator, data_loading_info.Loading_separator);

            _input_output.Output_line(ReconConsts.MergingSomeBudgetData);
            spreadsheet.Add_budgeted_bank_in_data_to_pending_file(
                budgeting_months,
                pending_file,
                data_loading_info.Monthly_budget_data);

            _input_output.Output_line("Merging bespoke data with pending file...");
            Bank_and_bank_in__Merge_bespoke_data_with_pending_file(
                _input_output,
                spreadsheet,
                pending_file,
                budgeting_months,
                data_loading_info);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _input_output.Output_line("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.Add_unreconciled_rows_to_csv_file(data_loading_info.Sheet_name, pending_file);
            _input_output.Output_line("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.Update_source_lines_for_output(data_loading_info.Loading_separator);
            pending_file.Write_to_file_as_source_lines(data_loading_info.File_paths.Owned_file_name);
            _input_output.Output_line("...");

            var third_party_file_io = new FileIO<ActualBankRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Third_party_file_name);
            var owned_file_io = new FileIO<BankRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);
            var reconciliator = new BankReconciliator(third_party_file_io, owned_file_io, data_loading_info);

            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor);

            return reconciliation_interface;
        }

        public void Bank_and_bank_in__Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation loading_info)
        {
            input_output.Output_line(ReconConsts.Loading_expenses);
            var expected_income_file_io = new FileIO<ExpectedIncomeRecord>(new FakeSpreadsheetRepoFactory());
            var expected_income_csv_file = new CSVFile<ExpectedIncomeRecord>(expected_income_file_io);
            expected_income_csv_file.Load(false);
            var expected_income_file = new ExpectedIncomeFile(expected_income_csv_file);
            spreadsheet.Add_unreconciled_rows_to_csv_file<ExpectedIncomeRecord>(MainSheetNames.Expected_in, expected_income_file.File);
            expected_income_csv_file.Populate_source_records_from_records();
            expected_income_file.Filter_for_employer_expenses_only();
            expected_income_file.Copy_to_pending_file(pending_file);
            expected_income_csv_file.Populate_records_from_original_file_load();
        }
    }
}