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
        void Output_all_lines(List<ConsoleLine> consoleLines);
        void Output_line(List<ConsoleSnippet> consoleSnippets);
        void Output_line(ConsoleLine line);
        void Output_line_with_index(ConsoleLine line);
        void Output_line(string line);
        void Output(string text);
        void Output_string_list(List<string> stringList);
        string Get_input(string explanatoryMessage, string debugDescription = "");
        string Get_generic_input(string debugDescription);
        void Show_error(Exception exception);
    }
}