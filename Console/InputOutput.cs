using System;
using System.Collections.Generic;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console
{
    internal class InputOutput : IInputOutput
    {
        public void OutputOptions(List<string> options)
        {
            foreach (var option in options)
            {
                System.Console.WriteLine(option);
            }
        }

        public void OutputAllLinesExceptTheFirst(List<IPotentialMatch> options)
        {
            for (int line_count = 1; line_count < options.Count; line_count++)
            {
                foreach (var console_line in options[line_count].ConsoleLines)
                {
                    OutputLine(console_line.GetConsoleSnippets(options[line_count]));
                }
                if (options[line_count].ConsoleLines.Count > 1)
                {
                    OutputLine("..............");
                }
            }
        }

        public void OutputAllLines(List<IPotentialMatch> options)
        {
            foreach (var option in options)
            {
                foreach (var console_line in option.ConsoleLines)
                {
                    OutputLine(console_line.GetConsoleSnippets(option));
                }
            }
        }

        public void OutputAllLines(List<ConsoleLine> consoleLines)
        {
            foreach (var line in consoleLines)
            {
                OutputLineWithIndex(line);
            }
        }

        public void OutputLine(List<ConsoleSnippet> consoleSnippets)
        {
            for (int snippet_index = 0; snippet_index < consoleSnippets.Count; snippet_index++)
            {
                System.Console.ForegroundColor = GetColour(consoleSnippets[snippet_index].TextColour);
                System.Console.Write(consoleSnippets[snippet_index].Text);
                System.Console.ResetColor();
            }
            System.Console.WriteLine("");
        }

        public void OutputLine(ConsoleLine line)
        {
            System.Console.Write(line.DateString);
            System.Console.Write(",");
            System.Console.Write(line.AmountString);
            System.Console.Write(",");
            System.Console.WriteLine(line.DescriptionString);
        }

        public void OutputLineWithIndex(ConsoleLine line)
        {
            System.Console.Write(line.Index);
            System.Console.Write(". ");
            OutputLine(line);
        }

        public void OutputLine(string line)
        {
            System.Console.WriteLine(line);
        }

        public void Output(string text)
        {
            System.Console.Write(text);
        }

        private ConsoleColor GetColour(ConsoleColour colour)
        {
            switch (colour)
            {
                case ConsoleColour.DarkYellow: return ConsoleColor.DarkYellow; 
                case ConsoleColour.Green: return ConsoleColor.Green; 
                case ConsoleColour.Red: return ConsoleColor.Red; 
                case ConsoleColour.White: return ConsoleColor.White; 
                case ConsoleColour.Yellow: return ConsoleColor.Yellow; 
                    default: return ConsoleColor.White; 
            }
        }

        public void OutputStringList(List<string> stringList)
        {
            foreach (var output in stringList)
            {
                System.Console.WriteLine(output);
            }
        }

        public string GetInput(string explanatoryMessage, string debugDescription = "")
        {
            System.Console.WriteLine("");
            System.Console.WriteLine(explanatoryMessage);

            return GetGenericInput(explanatoryMessage);
        }

        public string GetGenericInput(string debugDescription)
        {
            string result = System.Console.ReadLine();
            CheckForExit(result);

            return result;
        }

        private void CheckForExit(string input)
        {
            if (input.ToUpper() == "EXIT")
            {
                throw new Exception("Exit");
            }
        }

        public void ShowError(Exception exception)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("Something went wrong:");
            System.Console.WriteLine(exception.Message);

            if (null != exception.InnerException)
            {
                System.Console.WriteLine(exception.InnerException.Message);
                System.Console.WriteLine(exception.InnerException.StackTrace);
            }

            System.Console.WriteLine("");
            System.Console.WriteLine("Type 'Exit' to leave.");
        }
    }
}
