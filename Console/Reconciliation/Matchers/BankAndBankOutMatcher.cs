using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class BankAndBankOutMatcher : IMatcher
    {
        private readonly IInputOutput _inputOutput;
        private readonly ISpreadsheetRepoFactory _spreadsheetFactory;

        public BankAndBankOutMatcher(IInputOutput inputOutput, ISpreadsheetRepoFactory spreadsheetFactory)
        {
            _inputOutput = inputOutput;
            _spreadsheetFactory = spreadsheetFactory;
        }

        public void DoMatching(FilePaths mainFilePaths)
        {
            var loadingInfo = new BankAndBankOutLoader().LoadingInfo();
            loadingInfo.FilePaths = mainFilePaths;
            var reconciliationIntro = new ReconciliationIntro(_inputOutput);
            ReconciliationInterface<ActualBankRecord, BankRecord> reconciliationInterface
                = reconciliationIntro.LoadCorrectFiles<ActualBankRecord, BankRecord>(loadingInfo, _spreadsheetFactory, this);
            reconciliationInterface?.DoTheMatching();
        }

        public void DoPreliminaryStuff<TThirdPartyType, TOwnedType>()
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
        }
    }
}