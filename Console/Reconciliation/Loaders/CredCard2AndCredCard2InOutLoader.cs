using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class CredCard2AndCredCard2InOutLoader : ILoader<CredCard2Record, CredCard2InOutRecord>
    {
        public DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> Loading_info()
        {
            return new DataLoadingInformation<CredCard2Record, CredCard2InOutRecord>
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
                Sheet_name = MainSheetNames.Cred_card2,
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

        public IDataFile<CredCard2Record> Create_new_third_party_file(IFileIO<CredCard2Record> third_party_file_io)
        {
            var csv_file = new CSVFile<CredCard2Record>(third_party_file_io);
            return new GenericFile<CredCard2Record>(csv_file);
        }

        public IDataFile<CredCard2InOutRecord> Create_new_owned_file(IFileIO<CredCard2InOutRecord> owned_file_io)
        {
            var csv_file = new CSVFile<CredCard2InOutRecord>(owned_file_io);
            return new GenericFile<CredCard2InOutRecord>(csv_file);
        }

        public void Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<CredCard2InOutRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> data_loading_info)
        {
            var most_recent_cred_card_direct_debit = spreadsheet.Get_most_recent_row_containing_text<BankRecord>(
                MainSheetNames.Bank_out, 
                ReconConsts.Cred_card2_dd_description, 
                new List<int>{ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn});

            var statement_date = new DateTime();
            var next_date = most_recent_cred_card_direct_debit.Date.AddMonths(1);
            var input = input_output.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.Cred_card2_name,
                next_date.ToShortDateString()));
            double new_balance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out new_balance))
                {
                    new_balance = new_balance * -1;
                    pending_file.Records.Add(new CredCard2InOutRecord
                    {
                        Date = next_date,
                        Description = ReconConsts.Cred_card2_regular_pymt_description,
                        Unreconciled_amount = new_balance
                    });
                }
                statement_date = next_date.AddMonths(-1);
                next_date = next_date.Date.AddMonths(1);
                input = input_output.Get_input(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.Cred_card2_name, 
                    next_date.ToShortDateString()));
            }

            if (!new_balance.Double_equals(0))
            {
                spreadsheet.Update_balance_on_totals_sheet(
                    Codes.Cred_card2_bal,
                    new_balance,
                    string.Format(
                        ReconConsts.CredCardBalanceDescription,
                        ReconConsts.Cred_card2_name,
                        $"{statement_date.ToString("MMM")} {statement_date.Year}"),
                    balance_column: 5,
                    text_column: 6,
                    code_column: 4,
                    input_output: input_output);
            }
        }

        public void Generate_ad_hoc_data(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<CredCard2InOutRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> data_loading_info)
        {
        }

        public void Do_actions_which_require_third_party_data_access(
            IDataFile<CredCard2Record> third_party_file,
            IDataFile<CredCard2InOutRecord> owned_file,
            ISpreadsheet spreadsheet,
            IInputOutput input_output)
        {
        }
    }
}