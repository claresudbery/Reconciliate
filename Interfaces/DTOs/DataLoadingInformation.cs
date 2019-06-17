using Interfaces.Constants;

namespace Interfaces.DTOs
{
    public class DataLoadingInformation
    {
        public FilePaths File_paths { get; set; }
        public char Default_separator { get; set; }
        public char Loading_separator { get; set; }
        public string Pending_file_name { get; set; }
        public string Sheet_name { get; set; }
        public string Third_party_descriptor { get; set; }
        public string Owned_file_descriptor { get; set; }
        public BudgetItemListData Monthly_budget_data { get; set; }
        public BudgetItemListData Annual_budget_data { get; set; }
        public ThirdPartyFileLoadAction Third_party_file_load_action { get; set; }
    }
}