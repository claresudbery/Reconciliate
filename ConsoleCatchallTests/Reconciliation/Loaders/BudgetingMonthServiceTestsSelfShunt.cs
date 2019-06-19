using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.DTOs;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public partial class BudgetingMonthServiceTests : IInputOutput
    {
        readonly List<List<string>> _output_all_lines_recorded_descriptions = new List<List<string>>();
        readonly List<string> _output_single_line_recorded_messages = new List<string>();
        readonly List<ConsoleLine> _output_all_lines_recorded_console_lines = new List<ConsoleLine>();
        readonly List<ConsoleLine> _output_single_line_recorded_console_lines = new List<ConsoleLine>();
        private List<string> _get_input_messages = new List<string>();

        public void Output_all_lines(List<IPotentialMatch> options)
        {
            _mock_input_output.Object.Output_all_lines(options);
            _output_all_lines_recorded_descriptions.Add(options.Select(x => x.Console_lines[0].Description_string).ToList());
        }

        public void Output_all_lines(List<ConsoleLine> console_lines)
        {
            _mock_input_output.Object.Output_all_lines(console_lines);
            _output_all_lines_recorded_console_lines.AddRange(console_lines);
        }

        public void Output_options(List<string> options)
        {
            _mock_input_output.Object.Output_options(options);
        }

        public void Output_all_lines_except_the_first(List<IPotentialMatch> options)
        {
            _mock_input_output.Object.Output_all_lines_except_the_first(options);
        }

        public void Output_line(List<ConsoleSnippet> console_snippets)
        {
            _mock_input_output.Object.Output_line(console_snippets);
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

        public void Output_string_list(List<string> string_list)
        {
            _mock_input_output.Object.Output_string_list(string_list);
        }

        public string Get_input(string explanatory_message, string debug_description = "")
        {
            _get_input_messages.Add(explanatory_message);
            return _mock_input_output.Object.Get_input(explanatory_message, debug_description);
        }

        public string Get_generic_input(string debug_description)
        {
            return _mock_input_output.Object.Get_generic_input(debug_description);
        }

        public void Show_error(Exception exception)
        {
            _mock_input_output.Object.Show_error(exception);
        }
    }
}
