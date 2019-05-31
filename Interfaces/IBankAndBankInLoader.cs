using Interfaces.DTOs;

namespace Interfaces
{
    public interface IBankAndBankInLoader
    {
        void UpdateExpectedIncomeRecordWhenMatched(ICSVRecord sourceRecord, ICSVRecord actualRecord);
        void Finish();
    }
}