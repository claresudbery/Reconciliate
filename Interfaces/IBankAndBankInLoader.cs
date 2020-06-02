namespace Interfaces
{
    public interface IBankAndBankInLoader
    {
        void Update_expected_income_record_when_matched(ICSVRecord source_record, ICSVRecord actual_record);
        void Create_new_expenses_record_to_match_balance(ICSVRecord source_record, double balance);
        void Finish();
    }
}