using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankInLoader : IBankAndBankInLoader, ILoader<ActualBankRecord, BankRecord>
    {
        private ExpectedIncomeFile _expected_income_file;
        private CSVFile<ExpectedIncomeRecord> _expected_income_csv_file;

        public BankAndBankInLoader(ISpreadsheetRepoFactory spreadsheet_repo_factory)
        {
            var expected_income_file_io = new FileIO<ExpectedIncomeRecord>(spreadsheet_repo_factory);
            _expected_income_csv_file = new CSVFile<ExpectedIncomeRecord>(expected_income_file_io);
            _expected_income_file = new ExpectedIncomeFile(_expected_income_csv_file);
        }

        public DataLoadingInformation<ActualBankRecord, BankRecord> Loading_info()
        { 
            return new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_bank_file_name,
                    Owned_file_name = ReconConsts.DefaultBankInFileName
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.DefaultBankInPendingFileName,
                Sheet_name = MainSheetNames.Bank_in,
                Third_party_descriptor = ReconConsts.Bank_descriptor,
                Owned_file_descriptor = ReconConsts.BankInDescriptor,
                Loader = this,
                Monthly_budget_data = new BudgetItemListData
                {
                    Budget_sheet_name = MainSheetNames.Budget_in,
                    Owned_sheet_name = MainSheetNames.Bank_in,
                    Start_divider = Dividers.Date,
                    End_divider = Dividers.Total,
                    First_column_number = 2,
                    Last_column_number = 6,
                    Third_party_desc_col = 15
                },
                Annual_budget_data = null
            };
        }

        public IDataFile<ActualBankRecord> Create_new_third_party_file(IFileIO<ActualBankRecord> third_party_file_io)
        {
            var csv_file = new CSVFile<ActualBankRecord>(third_party_file_io);
            return new ActualBankInFile(csv_file);
        }

        public IDataFile<BankRecord> Create_new_owned_file(IFileIO<BankRecord> owned_file_io)
        {
            var csv_file = new CSVFile<BankRecord>(owned_file_io);
            return new GenericFile<BankRecord>(csv_file);
        }

        public void Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<ActualBankRecord, BankRecord> data_loading_info)
        {
            input_output.Output_line(ReconConsts.Loading_expenses);
            _expected_income_csv_file.Load(false);

            spreadsheet.Add_unreconciled_rows_to_csv_file<ExpectedIncomeRecord>(MainSheetNames.Expected_in, _expected_income_file.File);
            _expected_income_csv_file.Populate_source_records_from_records();
            _expected_income_file.Filter_for_employer_expenses_and_bank_transactions_only();

            _expected_income_file.Copy_to_pending_file(pending_file);
            _expected_income_csv_file.Populate_records_from_original_file_load();
        }

        public void Generate_ad_hoc_data(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<ActualBankRecord, BankRecord> data_loading_info)
        {
        }

        public void Update_expected_income_record_when_matched(ICSVRecord source_record, ICSVRecord matched_record)
        {
            _expected_income_file.Update_expected_income_record_when_matched(source_record, matched_record);
        }

        public void Create_new_expenses_record_to_match_balance(ICSVRecord source_record, double balance)
        {
            _expected_income_file.Create_new_expenses_record_to_match_balance(source_record, balance);
        }

        public void Finish()
        {
            _expected_income_file.Finish();
        }

        public void Do_actions_which_require_third_party_data_access(
            IDataFile<ActualBankRecord> third_party_file,
            IDataFile<BankRecord> owned_file,
            ISpreadsheet spreadsheet,
            IInputOutput input_output)
        {
        }
    }
}