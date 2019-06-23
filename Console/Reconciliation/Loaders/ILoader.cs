using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal interface ILoader
    {
        void Merge_bespoke_data_with_pending_file<TOwnedType>(
                IInputOutput input_output,
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation loading_info)
            where TOwnedType : ICSVRecord, new();
    }
}