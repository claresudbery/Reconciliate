namespace Interfaces
{
    public interface IBankAndBankInLoader
    {
        void Update_expected_income_record_when_matched(ICSVRecord source_record, ICSVRecord actual_record);
        void Finish();
    }
}