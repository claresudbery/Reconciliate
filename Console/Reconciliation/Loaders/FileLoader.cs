using System;
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
    internal class FileLoader
    {
        private readonly IInputOutput _input_output;
        private readonly ISpreadsheetRepoFactory _spreadsheet_factory;

        public FileLoader(IInputOutput input_output, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            _input_output = input_output;
            _spreadsheet_factory = spreadsheet_factory;
        }

        public ReconciliationInterface Load_specific_files_for_reconciliation_type(
            FilePaths main_file_paths,
            ReconciliationType reconciliation_type)
        {
            _input_output.Output_line("Loading data...");

            ReconciliationInterface reconciliation_interface = null;

            try
            {
                // NB This is the only function the spreadsheet is used in, until the very end (Reconciliator.Finish, called from
                // ReconciliationInterface), when another spreadsheet instance gets created by FileIO so it can call 
                // WriteBackToMainSpreadsheet. Between now and then, everything is done using csv files.
                var spreadsheet_repo = _spreadsheet_factory.Create_spreadsheet_repo();
                var spreadsheet = new Spreadsheet(spreadsheet_repo);
                var file_loader = new FileLoader(_input_output, _spreadsheet_factory);
                var reconciliation_intro = new ReconciliationIntro(_input_output);
                BudgetingMonths budgeting_months = reconciliation_intro.Recursively_ask_for_budgeting_months(spreadsheet);

                switch (reconciliation_type)
                {
                    case ReconciliationType.BankAndBankIn:
                        {
                            reconciliation_interface =
                                file_loader.Load_bank_and_bank_in(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths);
                        }
                        break;
                    case ReconciliationType.BankAndBankOut:
                        {
                            reconciliation_interface =
                                file_loader.Load_bank_and_bank_out(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths);
                        }
                        break;
                    case ReconciliationType.CredCard1AndCredCard1InOut:
                        {
                            reconciliation_interface =
                                file_loader.Load_cred_card1_and_cred_card1_in_out(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths);
                        }
                        break;
                    case ReconciliationType.CredCard2AndCredCard2InOut:
                        {
                            reconciliation_interface =
                                file_loader.Load_cred_card2_and_cred_card2_in_out(
                                    spreadsheet,
                                    budgeting_months,
                                    main_file_paths);
                        }
                        break;
                    default:
                        {
                            _input_output.Output_line("I don't know what files to load! Terminating now.");
                        }
                        break;
                }
            }
            finally
            {
                _spreadsheet_factory.Dispose_of_spreadsheet_repo();
            }

            _input_output.Output_line("");
            _input_output.Output_line("");

            return reconciliation_interface;
        }

        public ReconciliationInterface
            Load_bank_and_bank_in(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                FilePaths main_file_paths)
        {
            var data_loading_info = BankAndBankInData.LoadingInfo;
            data_loading_info.File_paths = main_file_paths;

            var pending_file_io = new FileIO<BankRecord>(_spreadsheet_factory);
            var pending_file = new CSVFile<BankRecord>(pending_file_io);
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);

            _input_output.Output_line(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.Default_separator);
            _input_output.Output_line("Converting source line separators...");
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator, data_loading_info.Loading_separator);
            _input_output.Output_line(ReconConsts.MergingSomeBudgetData);
            spreadsheet.Add_budgeted_bank_in_data_to_pending_file(budgeting_months, pending_file, data_loading_info.Monthly_budget_data);
            _input_output.Output_line("Merging bespoke data with pending file...");
            Bank_and_bank_in__Merge_bespoke_data_with_pending_file(
                _input_output,
                spreadsheet,
                pending_file,
                budgeting_months,
                data_loading_info);
            _input_output.Output_line("Updating source lines for output...");
            pending_file.Update_source_lines_for_output(data_loading_info.Loading_separator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _input_output.Output_line("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.Add_unreconciled_rows_to_csv_file(data_loading_info.Sheet_name, pending_file);
            _input_output.Output_line("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.Write_to_file_as_source_lines(data_loading_info.File_paths.Owned_file_name);
            _input_output.Output_line("...");

            var third_party_file_io = new FileIO<ActualBankRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Third_party_file_name);
            var owned_file_io = new FileIO<BankRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);
            var reconciliator = new BankReconciliator(third_party_file_io, owned_file_io, data_loading_info);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor);
            return reconciliation_interface;
        }

        public ReconciliationInterface
            Load_bank_and_bank_out(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                FilePaths main_file_paths)
        {
            var data_loading_info = BankAndBankOutData.LoadingInfo;
            data_loading_info.File_paths = main_file_paths;

            var pending_file_io = new FileIO<BankRecord>(_spreadsheet_factory);
            var pending_file = new CSVFile<BankRecord>(pending_file_io);
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);

            _input_output.Output_line(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.Default_separator);
            _input_output.Output_line("Converting source line separators...");
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator, data_loading_info.Loading_separator);
            _input_output.Output_line(ReconConsts.MergingSomeBudgetData);
            spreadsheet.Add_budgeted_bank_out_data_to_pending_file(
                budgeting_months,
                pending_file,
                data_loading_info.Monthly_budget_data,
                data_loading_info.Annual_budget_data);
            _input_output.Output_line("Merging bespoke data with pending file...");
            Bank_and_bank_out__Merge_bespoke_data_with_pending_file(
                _input_output,
                spreadsheet,
                pending_file,
                budgeting_months,
                data_loading_info);
            _input_output.Output_line("Updating source lines for output...");
            pending_file.Update_source_lines_for_output(data_loading_info.Loading_separator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _input_output.Output_line("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.Add_unreconciled_rows_to_csv_file(data_loading_info.Sheet_name, pending_file);
            _input_output.Output_line("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.Write_to_file_as_source_lines(data_loading_info.File_paths.Owned_file_name);
            _input_output.Output_line("...");

            var third_party_file_io = new FileIO<ActualBankRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Third_party_file_name);
            var owned_file_io = new FileIO<BankRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);
            var reconciliator = new BankReconciliator(third_party_file_io, owned_file_io, data_loading_info);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor);
            return reconciliation_interface;
        }

        public ReconciliationInterface
            Load_cred_card1_and_cred_card1_in_out(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                FilePaths main_file_paths)
        {
            var data_loading_info = CredCard1AndCredCard1InOutData.LoadingInfo;
            data_loading_info.File_paths = main_file_paths;

            var pending_file_io = new FileIO<CredCard1InOutRecord>(_spreadsheet_factory);
            var pending_file = new CSVFile<CredCard1InOutRecord>(pending_file_io);
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);

            _input_output.Output_line(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.Default_separator);
            _input_output.Output_line("Converting source line separators...");
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator, data_loading_info.Loading_separator);
            _input_output.Output_line(ReconConsts.MergingSomeBudgetData);
            spreadsheet.Add_budgeted_cred_card1_in_out_data_to_pending_file(budgeting_months, pending_file, data_loading_info.Monthly_budget_data);
            _input_output.Output_line("Merging bespoke data with pending file...");
            Cred_card1_and_cred_card1_in_out__Merge_bespoke_data_with_pending_file(
                _input_output,
                spreadsheet,
                pending_file,
                budgeting_months,
                data_loading_info);
            _input_output.Output_line("Updating source lines for output...");
            pending_file.Update_source_lines_for_output(data_loading_info.Loading_separator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _input_output.Output_line("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.Add_unreconciled_rows_to_csv_file(data_loading_info.Sheet_name, pending_file);
            _input_output.Output_line("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.Write_to_file_as_source_lines(data_loading_info.File_paths.Owned_file_name);
            _input_output.Output_line("...");

            var third_party_file_io = new FileIO<CredCard1Record>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Third_party_file_name);
            var owned_file_io = new FileIO<CredCard1InOutRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);
            var reconciliator = new CredCard1Reconciliator(third_party_file_io, owned_file_io);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor);
            return reconciliation_interface;
        }

        public ReconciliationInterface
            Load_cred_card2_and_cred_card2_in_out(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                FilePaths main_file_paths)
        {
            var data_loading_info = CredCard2AndCredCard2InOutData.LoadingInfo;
            data_loading_info.File_paths = main_file_paths;

            var pending_file_io = new FileIO<CredCard2InOutRecord>(_spreadsheet_factory);
            var pending_file = new CSVFile<CredCard2InOutRecord>(pending_file_io);
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);

            _input_output.Output_line(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.Default_separator);
            _input_output.Output_line("Converting source line separators...");
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator, data_loading_info.Loading_separator);
            _input_output.Output_line(ReconConsts.MergingSomeBudgetData);
            spreadsheet.Add_budgeted_cred_card2_in_out_data_to_pending_file(budgeting_months, pending_file, data_loading_info.Monthly_budget_data);
            _input_output.Output_line("Merging bespoke data with pending file...");
            Cred_card2_and_cred_card2_in_out__Merge_bespoke_data_with_pending_file(
                _input_output, spreadsheet, pending_file, budgeting_months, data_loading_info);
            _input_output.Output_line("Updating source lines for output...");
            pending_file.Update_source_lines_for_output(data_loading_info.Loading_separator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _input_output.Output_line("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.Add_unreconciled_rows_to_csv_file(data_loading_info.Sheet_name, pending_file);
            _input_output.Output_line("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.Write_to_file_as_source_lines(data_loading_info.File_paths.Owned_file_name);
            _input_output.Output_line("...");

            var third_party_file_io = new FileIO<CredCard2Record>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Third_party_file_name);
            var owned_file_io = new FileIO<CredCard2InOutRecord>(_spreadsheet_factory, data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);
            var reconciliator = new CredCard2Reconciliator(third_party_file_io, owned_file_io);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor);
            return reconciliation_interface;
        }

        public void Bank_and_bank_in__Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation loading_info)
        {
            input_output.Output_line(ReconConsts.Loading_expenses);
            var expected_income_file_io = new FileIO<ExpectedIncomeRecord>(new FakeSpreadsheetRepoFactory());
            var expected_income_csv_file = new CSVFile<ExpectedIncomeRecord>(expected_income_file_io);
            expected_income_csv_file.Load(false);
            var expected_income_file = new ExpectedIncomeFile(expected_income_csv_file);
            spreadsheet.Add_unreconciled_rows_to_csv_file<ExpectedIncomeRecord>(MainSheetNames.Expected_in, expected_income_file.File);
            expected_income_csv_file.Populate_source_records_from_records();
            expected_income_file.Filter_for_employer_expenses_only();
            expected_income_file.Copy_to_pending_file(pending_file);
            expected_income_csv_file.Populate_records_from_original_file_load();
        }

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

        private void Bank_and_bank_out__Add_most_recent_credit_card_direct_debits(
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

        public void Cred_card1_and_cred_card1_in_out__Merge_bespoke_data_with_pending_file(
            IInputOutput input_output, 
            ISpreadsheet spreadsheet, 
            ICSVFile<CredCard1InOutRecord> pending_file, 
            BudgetingMonths budgeting_months, 
            DataLoadingInformation data_loading_info)
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
                new_balance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.Cred_card1_name,
                    $"{statement_date.ToString("MMM")} {statement_date.Year}"),
                balance_column: 5,
                text_column: 6,
                code_column: 4);
        }

        public void Cred_card2_and_cred_card2_in_out__Merge_bespoke_data_with_pending_file(
                IInputOutput input_output,
                ISpreadsheet spreadsheet,
                ICSVFile<CredCard2InOutRecord> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation data_loading_info)
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