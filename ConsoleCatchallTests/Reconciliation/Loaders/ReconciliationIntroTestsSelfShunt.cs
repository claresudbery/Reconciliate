using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.DTOs;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public partial class ReconciliationIntroTests : IInputOutput
    {
        readonly List<List<string>> _outputAllLinesRecordedDescriptions = new List<List<string>>();
        readonly List<string> _outputSingleLineRecordedMessages = new List<string>();
        readonly List<ConsoleLine> _outputAllLinesRecordedConsoleLines = new List<ConsoleLine>();
        readonly List<ConsoleLine> _outputSingleLineRecordedConsoleLines = new List<ConsoleLine>();
        private List<string> _getInputMessages = new List<string>();

        public void Output_all_lines(List<IPotentialMatch> options)
        {
            _mockInputOutput.Object.Output_all_lines(options);
            _outputAllLinesRecordedDescriptions.Add(options.Select(x => x.Console_lines[0].Description_string).ToList());
        }

        public void Output_all_lines(List<ConsoleLine> consoleLines)
        {
            _mockInputOutput.Object.Output_all_lines(consoleLines);
            _outputAllLinesRecordedConsoleLines.AddRange(consoleLines);
        }

        public void Output_options(List<string> options)
        {
            _mockInputOutput.Object.Output_options(options);
        }

        public void Output_all_lines_except_the_first(List<IPotentialMatch> options)
        {
            _mockInputOutput.Object.Output_all_lines_except_the_first(options);
        }

        public void Output_line(List<ConsoleSnippet> consoleSnippets)
        {
            _mockInputOutput.Object.Output_line(consoleSnippets);
        }

        public void Output_line(ConsoleLine line)
        {
            _mockInputOutput.Object.Output_line(line);
            _outputSingleLineRecordedConsoleLines.Add(line);
        }

        public void Output_line_with_index(ConsoleLine line)
        {
            _mockInputOutput.Object.Output_line_with_index(line);
        }

        public void Output_line(string line)
        {
            _mockInputOutput.Object.Output_line(line);
            _outputSingleLineRecordedMessages.Add(line);
        }

        public void Output(string text)
        {
            _mockInputOutput.Object.Output(text);
            _outputSingleLineRecordedMessages.Add(text);
        }

        public void Output_string_list(List<string> stringList)
        {
            _mockInputOutput.Object.Output_string_list(stringList);
        }

        public string Get_input(string explanatoryMessage, string debugDescription = "")
        {
            _getInputMessages.Add(explanatoryMessage);
            return _mockInputOutput.Object.Get_input(explanatoryMessage, debugDescription);
        }

        public string Get_generic_input(string debugDescription)
        {
            return _mockInputOutput.Object.Get_generic_input(debugDescription);
        }

        public void Show_error(Exception exception)
        {
            _mockInputOutput.Object.Show_error(exception);
        }
    }
}
