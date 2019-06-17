using System.Collections.Generic;
using Interfaces.Constants;

namespace Interfaces.DTOs
{
    public class ConsoleLine
    {
        public int Index { get; set; }
        public string Date_string { get; set; }
        public string Amount_string { get; set; }
        public string Description_string { get; set; }

        private List<ConsoleSnippet> _console_snippets;

        public string As_text_line()
        {
            return $"{Index}. {Date_string},{Amount_string},{Description_string}";
        }

        public ConsoleLine As_separator(int index)
        {
            Index = index;
            Amount_string = "------";
            Date_string = "----------";
            Description_string = "------------------------------------------------";
            return this;
        }

        public List<ConsoleSnippet> Get_console_snippets(IPotentialMatch potentialMatch)
        {
            if (_console_snippets == null)
            {
                _console_snippets = new List<ConsoleSnippet>();
            }
            else
            {
                _console_snippets.Clear();
            }
            _console_snippets.Add(Index_as_console_snippet());
            _console_snippets.Add(Date_as_console_snippet(potentialMatch));
            _console_snippets.Add(Amount_as_console_snippet(potentialMatch));
            _console_snippets.Add(Description_as_console_snippet(potentialMatch));
            return _console_snippets;
        }

        private static ConsoleColour Get_colour(bool fullMatch, bool partialMatch)
        {
            var colour = ConsoleColour.White;

            if (fullMatch)
            {
                colour = ConsoleColour.Green;
            }
            else
            if (partialMatch)
            {
                colour = ConsoleColour.DarkYellow;
            }

            return colour;
        }

        private ConsoleSnippet Index_as_console_snippet()
        {
            return new ConsoleSnippet
            {
                Text = $"{Index}. ",
                Text_colour = ConsoleColour.White
            };
        }

        private ConsoleSnippet Date_as_console_snippet(IPotentialMatch potentialMatch)
        {
            return new ConsoleSnippet
            {
                Text = Date_string + ",",
                Text_colour = Get_colour(
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Date == 0,
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Date <= ReconConsts.PartialDateMatchThreshold)
            };
        }

        private ConsoleSnippet Amount_as_console_snippet(IPotentialMatch potentialMatch)
        {
            return new ConsoleSnippet
            {
                Text = Amount_string + ",",
                Text_colour = Get_colour(
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Amount == 0,
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Amount <= ReconConsts.PartialAmountMatchThreshold)
            };
        }

        private ConsoleSnippet Description_as_console_snippet(IPotentialMatch potentialMatch)
        {
            return new ConsoleSnippet
            {
                Text = Description_string,
                Text_colour = Get_colour(potentialMatch.Full_text_match, potentialMatch.Partial_text_match)
            };
        }
    }
}