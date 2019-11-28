using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class FileLoader
    {
        private readonly IInputOutput _input_output;

        public FileLoader(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public ReconciliationInterface<TThirdPartyType, TOwnedType>
            Load_files_and_merge_data<TThirdPartyType, TOwnedType>(
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info,
                ISpreadsheetRepoFactory spreadsheet_factory,
                IMatcher matcher)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            ReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface = null;

            try
            {
                // NB This is the only function the spreadsheet is used in, until the very end (Reconciliator.Finish, called from
                // ReconciliationInterface), when another spreadsheet instance gets created by FileIO so it can call 
                // WriteBackToMainSpreadsheet. Between now and then, everything is done using csv files.
                var spreadsheet_repo = spreadsheet_factory.Create_spreadsheet_repo();
                var spreadsheet = new Spreadsheet(spreadsheet_repo);
                BudgetingMonths budgeting_months = Recursively_ask_for_budgeting_months(spreadsheet);
                _input_output.Output_line("Loading data...");

                var pending_file_io = new FileIO<TOwnedType>(spreadsheet_factory);
                var third_party_file_io = new FileIO<TThirdPartyType>(spreadsheet_factory);
                var owned_file_io = new FileIO<TOwnedType>(spreadsheet_factory);
                var pending_file = new CSVFile<TOwnedType>(pending_file_io);

                reconciliation_interface = Load<TThirdPartyType, TOwnedType>(
                        spreadsheet, 
                        pending_file_io, 
                        pending_file, 
                        third_party_file_io, 
                        owned_file_io, 
                        budgeting_months, 
                        data_loading_info, 
                        matcher);
            }
            finally
            {
                spreadsheet_factory.Dispose_of_spreadsheet_repo();
            }

            _input_output.Output_line("");
            _input_output.Output_line("");

            return reconciliation_interface;
        }

        // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
        // We load it up into memory.
        // Then some budget amounts are added to that file (in memory).
        // Other budget amounts (like CredCard1 balance) have been written directly to the spreadsheet before this.
        // Then we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
        // Then we write all that data away into the 'owned' csv file (eg BankOut.csv). Then we read it back in again!
        // Also we load up the third party data, and pass it all on to the reconciliation interface.
        public ReconciliationInterface<TThirdPartyType, TOwnedType>
            Load<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                IFileIO<TOwnedType> pending_file_io,
                ICSVFile<TOwnedType> pending_file,
                IFileIO<TThirdPartyType> third_party_file_io,
                IFileIO<TOwnedType> owned_file_io,
                BudgetingMonths budgeting_months,
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info,
                IMatcher matcher)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            Load_pending_data(pending_file_io, pending_file, data_loading_info);
            Merge_budget_data(spreadsheet, pending_file, budgeting_months, data_loading_info);
            Merge_other_data(spreadsheet, pending_file, budgeting_months, data_loading_info);
            Merge_unreconciled_data(spreadsheet, pending_file, data_loading_info);
            var reconciliator = Load_third_party_and_owned_files_into_reconciliator<TThirdPartyType, TOwnedType>(data_loading_info, third_party_file_io, owned_file_io);
            var reconciliation_interface = Create_reconciliation_interface(data_loading_info, reconciliator, matcher);

            return reconciliation_interface;
        }

        public void Load_pending_data<TThirdPartyType, TOwnedType>(
                IFileIO<TOwnedType> pending_file_io,
                ICSVFile<TOwnedType> pending_file,
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line(
                "Loading data from pending file (which you should have already split out, if necessary)...");
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);
            pending_file.Load(true, data_loading_info.Default_separator);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator, data_loading_info.Loading_separator);
        }

        private void Merge_budget_data<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info)
                where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line("Merging budget data with pending data...");
            spreadsheet.Add_budgeted_monthly_data_to_pending_file(budgeting_months, pending_file, data_loading_info.Monthly_budget_data);
            if (null != data_loading_info.Annual_budget_data)
            {
                spreadsheet.Add_budgeted_annual_data_to_pending_file(budgeting_months, pending_file, data_loading_info.Annual_budget_data);
            }
        }

        private void Merge_other_data<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            data_loading_info.Loader.Merge_bespoke_data_with_pending_file(_input_output, spreadsheet, pending_file, budgeting_months, data_loading_info);
        }

        private void Merge_unreconciled_data<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.Add_unreconciled_rows_to_csv_file<TOwnedType>(data_loading_info.Sheet_name, pending_file);

            _input_output.Output_line("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.Update_source_lines_for_output(data_loading_info.Loading_separator);
            pending_file.Write_to_file_as_source_lines(data_loading_info.File_paths.Owned_file_name);

            _input_output.Output_line("...");
        }

        private Reconciliator<TThirdPartyType, TOwnedType>
            Load_third_party_and_owned_files_into_reconciliator<TThirdPartyType, TOwnedType>(
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info,
                IFileIO<TThirdPartyType> third_party_file_io,
                IFileIO<TOwnedType> owned_file_io)
            where TThirdPartyType : ICSVRecord, new() where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line("Loading data back in from 'owned' and 'third party' files...");
            third_party_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Third_party_file_name);
            owned_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);
            var third_party_file = data_loading_info.Loader.Create_new_third_party_file(third_party_file_io);
            var owned_file = data_loading_info.Loader.Create_new_owned_file(owned_file_io);

            var reconciliator = new Reconciliator<TThirdPartyType, TOwnedType>(
                data_loading_info,
                third_party_file,
                owned_file);

            return reconciliator;
        }

        private ReconciliationInterface<TThirdPartyType, TOwnedType>
            Create_reconciliation_interface<TThirdPartyType, TOwnedType>(
                DataLoadingInformation<TThirdPartyType, TOwnedType> data_loading_info,
                Reconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IMatcher matcher)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line("Creating reconciliation interface...");
            var reconciliation_interface = new ReconciliationInterface<TThirdPartyType, TOwnedType>(
                new InputOutput(),
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor,
                matcher);
            return reconciliation_interface;
        }

        public BudgetingMonths Recursively_ask_for_budgeting_months(ISpreadsheet spreadsheet)
        {
            DateTime next_unplanned_month = Get_next_unplanned_month(spreadsheet);
            int last_month_for_budget_planning = Get_last_month_for_budget_planning(spreadsheet, next_unplanned_month.Month);
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = next_unplanned_month.Month,
                Last_month_for_budget_planning = last_month_for_budget_planning,
                Start_year = next_unplanned_month.Year
            };
            if (last_month_for_budget_planning != 0)
            {
                budgeting_months.Last_month_for_budget_planning = Confirm_budgeting_month_choices_with_user(budgeting_months, spreadsheet);
            }
            return budgeting_months;
        }

        private DateTime Get_next_unplanned_month(ISpreadsheet spreadsheet)
        {
            DateTime default_month = DateTime.Today;
            DateTime next_unplanned_month = default_month;
            bool bad_input = false;
            try
            {
                next_unplanned_month = spreadsheet.Get_next_unplanned_month();
            }
            catch (Exception)
            {
                string new_month = _input_output.Get_input(ReconConsts.CantFindMortgageRow);
                try
                {
                    if (!String.IsNullOrEmpty(new_month) && Char.IsDigit(new_month[0]))
                    {
                        int actual_month = Convert.ToInt32(new_month);
                        if (actual_month < 1 || actual_month > 12)
                        {
                            bad_input = true;
                        }
                        else
                        {
                            var year = default_month.Year;
                            if (actual_month < default_month.Month)
                            {
                                year++;
                            }
                            next_unplanned_month = new DateTime(year, actual_month, 1);
                        }
                    }
                    else
                    {
                        bad_input = true;
                    }
                }
                catch (Exception)
                {
                    bad_input = true;
                }
            }

            if (bad_input)
            {
                _input_output.Output_line(ReconConsts.DefaultUnplannedMonth);
                next_unplanned_month = default_month;
            }

            return next_unplanned_month;
        }

        private int Get_last_month_for_budget_planning(ISpreadsheet spreadsheet, int next_unplanned_month)
        {
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(next_unplanned_month);
            var request_to_enter_month = String.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            string month = _input_output.Get_input(request_to_enter_month);
            int result = 0;

            try
            {
                if (!String.IsNullOrEmpty(month) && Char.IsDigit(month[0]))
                {
                    result = Convert.ToInt32(month);
                    if (result < 1 || result > 12)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore it and return zero by default.
            }

            result = Handle_zero_month_choice_result(result, spreadsheet, next_unplanned_month);
            return result;
        }

        private int Confirm_budgeting_month_choices_with_user(BudgetingMonths budgeting_months, ISpreadsheet spreadsheet)
        {
            var new_result = budgeting_months.Last_month_for_budget_planning;
            string input = Get_response_to_budgeting_months_confirmation_message(budgeting_months);

            if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
            {
                // I know this doesn't really do anything but I found the if statement easier to parse this way round.
                new_result = budgeting_months.Last_month_for_budget_planning;
            }
            else
            {
                // Recursion ftw!
                new_result = Get_last_month_for_budget_planning(spreadsheet, budgeting_months.Next_unplanned_month);
            }

            return new_result;
        }

        private string Get_response_to_budgeting_months_confirmation_message(BudgetingMonths budgeting_months)
        {
            string first_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgeting_months.Next_unplanned_month);
            string second_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgeting_months.Last_month_for_budget_planning);

            int month_span = budgeting_months.Num_budgeting_months();

            var confirmation_text = String.Format(ReconConsts.ConfirmMonthInterval, first_month, second_month, month_span);

            return _input_output.Get_input(confirmation_text);
        }

        private int Handle_zero_month_choice_result(int chosen_month, ISpreadsheet spreadsheet, int next_unplanned_month)
        {
            var new_result = chosen_month;
            if (chosen_month == 0)
            {
                var input = _input_output.Get_input(ReconConsts.ConfirmBadMonth);

                if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    new_result = 0;
                    _input_output.Output_line(ReconConsts.ConfirmNoMonthlyBudgeting);
                }
                else
                {
                    // Recursion ftw!
                    new_result = Get_last_month_for_budget_planning(spreadsheet, next_unplanned_month);
                }
            }
            return new_result;
        }
    }
}