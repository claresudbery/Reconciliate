using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class CredCard1AndCredCard1InOutMatcher : IMatcher
    {
        private readonly IInputOutput _inputOutput;
        private readonly ISpreadsheetRepoFactory _spreadsheetFactory;

        public CredCard1AndCredCard1InOutMatcher(IInputOutput inputOutput, ISpreadsheetRepoFactory spreadsheetFactory)
        {
            _inputOutput = inputOutput;
            _spreadsheetFactory = spreadsheetFactory;
        }

        public void DoMatching(FilePaths mainFilePaths)
        {
            var loadingInfo = new CredCard1AndCredCard1InOutLoader().LoadingInfo();
            loadingInfo.FilePaths = mainFilePaths;
            var fileLoader = new FileLoader(_inputOutput);
            ReconciliationInterface<CredCard1Record, CredCard1InOutRecord> reconciliationInterface
                = fileLoader.LoadCorrectFiles<CredCard1Record, CredCard1InOutRecord>(loadingInfo, _spreadsheetFactory, this);
            reconciliationInterface?.DoTheMatching();
        }

        public void DoPreliminaryStuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliationInterface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
        }

        public void Finish()
        {
        }
    }
}