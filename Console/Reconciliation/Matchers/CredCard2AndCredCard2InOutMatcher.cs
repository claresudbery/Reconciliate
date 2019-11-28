using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Matchers
{
    internal class CredCard2AndCredCard2InOutMatcher : IMatcher
    {
        private readonly IInputOutput _input_output;

        public CredCard2AndCredCard2InOutMatcher(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public void Do_matching(FilePaths main_file_paths, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            var loading_info = new CredCard2AndCredCard2InOutLoader().Loading_info();
            loading_info.File_paths = main_file_paths;
            var file_loader = new FileLoader(_input_output);
            ReconciliationInterface<CredCard2Record, CredCard2InOutRecord> reconciliation_interface
                = file_loader.Load_files_and_merge_data<CredCard2Record, CredCard2InOutRecord>(loading_info, spreadsheet_factory, this);
            reconciliation_interface?.Do_the_matching();
        }

        public void Do_preliminary_stuff<TThirdPartyType, TOwnedType>(
                IReconciliator<TThirdPartyType, TOwnedType> reconciliator,
                IReconciliationInterface<TThirdPartyType, TOwnedType> reconciliation_interface)
            where TThirdPartyType : ICSVRecord, new()
            where TOwnedType : ICSVRecord, new()
        {
        }

        public void Finish()
        {
        }
    }
}