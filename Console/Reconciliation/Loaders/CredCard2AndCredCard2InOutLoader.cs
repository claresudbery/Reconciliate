using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class CredCard2AndCredCard2InOutLoader : ILoader
    {
        private readonly IInputOutput _input_output;
        private readonly FileLoader _file_loader;

        public CredCard2AndCredCard2InOutLoader(IInputOutput input_output, FileLoader file_loader = null)
        {
            _input_output = input_output;
            _file_loader = file_loader;
        }

        public ReconciliationInterface Load_files_and_merge_data(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            var loading_info = CredCard2AndCredCard2InOutData.LoadingInfo;
            loading_info.File_paths = main_file_paths;
            return _file_loader.Load_files_and_merge_data<CredCard2Record, CredCard2InOutRecord>(
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
            var most_recent_cred_card_direct_debit = spreadsheet.Get_most_recent_row_containing_text<BankRecord>(
                MainSheetNames.Bank_out,
                ReconConsts.Cred_card2_dd_description,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

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
                    ((ICSVFile<CredCard2InOutRecord>)(pending_file)).Records.Add(new CredCard2InOutRecord
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

            spreadsheet.Update_balance_on_totals_sheet(
                Codes.Cred_card2_bal,
                new_balance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.Cred_card2_name,
                    $"{statement_date.ToString("MMM")} {statement_date.Year}"),
                balance_column: 5,
                text_column: 6,
                code_column: 4);
        }
    }
}