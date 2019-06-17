using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ReconciliationBaseTests.ReconciliationBase
{
    [TestFixture]
    internal partial class ReconciliationInterfaceTests : IInputOutput
    {
        private void Clear_self_shunt_variables()
        {
            _output_all_lines_recorded_descriptions.Clear();
            _output_single_line_recorded_messages.Clear();
            _output_all_lines_recorded_console_lines.Clear();
            _output_single_line_recorded_console_lines.Clear();
        }

        private void Setup_for_all_matches_chosen_with_index_zero()
        {
            _mock_input_output.Setup(x =>
                x.Get_input(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                .Returns("0");
            Setup_to_exit_at_the_end();
        }

        private void Setup_to_choose_match(string sourceRecordDescription, int matchIndex)
        {
            _mock_input_output.Setup(x =>
                x.Get_input(ReconConsts.EnterNumberOfMatch, sourceRecordDescription))
                .Returns($"{matchIndex}");
        }

        private void Setup_to_move_on_to_manual_matching_then_exit()
        {
            _mock_input_output.SetupSequence(x =>
                    x.Get_generic_input(ReconConsts.GoAgainFinish))
                .Returns("1")
                .Returns("2");
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.ChooseWhatToDoWithMatches, "")).Returns("");
        }

        private void Setup_to_exit_at_the_end()
        {
            _mock_input_output.Setup(x => x.Get_generic_input(ReconConsts.GoAgainFinish)).Returns("2");
            _mock_input_output.Setup(x => x.Get_input(ReconConsts.ChooseWhatToDoWithMatches, "")).Returns("");
        }

        private void Setup_to_remove_auto_match(string index = "0")
        {
            _mock_input_output.Setup(x => x.Get_generic_input(ReconConsts.GoAgainFinish)).Returns("2");
            _mock_input_output.SetupSequence(
                    x => x.Get_input(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.AutoMatches))
                .Returns(index)
                .Returns("");
        }

        private void Setup_to_remove_final_match(string index = "0")
        {
            _mock_input_output.Setup(x => x.Get_generic_input(ReconConsts.GoAgainFinish)).Returns("2");
            _mock_input_output.SetupSequence(
                    x => x.Get_input(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.FinalMatches))
                .Returns(index)
                .Returns("");
        }

        private void Setup_to_delete_third_party_record(string recordDescription)
        {
            _mock_input_output.Setup(x =>
                x.Get_input(ReconConsts.EnterNumberOfMatch, recordDescription))
                .Returns("D");
            _mock_input_output.Setup(x =>
                x.Get_input(ReconConsts.WhetherToDeleteThirdParty, recordDescription))
                .Returns("Y");
        }

        private void Setup_to_delete_owned_record_once_only(string recordDescription, int deletedecordIndex, int matchedRecordIndex)
        {
            // Choose to delete on the first time through, but not on the second.
            _mock_input_output.SetupSequence(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, recordDescription))
                .Returns("D")
                .Returns($"{matchedRecordIndex}");

            _mock_input_output.Setup(x =>
                    x.Get_input(ReconConsts.WhetherToDeleteThirdParty, recordDescription))
                .Returns("N");
            _mock_input_output.Setup(x =>
                    x.Get_input(ReconConsts.EnterDeletionIndex, recordDescription))
                .Returns($"{deletedecordIndex}");
        }

        private void Setup_to_delete_owned_record_twice_only(string recordDescription, int deletedecordIndex, int matchedRecordIndex)
        {
            // Choose to delete on the first time through, but not on the second.
            _mock_input_output.SetupSequence(x =>
                    x.Get_input(ReconConsts.EnterNumberOfMatch, recordDescription))
                .Returns("D")
                .Returns("D")
                .Returns($"{matchedRecordIndex}");

            _mock_input_output.Setup(x =>
                    x.Get_input(ReconConsts.WhetherToDeleteThirdParty, recordDescription))
                .Returns("N");
            _mock_input_output.Setup(x =>
                    x.Get_input(ReconConsts.EnterDeletionIndex, recordDescription))
                .Returns($"{deletedecordIndex}");
        }

        // If a record is deleted or amended during the test, you can't use the normal Verify method.
        // Have to use this instead.
        // !! Can only be used once per test !!
        private void Prepare_to_verify_record_is_output_amongst_non_prioritised_matches(string recordDescription)
        {
            _num_times_called = 0;
            _mock_input_output
                .Setup(x => x.Output_all_lines_except_the_first(
                    It.Is<List<IPotentialMatch>>(
                        y => y.Count(z => z.Console_lines[0].Description_string == recordDescription) == 1)))
                .Callback((List<IPotentialMatch> e) => _num_times_called++)
                .Verifiable();
        }

        // If a record is deleted or amended during the test, you can't use the normal Verify method.
        // Have to use this instead.
        // !! Can only be used once per test !!
        private void Prepare_to_verify_record_is_output_amongst_all_matches(string recordDescription)
        {
            _num_times_called = 0;
            _mock_input_output
                .Setup(x => x.Output_all_lines(
                    It.Is<List<IPotentialMatch>>(
                        y => y.Count(z => z.Console_lines[0].Description_string == recordDescription) == 1)))
                .Callback((List<IPotentialMatch> e) => _num_times_called++)
                .Verifiable();
        }

        private void Verify_is_output_amongst_non_prioritised_matches(string lineDescription, int numTimes)
        {
            _mock_input_output
                .Verify(x => x.Output_all_lines_except_the_first(
                        It.Is<List<IPotentialMatch>>(
                            y => y.Count(z => z.Console_lines[0].Description_string == lineDescription) == 1)),
                    Times.Exactly(numTimes));
        }

        private void Verify_is_output_amongst_all_matches(string lineDescription, int numTimes)
        {
            _mock_input_output
                .Verify(x => x.Output_all_lines(
                        It.Is<List<IPotentialMatch>>(
                            y => y.Count(z => z.Console_lines[0].Description_string == lineDescription) == 1)),
                            Times.Exactly(numTimes));
        }

        private void Verify_is_output_as_console_snippet(string lineDescription, int numTimes)
        {
            _mock_input_output
                .Verify(x => x.Output_line(
                        It.Is<List<ConsoleSnippet>>(
                            y => y.Count(z => z.Text == lineDescription) == 1)),
                            Times.Exactly(numTimes));
        }

        private void Verify_is_output_as_console_line(string lineDescription, int numTimes)
        {
            _mock_input_output
                .Verify(x => x.Output_line(
                        It.Is<ConsoleLine>(
                            y => y.Description_string.Contains(lineDescription))),
                            numTimes != -1
                                ? Times.Exactly(numTimes)
                                : Times.AtLeastOnce());
        }
    }

}
