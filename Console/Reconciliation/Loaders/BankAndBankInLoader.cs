using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankInLoader : ILoader
    {
        private readonly IInputOutput _input_output;

        public BankAndBankInLoader(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public void Load_files_and_merge_data(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            var loading_info = BankAndBankInData.LoadingInfo;
            loading_info.File_paths = main_file_paths;
            var file_loader = new FileLoader(_input_output);
            var reconciliation_interface = file_loader.Load_files_and_merge_data<ActualBankRecord, BankRecord>(
                loading_info, this, spreadsheet_factory);
            reconciliation_interface?.Do_the_matching();
        }

        public void Merge_bespoke_data_with_pending_file<TOwnedType>(
                IInputOutput input_output,
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation loading_info)
            where TOwnedType : ICSVRecord, new()
        {
            var expected_income_file_io = new FileIO<ExpectedIncomeRecord>(new FakeSpreadsheetRepoFactory());
            var expected_income_csv_file = new CSVFile<ExpectedIncomeRecord>(expected_income_file_io);
            var expected_income_file = new ExpectedIncomeFile(expected_income_csv_file);

            input_output.Output_line(ReconConsts.Loading_expenses);
            expected_income_csv_file.Load(false);

            spreadsheet.Add_unreconciled_rows_to_csv_file<ExpectedIncomeRecord>(MainSheetNames.Expected_in, expected_income_file.File);
            expected_income_csv_file.Populate_source_records_from_records();
            expected_income_file.Filter_for_employer_expenses_only();

            expected_income_file.Copy_to_pending_file((ICSVFile<BankRecord>)pending_file);
            expected_income_csv_file.Populate_records_from_original_file_load();
        }
    }
}