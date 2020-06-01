using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class CredCard1AndCredCard1InOutLoader : ILoader<CredCard1Record, CredCard1InOutRecord>
    {
        public DataLoadingInformation<CredCard1Record, CredCard1InOutRecord> Loading_info()
        {
            return new DataLoadingInformation<CredCard1Record, CredCard1InOutRecord>
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_cred_card1_file_name,
                    Owned_file_name = ReconConsts.Default_cred_card1_in_out_file_name
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.Default_cred_card1_in_out_pending_file_name,
                Sheet_name = MainSheetNames.Cred_card1,
                Third_party_descriptor = ReconConsts.Cred_card1_descriptor,
                Owned_file_descriptor = ReconConsts.Cred_card1_in_out_descriptor,
                Loader = this,
                Monthly_budget_data = new BudgetItemListData
                {
                    Sheet_name = MainSheetNames.Budget_out,
                    Start_divider = Dividers.Cred_card1,
                    End_divider = Dividers.Cred_card2,
                    First_column_number = 2,
                    Last_column_number = 5
                },
                Annual_budget_data = null
            };
        }

        public IDataFile<CredCard1Record> Create_new_third_party_file(IFileIO<CredCard1Record> third_party_file_io)
        {
            var csv_file = new CSVFile<CredCard1Record>(third_party_file_io);
            return new GenericFile<CredCard1Record>(csv_file);
        }

        public IDataFile<CredCard1InOutRecord> Create_new_owned_file(IFileIO<CredCard1InOutRecord> owned_file_io)
        {
            var csv_file = new CSVFile<CredCard1InOutRecord>(owned_file_io);
            return new GenericFile<CredCard1InOutRecord>(csv_file);
        }

        public void Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<CredCard1InOutRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<CredCard1Record, CredCard1InOutRecord> data_loading_info)
        {
            var most_recent_cred_card_direct_debit = spreadsheet.Get_most_recent_row_containing_text<BankRecord>(
                MainSheetNames.Bank_out, 
                ReconConsts.Cred_card1_dd_description,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var statement_date = new DateTime();
            var next_date = most_recent_cred_card_direct_debit.Date.AddMonths(1);
            var input = input_output.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.Cred_card1_name,
                next_date.ToShortDateString()));
            double new_balance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out new_balance))
                {
                    new_balance = new_balance * -1;
                    pending_file.Records.Add(new CredCard1InOutRecord
                    {
                        Date = next_date,
                        Description = ReconConsts.Cred_card1_regular_pymt_description,
                        Unreconciled_amount = new_balance
                    });
                }
                statement_date = next_date.AddMonths(-1);
                next_date = next_date.Date.AddMonths(1);
                input = input_output.Get_input(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.Cred_card1_name,
                    next_date.ToShortDateString()));
            }

            spreadsheet.Update_balance_on_totals_sheet(
                Codes.Cred_card1_bal,
                new_balance,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.Cred_card1_name,
                    $"{statement_date.ToString("MMM")} {statement_date.Year}"),
                balance_column: 5,
                text_column: 6,
                code_column: 4,
                input_output: input_output);
        }
    }
}