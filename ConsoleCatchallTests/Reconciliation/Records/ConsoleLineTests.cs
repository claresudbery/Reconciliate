using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces.Constants;
using Interfaces.DTOs;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Records
{
    [TestFixture]
    public class ConsoleLineTests
    {
        [Test]
        public void WillSetIndexAsFirstConsoleSnippet()
        {
            // Arrange
            var index = 23;
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = index };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual($"{index}. ", console_snippets[0].Text);
        }

        [Test]
        public void WillSetIndexTextColourAsWhite()
        {
            // Arrange
            var index = 23;
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = index };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[0].TextColour);
        }

        [Test]
        public void WillSetDateAsSecondConsoleSnippet()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(console_line.DateString + ",", console_snippets[1].Text);
        }

        [Test]
        [TestCase(6)]
        [TestCase(10)]
        public void WillSetDateTextColourAsWhite_IfDateRankingIsGreaterThanFive(int dateRanking)
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Date = dateRanking
                }
            };
            var console_line = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[1].TextColour);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void WillSetDateTextColourAsDarkYellow_IfDateRankingIsLessThanOrEqualToTwo(int dateRanking)
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Date = dateRanking
                },
            };
            var console_line = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, console_snippets[1].TextColour);
        }

        [Test]
        public void WillSetDateTextColourAsGreen_IfDateRankingIsZero()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Date = 0
                },
            };
            var console_line = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, console_snippets[1].TextColour);
        }

        [Test]
        public void WillSetDateTextColourAsWhite_IfRankingsAreNull()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = null
            };
            var console_line = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[1].TextColour);
        }

        [Test]
        public void WillSetAmountAsThirdConsoleSnippet()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(console_line.AmountString + ",", console_snippets[2].Text);
        }

        [Test]
        public void WillSetAmountTextColourAsWhite_IfAmountIsNotAMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Amount = 100
                },
                AmountMatch = true // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[2].TextColour);
        }

        [Test]
        public void WillSetAmountTextColourAsDarkYellow_IfAmountIsAPartialMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Amount = 1
                },
                AmountMatch = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, console_snippets[2].TextColour);
        }

        [Test]
        public void WillSetAmountTextColourAsGreen_IfAmountRankingIsZero()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Amount = 0
                },
                AmountMatch = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, console_snippets[2].TextColour);
        }

        [Test]
        public void WillSetAmountTextColourAsWhite_IfRankingsAreNull()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = null,
                AmountMatch = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[2].TextColour);
        }

        [Test]
        public void WillSetDescriptionAsFourthConsoleSnippet()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(console_line.DescriptionString, console_snippets[3].Text);
        }

        [Test]
        public void WillSetDescriptionTextColourAsWhite_IfThereIsNoTextMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                FullTextMatch = false,
                PartialTextMatch = false,
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[3].TextColour);
        }

        [Test]
        public void WillSetDescriptionTextColourAsDarkYellow_IfThereIsAPartialTextMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                FullTextMatch = false,
                PartialTextMatch = true,
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, console_snippets[3].TextColour);
        }

        [Test]
        public void WillSetDescriptionTextColourAsGreen_IfThereIsAFullTextMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                FullTextMatch = true,
                PartialTextMatch = true,
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, console_snippets[3].TextColour);
        }
        [Test]
        public void WillRegenerateConsoleSnippetIfIndexChanges()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = 1 };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.GetConsoleSnippets(potential_match);
            var new_index = console_line.Index + 1;
            console_line.Index = new_index;

            // Act
            var new_result = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual($"{new_index}. ", new_result[0].Text, "Index text should be updated");
        }

        [Test]
        public void WillRegenerateConsoleSnippetIfDateChanges()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { DateString = "19/10/2018" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.GetConsoleSnippets(potential_match);
            var new_date = "New " + console_line.DateString;
            console_line.DateString = new_date;

            // Act
            var new_result = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual($"{new_date},", new_result[1].Text, "Date text should be updated");
        }

        [Test]
        public void WillRegenerateConsoleSnippetIfAmountChanges()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { AmountString = "£22.34" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.GetConsoleSnippets(potential_match);
            var new_amount = "New " + console_line.AmountString;
            console_line.AmountString = new_amount;

            // Act
            var new_result = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual($"{new_amount},", new_result[2].Text, "Amount text should be updated");
        }

        [Test]
        public void WillRegenerateConsoleSnippetIfDescriptionChanges()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { DescriptionString = "For Aloysius" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.GetConsoleSnippets(potential_match);
            var new_description = "New " + console_line.DescriptionString;
            console_line.DescriptionString = new_description;

            // Act
            var new_result = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual(new_description, new_result[3].Text, "Description text should be updated");
        }

        [Test]
        public void WillSetDateColourToGreenIfRankingsAreNull()
        {
            // Arrange
            var index = 23;
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = index };

            // Act
            var console_snippets = console_line.GetConsoleSnippets(potential_match);

            // Assert
            Assert.AreEqual($"{index}. ", console_snippets[0].Text);
        }
    }
}