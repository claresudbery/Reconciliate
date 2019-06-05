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
        private void ClearSelfShuntVariables()
        {
            _outputAllLinesRecordedDescriptions.Clear();
            _outputSingleLineRecordedMessages.Clear();
            _outputAllLinesRecordedConsoleLines.Clear();
            _outputSingleLineRecordedConsoleLines.Clear();
        }

        private void SetupForAllMatchesChosenWithIndexZero()
        {
            _mockInputOutput.Setup(x =>
                x.GetInput(ReconConsts.EnterNumberOfMatch, It.IsAny<string>()))
                .Returns("0");
            SetupToExitAtTheEnd();
        }

        private void SetupToChooseMatch(string sourceRecordDescription, int matchIndex)
        {
            _mockInputOutput.Setup(x =>
                x.GetInput(ReconConsts.EnterNumberOfMatch, sourceRecordDescription))
                .Returns($"{matchIndex}");
        }

        private void SetupToMoveOnToManualMatchingThenExit()
        {
            _mockInputOutput.SetupSequence(x =>
                    x.GetGenericInput(ReconConsts.GoAgainFinish))
                .Returns("1")
                .Returns("2");
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.ChooseWhatToDoWithMatches, "")).Returns("");
        }

        private void SetupToExitAtTheEnd()
        {
            _mockInputOutput.Setup(x => x.GetGenericInput(ReconConsts.GoAgainFinish)).Returns("2");
            _mockInputOutput.Setup(x => x.GetInput(ReconConsts.ChooseWhatToDoWithMatches, "")).Returns("");
        }

        private void SetupToRemoveAutoMatch(string index = "0")
        {
            _mockInputOutput.Setup(x => x.GetGenericInput(ReconConsts.GoAgainFinish)).Returns("2");
            _mockInputOutput.SetupSequence(
                    x => x.GetInput(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.AutoMatches))
                .Returns(index)
                .Returns("");
        }

        private void SetupToRemoveFinalMatch(string index = "0")
        {
            _mockInputOutput.Setup(x => x.GetGenericInput(ReconConsts.GoAgainFinish)).Returns("2");
            _mockInputOutput.SetupSequence(
                    x => x.GetInput(ReconConsts.ChooseWhatToDoWithMatches, ReconConsts.FinalMatches))
                .Returns(index)
                .Returns("");
        }

        private void SetupToDeleteThirdPartyRecord(string recordDescription)
        {
            _mockInputOutput.Setup(x =>
                x.GetInput(ReconConsts.EnterNumberOfMatch, recordDescription))
                .Returns("D");
            _mockInputOutput.Setup(x =>
                x.GetInput(ReconConsts.WhetherToDeleteThirdParty, recordDescription))
                .Returns("Y");
        }

        private void SetupToDeleteOwnedRecordOnceOnly(string recordDescription, int deletedecordIndex, int matchedRecordIndex)
        {
            // Choose to delete on the first time through, but not on the second.
            _mockInputOutput.SetupSequence(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, recordDescription))
                .Returns("D")
                .Returns($"{matchedRecordIndex}");

            _mockInputOutput.Setup(x =>
                    x.GetInput(ReconConsts.WhetherToDeleteThirdParty, recordDescription))
                .Returns("N");
            _mockInputOutput.Setup(x =>
                    x.GetInput(ReconConsts.EnterDeletionIndex, recordDescription))
                .Returns($"{deletedecordIndex}");
        }

        private void SetupToDeleteOwnedRecordTwiceOnly(string recordDescription, int deletedecordIndex, int matchedRecordIndex)
        {
            // Choose to delete on the first time through, but not on the second.
            _mockInputOutput.SetupSequence(x =>
                    x.GetInput(ReconConsts.EnterNumberOfMatch, recordDescription))
                .Returns("D")
                .Returns("D")
                .Returns($"{matchedRecordIndex}");

            _mockInputOutput.Setup(x =>
                    x.GetInput(ReconConsts.WhetherToDeleteThirdParty, recordDescription))
                .Returns("N");
            _mockInputOutput.Setup(x =>
                    x.GetInput(ReconConsts.EnterDeletionIndex, recordDescription))
                .Returns($"{deletedecordIndex}");
        }

        // If a record is deleted or amended during the test, you can't use the normal Verify method.
        // Have to use this instead.
        // !! Can only be used once per test !!
        private void PrepareToVerifyRecordIsOutputAmongstNonPrioritisedMatches(string recordDescription)
        {
            _numTimesCalled = 0;
            _mockInputOutput
                .Setup(x => x.OutputAllLinesExceptTheFirst(
                    It.Is<List<IPotentialMatch>>(
                        y => y.Count(z => z.ConsoleLines[0].DescriptionString == recordDescription) == 1)))
                .Callback((List<IPotentialMatch> e) => _numTimesCalled++)
                .Verifiable();
        }

        // If a record is deleted or amended during the test, you can't use the normal Verify method.
        // Have to use this instead.
        // !! Can only be used once per test !!
        private void PrepareToVerifyRecordIsOutputAmongstAllMatches(string recordDescription)
        {
            _numTimesCalled = 0;
            _mockInputOutput
                .Setup(x => x.OutputAllLines(
                    It.Is<List<IPotentialMatch>>(
                        y => y.Count(z => z.ConsoleLines[0].DescriptionString == recordDescription) == 1)))
                .Callback((List<IPotentialMatch> e) => _numTimesCalled++)
                .Verifiable();
        }

        private void VerifyIsOutputAmongstNonPrioritisedMatches(string lineDescription, int numTimes)
        {
            _mockInputOutput
                .Verify(x => x.OutputAllLinesExceptTheFirst(
                        It.Is<List<IPotentialMatch>>(
                            y => y.Count(z => z.ConsoleLines[0].DescriptionString == lineDescription) == 1)),
                    Times.Exactly(numTimes));
        }

        private void VerifyIsOutputAmongstAllMatches(string lineDescription, int numTimes)
        {
            _mockInputOutput
                .Verify(x => x.OutputAllLines(
                        It.Is<List<IPotentialMatch>>(
                            y => y.Count(z => z.ConsoleLines[0].DescriptionString == lineDescription) == 1)),
                            Times.Exactly(numTimes));
        }

        private void VerifyIsOutputAsConsoleSnippet(string lineDescription, int numTimes)
        {
            _mockInputOutput
                .Verify(x => x.OutputLine(
                        It.Is<List<ConsoleSnippet>>(
                            y => y.Count(z => z.Text == lineDescription) == 1)),
                            Times.Exactly(numTimes));
        }

        private void VerifyIsOutputAsConsoleLine(string lineDescription, int numTimes)
        {
            _mockInputOutput
                .Verify(x => x.OutputLine(
                        It.Is<ConsoleLine>(
                            y => y.DescriptionString.Contains(lineDescription))),
                            numTimes != -1
                                ? Times.Exactly(numTimes)
                                : Times.AtLeastOnce());
        }
    }

}
