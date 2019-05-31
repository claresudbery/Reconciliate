using System.Collections.Generic;
using Interfaces.Constants;

namespace Interfaces.DTOs
{
    public class ConsoleLine
    {
        public int Index { get; set; }
        public string DateString { get; set; }
        public string AmountString { get; set; }
        public string DescriptionString { get; set; }

        private List<ConsoleSnippet> _consoleSnippets;

        public string AsTextLine()
        {
            return $"{Index}. {DateString},{AmountString},{DescriptionString}";
        }

        public ConsoleLine AsSeparator(int index)
        {
            Index = index;
            AmountString = "------";
            DateString = "----------";
            DescriptionString = "------------------------------------------------";
            return this;
        }

        public List<ConsoleSnippet> GetConsoleSnippets(IPotentialMatch potentialMatch)
        {
            if (_consoleSnippets == null)
            {
                _consoleSnippets = new List<ConsoleSnippet>();
            }
            else
            {
                _consoleSnippets.Clear();
            }
            _consoleSnippets.Add(IndexAsConsoleSnippet());
            _consoleSnippets.Add(DateAsConsoleSnippet(potentialMatch));
            _consoleSnippets.Add(AmountAsConsoleSnippet(potentialMatch));
            _consoleSnippets.Add(DescriptionAsConsoleSnippet(potentialMatch));
            return _consoleSnippets;
        }

        private static ConsoleColour GetColour(bool fullMatch, bool partialMatch)
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

        private ConsoleSnippet IndexAsConsoleSnippet()
        {
            return new ConsoleSnippet
            {
                Text = $"{Index}. ",
                TextColour = ConsoleColour.White
            };
        }

        private ConsoleSnippet DateAsConsoleSnippet(IPotentialMatch potentialMatch)
        {
            return new ConsoleSnippet
            {
                Text = DateString + ",",
                TextColour = GetColour(
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Date == 0,
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Date <= ReconConsts.PartialDateMatchThreshold)
            };
        }

        private ConsoleSnippet AmountAsConsoleSnippet(IPotentialMatch potentialMatch)
        {
            return new ConsoleSnippet
            {
                Text = AmountString + ",",
                TextColour = GetColour(
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Amount == 0,
                    potentialMatch.Rankings != null && potentialMatch.Rankings.Amount <= ReconConsts.PartialAmountMatchThreshold)
            };
        }

        private ConsoleSnippet DescriptionAsConsoleSnippet(IPotentialMatch potentialMatch)
        {
            return new ConsoleSnippet
            {
                Text = DescriptionString,
                TextColour = GetColour(potentialMatch.FullTextMatch, potentialMatch.PartialTextMatch)
            };
        }
    }
}