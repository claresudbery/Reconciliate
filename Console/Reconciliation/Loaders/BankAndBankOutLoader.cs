using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankOutLoader : ILoader<ActualBankRecord, BankRecord>
    {
        public DataLoadingInformation<ActualBankRecord, BankRecord> Loading_info()
        {
            return new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_bank_file_name,
                    Owned_file_name = ReconConsts.DefaultBankOutFileName
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.DefaultBankOutPendingFileName,
                Sheet_name = MainSheetNames.Bank_out,
                Third_party_descriptor = ReconConsts.Bank_descriptor,
                Owned_file_descriptor = ReconConsts.BankOutDescriptor,
                Loader = this,
                Monthly_budget_data = new BudgetItemListData
                {
                    Sheet_name = MainSheetNames.Budget_out,
                    Start_divider = Dividers.Sod_ds,
                    End_divider = Dividers.Cred_card1,
                    First_column_number = 2,
                    Last_column_number = 6
                },
                Annual_budget_data = new BudgetItemListData
                {
                    Sheet_name = MainSheetNames.Budget_out,
                    Start_divider = Dividers.Annual_sod_ds,
                    End_divider = Dividers.Annual_total,
                    First_column_number = 2,
                    Last_column_number = 6
                }
            };
        }

        public IDataFile<ActualBankRecord> Create_new_third_party_file(IFileIO<ActualBankRecord> third_party_file_io)
        {
            var csv_file = new CSVFile<ActualBankRecord>(third_party_file_io);
            return new ActualBankOutFile(csv_file);
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
            Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                pending_file,
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description);

            Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                pending_file,
                ReconConsts.Cred_card2_name,
                ReconConsts.Cred_card2_dd_description);
        }

        private void Add_most_recent_credit_card_direct_debits(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            string cred_card_name,
            string direct_debit_description)
        {
            var most_recent_cred_card_direct_debit = spreadsheet.Get_most_recent_row_containing_text<BankRecord>(
                MainSheetNames.Bank_out, 
                direct_debit_description,
                new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn});

            var next_date = most_recent_cred_card_direct_debit.Date.AddMonths(1);
            var input = input_output.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                cred_card_name,
                next_date.ToShortDateString()));
            while (input != "0")
            {
                double amount;
                if (double.TryParse(input, out amount))
                {
                    pending_file.Records.Add(new BankRecord
                    {
                        Date = next_date,
                        Description = direct_debit_description,
                        Type = "POS",
                        Unreconciled_amount = amount
                    });
                }
                next_date = next_date.Date.AddMonths(1);
                input = input_output.Get_input(string.Format(
                    ReconConsts.AskForCredCardDirectDebit, 
                    cred_card_name,
                    next_date.ToShortDateString()));
            }
        }
    }
}