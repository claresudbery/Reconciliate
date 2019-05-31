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
            for (int lineCount = 1; lineCount < options.Count; lineCount++)
            {
                foreach (var consoleLine in options[lineCount].ConsoleLines)
                {
                    OutputLine(consoleLine.GetConsoleSnippets(options[lineCount]));
                }
                if (options[lineCount].ConsoleLines.Count > 1)
                {
                    OutputLine("..............");
                }
            }
        }

        public void OutputAllLines(List<IPotentialMatch> options)
        {
            foreach (var option in options)
            {
                foreach (var consoleLine in option.ConsoleLines)
                {
                    OutputLine(consoleLine.GetConsoleSnippets(option));
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
            for (int snippetIndex = 0; snippetIndex < consoleSnippets.Count; snippetIndex++)
            {
                System.Console.ForegroundColor = GetColour(consoleSnippets[snippetIndex].TextColour);
                System.Console.Write(consoleSnippets[snippetIndex].Text);
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
