using System;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Utils;
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

        public void Create_pending_csvs(string path)
        {
            try
            {
                var pending_csv_file_creator = new PendingCsvFileCreator(path);
                pending_csv_file_creator.Create_and_populate_all_csvs();
            }
            catch (Exception e)
            {
                _input_output.Output_line(e.Message);
            }
        }

        public ReconciliationInterface Load_specific_files_for_reconciliation_type()
        {
            var main_file_paths = new PathSetter(_input_output).Set_path_and_file_names();
            ReconciliationInterface reconciliation_interface = null;

            switch (main_file_paths.Reconciliation_type)
            {
                case ReconciliationType.BankAndBankIn: {
                    reconciliation_interface = new BankAndBankInLoader(_input_output, this).Load_files_and_merge_data(main_file_paths);
                } break;
                case ReconciliationType.BankAndBankOut: {
                    reconciliation_interface = new BankAndBankOutLoader(_input_output, this).Load_files_and_merge_data(main_file_paths);
                } break;
                case ReconciliationType.CredCard1AndCredCard1InOut: {
                    reconciliation_interface = new CredCard1AndCredCard1InOutLoader(_input_output, this).Load_files_and_merge_data(main_file_paths);
                } break;
                case ReconciliationType.CredCard2AndCredCard2InOut: {
                    reconciliation_interface = new CredCard2AndCredCard2InOutLoader(_input_output, this).Load_files_and_merge_data(main_file_paths);
                } break;
                default: {
                    _input_output.Output_line("I don't know what files to load! Terminating now.");
                } break;
            }

            return reconciliation_interface;
        }

        public ReconciliationInterface Load_files_and_merge_data<TThirdPartyType, TOwnedType>(
                DataLoadingInformation data_loading_info, 
                ILoader loader,
                FilePaths main_file_paths)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            ReconciliationInterface reconciliation_interface = null;

            try
            {
                // NB This is the only function the spreadsheet is used in, until the very end (Reconciliator.Finish, called from
                // ReconciliationInterface), when another spreadsheet instance gets created by FileIO so it can call 
                // WriteBackToMainSpreadsheet. Between now and then, everything is done using csv files.
                var spreadsheet_repo = _spreadsheet_factory.Create_spreadsheet_repo();
                var spreadsheet = new Spreadsheet(spreadsheet_repo);
                var budgeting_month_service = new BudgetingMonthService(_input_output);
                BudgetingMonths budgeting_months = budgeting_month_service.Recursively_ask_for_budgeting_months(spreadsheet);
                _input_output.Output_line("Loading data...");

                var pending_file_io = new FileIO<TOwnedType>(_spreadsheet_factory);
                var pending_file = new CSVFile<TOwnedType>(pending_file_io);
                var third_party_file_io = new FileIO<TThirdPartyType>(_spreadsheet_factory);
                var owned_file_io = new FileIO<TOwnedType>(_spreadsheet_factory);

                reconciliation_interface = Load(
                    spreadsheet,
                    budgeting_months,
                    main_file_paths,
                    pending_file_io,
                    pending_file,
                    third_party_file_io,
                    owned_file_io,
                    data_loading_info,
                    loader);
            }
            finally
            {
                _spreadsheet_factory.Dispose_of_spreadsheet_repo();
            }

            _input_output.Output_line("");
            _input_output.Output_line("");

            return reconciliation_interface;
        }

        public ReconciliationInterface Load<TThirdPartyType, TOwnedType>(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                FilePaths main_file_paths,
                IFileIO<TOwnedType> pending_file_io,
                ICSVFile<TOwnedType> pending_file,
                IFileIO<TThirdPartyType> third_party_file_io,
                IFileIO<TOwnedType> owned_file_io,
                DataLoadingInformation data_loading_info,
                ILoader loader)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            data_loading_info.File_paths = main_file_paths;
            Load_pending_data(pending_file_io, pending_file, data_loading_info);
            Merge_budget_data(spreadsheet, budgeting_months, pending_file, data_loading_info);
            Merge_other_data(spreadsheet, budgeting_months, pending_file, data_loading_info, loader);
            Merge_unreconciled_data(spreadsheet, pending_file, data_loading_info);
            var reconciliator = Load_third_party_and_owned_files_into_reconciliator(third_party_file_io, owned_file_io, data_loading_info);
            var reconciliation_interface = Create_reconciliation_interface(reconciliator, data_loading_info);

            return reconciliation_interface;
        }

        private ReconciliationInterface Create_reconciliation_interface<TThirdPartyType, TOwnedType>(
                Reconciliator<TThirdPartyType, TOwnedType> reconciliator,
                DataLoadingInformation data_loading_info)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line(ReconConsts.CreatingReconciliationInterface);
            return new ReconciliationInterface(
                _input_output,
                reconciliator,
                data_loading_info.Third_party_descriptor,
                data_loading_info.Owned_file_descriptor);
        }

        private Reconciliator<TThirdPartyType, TOwnedType> Load_third_party_and_owned_files_into_reconciliator<TThirdPartyType, TOwnedType>(
                IFileIO<TThirdPartyType> third_party_file_io,
                IFileIO<TOwnedType> owned_file_io,
                DataLoadingInformation data_loading_info)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line(ReconConsts.LoadingDataFromFiles);
            third_party_file_io.Set_file_paths(data_loading_info.File_paths.Main_path,
                data_loading_info.File_paths.Third_party_file_name);
            owned_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.File_paths.Owned_file_name);

            var reconciliator = new Reconciliator<TThirdPartyType, TOwnedType>(
                third_party_file_io,
                owned_file_io,
                data_loading_info);
            return reconciliator;
        }

        private void Merge_unreconciled_data<TOwnedType>(
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                DataLoadingInformation data_loading_info)
            where TOwnedType : ICSVRecord, new()
        {
            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _input_output.Output_line(ReconConsts.MergingUnreconciledRows);
            spreadsheet.Add_unreconciled_rows_to_csv_file(data_loading_info.Sheet_name, pending_file);

            _input_output.Output_line(ReconConsts.CopyingMergedData);
            pending_file.Update_source_lines_for_output(data_loading_info.Loading_separator);
            pending_file.Write_to_file_as_source_lines(data_loading_info.File_paths.Owned_file_name);

            _input_output.Output_line(ReconConsts.StuffIsHappening);
        }

        private void Merge_other_data<TOwnedType>(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                ICSVFile<TOwnedType> pending_file,
                DataLoadingInformation data_loading_info,
                ILoader loader)
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line(ReconConsts.MergingBespokeData);
            loader.Merge_bespoke_data_with_pending_file(
                _input_output,
                spreadsheet,
                pending_file,
                budgeting_months,
                data_loading_info);
        }

        private void Merge_budget_data<TOwnedType>(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgeting_months,
                ICSVFile<TOwnedType> pending_file,
                DataLoadingInformation data_loading_info)
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line(ReconConsts.MergingBudgetDataWithPendingData);
            spreadsheet.Add_budgeted_monthly_data_to_pending_file(
                budgeting_months,
                pending_file,
                data_loading_info.Monthly_budget_data);
            if (null != data_loading_info.Annual_budget_data)
            {
                spreadsheet.Add_budgeted_annual_data_to_pending_file(
                    budgeting_months,
                    pending_file,
                    data_loading_info.Annual_budget_data);
            }
        }

        private void Load_pending_data<TOwnedType>(
                IFileIO<TOwnedType> pending_file_io,
                ICSVFile<TOwnedType> pending_file,
                DataLoadingInformation data_loading_info)
            where TOwnedType : ICSVRecord, new()
        {
            _input_output.Output_line(ReconConsts.LoadingDataFromPendingFile);
            pending_file_io.Set_file_paths(data_loading_info.File_paths.Main_path, data_loading_info.Pending_file_name);
            pending_file.Load(true, data_loading_info.Default_separator);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            _input_output.Output_line(ReconConsts.ConvertingSourceLineSeparators);
            pending_file.Convert_source_line_separators(data_loading_info.Default_separator,
                data_loading_info.Loading_separator);
        }
    }
}