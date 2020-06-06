using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class DummyLoader : ILoader<ActualBankRecord, BankRecord>
    {
        public DataLoadingInformation<ActualBankRecord, BankRecord> Loading_info()
        {
            return new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_cred_card2_file_name,
                    Owned_file_name = ReconConsts.Default_cred_card2_in_out_file_name
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.Default_cred_card2_in_out_pending_file_name,
                Sheet_name = MainSheetNames.Bank_out,
                Third_party_descriptor = ReconConsts.Cred_card2_descriptor,
                Owned_file_descriptor = ReconConsts.Cred_card2_in_out_descriptor,
                Loader = this,
                Monthly_budget_data = new BudgetItemListData
                {
                    Budget_sheet_name = MainSheetNames.Budget_out,
                    Owned_sheet_name = MainSheetNames.Cred_card2,
                    Start_divider = Dividers.Cred_card2,
                    End_divider = Dividers.SODD_total,
                    First_column_number = 2,
                    Last_column_number = 5,
                    Third_party_desc_col = 10
                },
                Annual_budget_data = null
            };
        }

        public IDataFile<ActualBankRecord> Create_new_third_party_file(IFileIO<ActualBankRecord> third_party_file_io)
        {
            var csv_file = new CSVFile<ActualBankRecord>(third_party_file_io);
            return new GenericFile<ActualBankRecord>(csv_file);
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