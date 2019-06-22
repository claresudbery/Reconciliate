using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Reconciliators;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankOutLoader
    {
        private readonly IInputOutput _input_output;
        private readonly ISpreadsheetRepoFactory _spreadsheet_factory;

        public BankAndBankOutLoader(IInputOutput input_output, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            _input_output = input_output;
            _spreadsheet_factory = spreadsheet_factory;
        }

        public ReconciliationInterface
            Load(ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                FilePaths main_file_paths,
                IFileIO<BankRecord> pending_file_io,
                ICSVFile<BankRecord> pending_file,
                IFileIO<ActualBankRecord> third_party_file_io)
        {
            var data_loading_info = BankAndBankOutData.LoadingInfo;
            data_loading_info.File_paths = main_file_paths;
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);

            _input_output.Output_line(ReconConsts.LoadingDataFromPendingFile);
            pending_file.Load(true, data_loading_info.Default_separator);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            _input_output.Output_line("Converting source line separators...");
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator, data_loading_info.Loading_separator);

            _input_output.Output_line(ReconConsts.MergingSomeBudgetData);
            spreadsheet.Add_budgeted_bank_out_data_to_pending_file(
                budgeting_months,
                pending_file,
                data_loading_info.Monthly_budget_data,
                data_loading_info.Annual_budget_data);

            _input_output.Output_line("Merging bespoke data with pending file...");
            Merge_bespoke_data_with_pending_file(
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

            third_party_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Third_party_file_name);
            var owned_file_io = new FileIO<BankRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);
            var reconciliator = new BankReconciliator(third_party_file_io, owned_file_io, data_loading_info);

            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor);

            return reconciliation_interface;
        }

        public void Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation loading_info)
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