using System;
using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface IInputOutput
    {
        void Output_options(List<string> options);
        void Output_all_lines_except_the_first(List<IPotentialMatch> options);
        void Output_all_lines(List<IPotentialMatch> options);
        void Output_all_lines(List<ConsoleLine> console_lines);
        void Output_line(List<ConsoleSnippet> console_snippets);
        void Output_line(ConsoleLine line);
        void Output_line_with_index(ConsoleLine line);
        void Output_line(string line);
        void Output(string text);
        void Output_string_list(List<string> string_list);
        string Get_input(string explanatory_message, string debug_description = "");
        string Get_generic_input(string debug_description);
        void Show_error(Exception exception);
    }
}