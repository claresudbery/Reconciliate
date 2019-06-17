using System;
using System.Collections.Generic;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console
{
    internal class InputOutput : IInputOutput
    {
        public void Output_options(List<string> options)
        {
            foreach (var option in options)
            {
                System.Console.WriteLine(option);
            }
        }

        public void Output_all_lines_except_the_first(List<IPotentialMatch> options)
        {
            for (int line_count = 1; line_count < options.Count; line_count++)
            {
                foreach (var console_line in options[line_count].Console_lines)
                {
                    Output_line(console_line.Get_console_snippets(options[line_count]));
                }
                if (options[line_count].Console_lines.Count > 1)
                {
                    Output_line("..............");
                }
            }
        }

        public void Output_all_lines(List<IPotentialMatch> options)
        {
            foreach (var option in options)
            {
                foreach (var console_line in option.Console_lines)
                {
                    Output_line(console_line.Get_console_snippets(option));
                }
            }
        }

        public void Output_all_lines(List<ConsoleLine> consoleLines)
        {
            foreach (var line in consoleLines)
            {
                Output_line_with_index(line);
            }
        }

        public void Output_line(List<ConsoleSnippet> consoleSnippets)
        {
            for (int snippet_index = 0; snippet_index < consoleSnippets.Count; snippet_index++)
            {
                System.Console.ForegroundColor = Get_colour(consoleSnippets[snippet_index].Text_colour);
                System.Console.Write(consoleSnippets[snippet_index].Text);
                System.Console.ResetColor();
            }
            System.Console.WriteLine("");
        }

        public void Output_line(ConsoleLine line)
        {
            System.Console.Write(line.Date_string);
            System.Console.Write(",");
            System.Console.Write(line.Amount_string);
            System.Console.Write(",");
            System.Console.WriteLine(line.Description_string);
        }

        public void Output_line_with_index(ConsoleLine line)
        {
            System.Console.Write(line.Index);
            System.Console.Write(". ");
            Output_line(line);
        }

        public void Output_line(string line)
        {
            System.Console.WriteLine(line);
        }

        public void Output(string text)
        {
            System.Console.Write(text);
        }

        private ConsoleColor Get_colour(ConsoleColour colour)
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

        public void Output_string_list(List<string> stringList)
        {
            foreach (var output in stringList)
            {
                System.Console.WriteLine(output);
            }
        }

        public string Get_input(string explanatoryMessage, string debugDescription = "")
        {
            System.Console.WriteLine("");
            System.Console.WriteLine(explanatoryMessage);

            return Get_generic_input(explanatoryMessage);
        }

        public string Get_generic_input(string debugDescription)
        {
            string result = System.Console.ReadLine();
            Check_for_exit(result);

            return result;
        }

        private void Check_for_exit(string input)
        {
            if (input.ToUpper() == "EXIT")
            {
                throw new Exception("Exit");
            }
        }

        public void Show_error(Exception exception)
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
