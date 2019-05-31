using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.DTOs;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Matchers
{
    [TestFixture]
    public partial class BankAndBankInMatcherTests : IInputOutput
    {
        readonly List<List<string>> _outputAllLinesRecordedDescriptions = new List<List<string>>();
        readonly List<string> _outputSingleLineRecordedMessages = new List<string>();
        readonly List<ConsoleLine> _outputAllLinesRecordedConsoleLines = new List<ConsoleLine>();
        readonly List<ConsoleLine> _outputSingleLineRecordedConsoleLines = new List<ConsoleLine>();

        public void OutputAllLines(List<IPotentialMatch> options)
        {
            _mockInputOutput.Object.OutputAllLines(options);
            _outputAllLinesRecordedDescriptions.AddRange(options
                .Select(x => x.ConsoleLines
                    .Select(y => y.DescriptionString).ToList()));
        }

        public void OutputAllLines(List<ConsoleLine> consoleLines)
        {
            _mockInputOutput.Object.OutputAllLines(consoleLines);
            _outputAllLinesRecordedConsoleLines.AddRange(consoleLines);
        }

        public void OutputOptions(List<string> options)
        {
            _mockInputOutput.Object.OutputOptions(options);
        }

        public void OutputAllLinesExceptTheFirst(List<IPotentialMatch> options)
        {
            _mockInputOutput.Object.OutputAllLinesExceptTheFirst(options);
        }

        public void OutputLine(List<ConsoleSnippet> consoleSnippets)
        {
            _mockInputOutput.Object.OutputLine(consoleSnippets);
        }

        public void OutputLine(ConsoleLine line)
        {
            _mockInputOutput.Object.OutputLine(line);
            _outputSingleLineRecordedConsoleLines.Add(line);
        }

        public void OutputLineWithIndex(ConsoleLine line)
        {
            _mockInputOutput.Object.OutputLineWithIndex(line);
        }

        public void OutputLine(string line)
        {
            _mockInputOutput.Object.OutputLine(line);
            _outputSingleLineRecordedMessages.Add(line);
        }

        public void Output(string text)
        {
            _mockInputOutput.Object.Output(text);
            _outputSingleLineRecordedMessages.Add(text);
        }

        public void OutputStringList(List<string> stringList)
        {
            _mockInputOutput.Object.OutputStringList(stringList);
        }

        public string GetInput(string explanatoryMessage, string debugDescription = "")
        {
            return _mockInputOutput.Object.GetInput(explanatoryMessage, debugDescription);
        }

        public string GetGenericInput(string debugDescription)
        {
            return _mockInputOutput.Object.GetGenericInput(debugDescription);
        }

        public void ShowError(Exception exception)
        {
            _mockInputOutput.Object.ShowError(exception);
        }
    }
}
