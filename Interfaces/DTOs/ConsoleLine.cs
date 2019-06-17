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

        public List<ConsoleSnippet> Get_console_snippets(IPotentialMatch potential_match)
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
            _console_snippets.Add(Date_as_console_snippet(potential_match));
            _console_snippets.Add(Amount_as_console_snippet(potential_match));
            _console_snippets.Add(Description_as_console_snippet(potential_match));
            return _console_snippets;
        }

        private static ConsoleColour Get_colour(bool full_match, bool partial_match)
        {
            var colour = ConsoleColour.White;

            if (full_match)
            {
                colour = ConsoleColour.Green;
            }
            else
            if (partial_match)
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

        private ConsoleSnippet Date_as_console_snippet(IPotentialMatch potential_match)
        {
            return new ConsoleSnippet
            {
                Text = Date_string + ",",
                Text_colour = Get_colour(
                    potential_match.Rankings != null && potential_match.Rankings.Date == 0,
                    potential_match.Rankings != null && potential_match.Rankings.Date <= ReconConsts.PartialDateMatchThreshold)
            };
        }

        private ConsoleSnippet Amount_as_console_snippet(IPotentialMatch potential_match)
        {
            return new ConsoleSnippet
            {
                Text = Amount_string + ",",
                Text_colour = Get_colour(
                    potential_match.Rankings != null && potential_match.Rankings.Amount == 0,
                    potential_match.Rankings != null && potential_match.Rankings.Amount <= ReconConsts.PartialAmountMatchThreshold)
            };
        }

        private ConsoleSnippet Description_as_console_snippet(IPotentialMatch potential_match)
        {
            return new ConsoleSnippet
            {
                Text = Description_string,
                Text_colour = Get_colour(potential_match.Full_text_match, potential_match.Partial_text_match)
            };
        }
    }
}