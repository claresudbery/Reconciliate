using System;
using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface IInputOutput
    {
        void OutputOptions(List<string> options);
        void OutputAllLinesExceptTheFirst(List<IPotentialMatch> options);
        void OutputAllLines(List<IPotentialMatch> options);
        void OutputAllLines(List<ConsoleLine> consoleLines);
        void OutputLine(List<ConsoleSnippet> consoleSnippets);
        void OutputLine(ConsoleLine line);
        void OutputLineWithIndex(ConsoleLine line);
        void OutputLine(string line);
        void Output(string text);
        void OutputStringList(List<string> stringList);
        string GetInput(string explanatoryMessage, string debugDescription = "");
        string GetGenericInput(string debugDescription);
        void ShowError(Exception exception);
    }
}