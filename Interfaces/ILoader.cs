﻿using Interfaces.DTOs;

namespace Interfaces
{
    public interface ILoader
    {
        void Do_matching(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory);

        void Merge_bespoke_data_with_pending_file<TOwnedType>(
                IInputOutput input_output,
                ISpreadsheet spreadsheet,
                ICSVFile<TOwnedType> pending_file,
                BudgetingMonths budgeting_months,
                DataLoadingInformation loading_info)
            where TOwnedType : ICSVRecord, new();
    }
}