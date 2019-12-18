using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class FileLoader
    {
        public void Bank_and_bank_out__Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation data_loading_info)
        {
            Bank_and_bank_out__Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                pending_file,
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description);

            Bank_and_bank_out__Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                (ICSVFile<BankRecord>)pending_file,
                ReconConsts.Cred_card2_name,
                ReconConsts.Cred_card2_dd_description);
        }

        public void Bank_and_bank_out__Add_most_recent_credit_card_direct_debits(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            string cred_card_name,
            string direct_debit_description)
        {
            var most_recent_cred_card_direct_debit = spreadsheet.Get_most_recent_row_containing_text<BankRecord>(
                MainSheetNames.Bank_out,
                direct_debit_description,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

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