using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankOutLoader : ILoader
    {
        private readonly IInputOutput _input_output;
        private readonly FileLoader _file_loader;

        public BankAndBankOutLoader(IInputOutput input_output, FileLoader file_loader = null)
        {
            _input_output = input_output;
            _file_loader = file_loader;
        }

        public ReconciliationInterface Load_files_and_merge_data(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory = null)
        {
            var loading_info = BankAndBankOutData.LoadingInfo;
            loading_info.File_paths = main_file_paths;
            return _file_loader.Load_files_and_merge_data<ActualBankRecord, BankRecord>(
                loading_info, this);
        }

        public void Merge_bespoke_data_with_pending_file<TOwnedType>(
                IInputOutput input_output,
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation loading_info)
            where TOwnedType : ICSVRecord, new()
        {
            Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                (ICSVFile<BankRecord>)pending_file,
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description);

            Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                (ICSVFile<BankRecord>)pending_file,
                ReconConsts.Cred_card2_name,
                ReconConsts.Cred_card2_dd_description);
        }

        private static void Add_most_recent_credit_card_direct_debits(
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