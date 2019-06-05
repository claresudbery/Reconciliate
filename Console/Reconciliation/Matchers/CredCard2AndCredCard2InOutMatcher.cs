using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class CredCard2AndCredCard2InOutMatcher : IMatcher
    {
        private readonly IInputOutput _inputOutput;
        private readonly ISpreadsheetRepoFactory _spreadsheetFactory;

        public CredCard2AndCredCard2InOutMatcher(IInputOutput inputOutput, ISpreadsheetRepoFactory spreadsheetFactory)
        {
            _inputOutput = inputOutput;
            _spreadsheetFactory = spreadsheetFactory;
        }

        public void DoMatching(FilePaths mainFilePaths)
        {
            var loadingInfo = new CredCard2AndCredCard2InOutLoader().LoadingInfo();
            loadingInfo.FilePaths = mainFilePaths;
            var reconciliationIntro = new ReconciliationIntro(_inputOutput);
            ReconciliationInterface<CredCard2Record, CredCard2InOutRecord> reconciliationInterface
                = reconciliationIntro.LoadCorrectFiles<CredCard2Record, CredCard2InOutRecord>(loadingInfo, _spreadsheetFactory, this);
            reconciliationInterface?.DoTheMatching();
        }

        public void DoPreliminaryStuff()
        {
        }
    }
}