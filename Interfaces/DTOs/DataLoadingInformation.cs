namespace Interfaces.DTOs
{
    public class DataLoadingInformation<TThirdPartyType, TOwnedType>
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
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
        public ILoader<TThirdPartyType, TOwnedType> Loader { get; set; }
    }
}