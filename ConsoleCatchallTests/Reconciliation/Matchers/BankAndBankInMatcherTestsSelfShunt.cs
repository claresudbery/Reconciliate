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
        readonly List<List<string>> _output_all_lines_recorded_descriptions = new List<List<string>>();
        readonly List<string> _output_single_line_recorded_messages = new List<string>();
        readonly List<ConsoleLine> _output_all_lines_recorded_console_lines = new List<ConsoleLine>();
        readonly List<ConsoleLine> _output_single_line_recorded_console_lines = new List<ConsoleLine>();

        public void Output_all_lines(List<IPotentialMatch> options)
        {
            _mock_input_output.Object.Output_all_lines(options);
            _output_all_lines_recorded_descriptions.AddRange(options
                .Select(x => x.Console_lines
                    .Select(y => y.Description_string).ToList()));
        }

        public void Output_all_lines(List<ConsoleLine> consoleLines)
        {
            _mock_input_output.Object.Output_all_lines(consoleLines);
            _output_all_lines_recorded_console_lines.AddRange(consoleLines);
        }

        public void Output_options(List<string> options)
        {
            _mock_input_output.Object.Output_options(options);
        }

        public void Output_all_lines_except_the_first(List<IPotentialMatch> options)
        {
            _mock_input_output.Object.Output_all_lines_except_the_first(options);
        }

        public void Output_line(List<ConsoleSnippet> consoleSnippets)
        {
            _mock_input_output.Object.Output_line(consoleSnippets);
        }

        public void Output_line(ConsoleLine line)
        {
            _mock_input_output.Object.Output_line(line);
            _output_single_line_recorded_console_lines.Add(line);
        }

        public void Output_line_with_index(ConsoleLine line)
        {
            _mock_input_output.Object.Output_line_with_index(line);
        }

        public void Output_line(string line)
        {
            _mock_input_output.Object.Output_line(line);
            _output_single_line_recorded_messages.Add(line);
        }

        public void Output(string text)
        {
            _mock_input_output.Object.Output(text);
            _output_single_line_recorded_messages.Add(text);
        }

        public void Output_string_list(List<string> stringList)
        {
            _mock_input_output.Object.Output_string_list(stringList);
        }

        public string Get_input(string explanatoryMessage, string debugDescription = "")
        {
            return _mock_input_output.Object.Get_input(explanatoryMessage, debugDescription);
        }

        public string Get_generic_input(string debugDescription)
        {
            return _mock_input_output.Object.Get_generic_input(debugDescription);
        }

        public void Show_error(Exception exception)
        {
            _mock_input_output.Object.Show_error(exception);
        }
    }
}
