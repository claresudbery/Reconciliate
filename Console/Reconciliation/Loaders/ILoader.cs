using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal interface ILoader
    {
        ReconciliationInterface Load_files_and_merge_data(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory);

        void Merge_bespoke_data_with_pending_file<TOwnedType>(
                IInputOutput input_output,
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation loading_info)
            where TOwnedType : ICSVRecord, new();
    }
}