namespace Interfaces.DTOs
{
    public class DataLoadingInformation<TThirdPartyType, TOwnedType>
        where TThirdPartyType : ICSVRecord, new()
        where TOwnedType : ICSVRecord, new()
    {
        public FilePaths FilePaths { get; set; }
        public char DefaultSeparator { get; set; }
        public char LoadingSeparator { get; set; }
        public string PendingFileName { get; set; }
        public string SheetName { get; set; }
        public string ThirdPartyDescriptor { get; set; }
        public string OwnedFileDescriptor { get; set; }
        public BudgetItemListData MonthlyBudgetData { get; set; }
        public BudgetItemListData AnnualBudgetData { get; set; }
        public ILoader<TThirdPartyType, TOwnedType> Loader { get; set; }
    }
}