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
        public void Will_set_index_as_first_console_snippet()
        {
            // Arrange
            var index = 23;
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = index };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual($"{index}. ", console_snippets[0].Text);
        }

        [Test]
        public void Will_set_index_text_colour_as_white()
        {
            // Arrange
            var index = 23;
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = index };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[0].Text_colour);
        }

        [Test]
        public void Will_set_date_as_second_console_snippet()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Date_string = "10/10/2018" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(console_line.Date_string + ",", console_snippets[1].Text);
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
            var console_line = new ConsoleLine { Date_string = "10/10/2018" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[1].Text_colour);
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
            var console_line = new ConsoleLine { Date_string = "10/10/2018" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, console_snippets[1].Text_colour);
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
            var console_line = new ConsoleLine { Date_string = "10/10/2018" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, console_snippets[1].Text_colour);
        }

        [Test]
        public void WillSetDateTextColourAsWhite_IfRankingsAreNull()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = null
            };
            var console_line = new ConsoleLine { Date_string = "10/10/2018" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[1].Text_colour);
        }

        [Test]
        public void Will_set_amount_as_third_console_snippet()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Amount_string = "£34.55" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(console_line.Amount_string + ",", console_snippets[2].Text);
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
                Amount_match = true // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { Amount_string = "£34.55" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[2].Text_colour);
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
                Amount_match = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { Amount_string = "£34.55" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, console_snippets[2].Text_colour);
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
                Amount_match = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { Amount_string = "£34.55" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, console_snippets[2].Text_colour);
        }

        [Test]
        public void WillSetAmountTextColourAsWhite_IfRankingsAreNull()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = null,
                Amount_match = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var console_line = new ConsoleLine { Amount_string = "£34.55" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[2].Text_colour);
        }

        [Test]
        public void Will_set_description_as_fourth_console_snippet()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Description_string = "Some description" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(console_line.Description_string, console_snippets[3].Text);
        }

        [Test]
        public void WillSetDescriptionTextColourAsWhite_IfThereIsNoTextMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Full_text_match = false,
                Partial_text_match = false,
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Description_string = "Some description" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.White, console_snippets[3].Text_colour);
        }

        [Test]
        public void WillSetDescriptionTextColourAsDarkYellow_IfThereIsAPartialTextMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Full_text_match = false,
                Partial_text_match = true,
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Description_string = "Some description" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, console_snippets[3].Text_colour);
        }

        [Test]
        public void WillSetDescriptionTextColourAsGreen_IfThereIsAFullTextMatch()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Full_text_match = true,
                Partial_text_match = true,
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Description_string = "Some description" };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, console_snippets[3].Text_colour);
        }
        [Test]
        public void Will_regenerate_console_snippet_if_index_changes()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = 1 };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.Get_console_snippets(potential_match);
            var new_index = console_line.Index + 1;
            console_line.Index = new_index;

            // Act
            var new_result = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual($"{new_index}. ", new_result[0].Text, "Index text should be updated");
        }

        [Test]
        public void Will_regenerate_console_snippet_if_date_changes()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Date_string = "19/10/2018" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.Get_console_snippets(potential_match);
            var new_date = "New " + console_line.Date_string;
            console_line.Date_string = new_date;

            // Act
            var new_result = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual($"{new_date},", new_result[1].Text, "Date text should be updated");
        }

        [Test]
        public void Will_regenerate_console_snippet_if_amount_changes()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Amount_string = "£22.34" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.Get_console_snippets(potential_match);
            var new_amount = "New " + console_line.Amount_string;
            console_line.Amount_string = new_amount;

            // Act
            var new_result = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual($"{new_amount},", new_result[2].Text, "Amount text should be updated");
        }

        [Test]
        public void Will_regenerate_console_snippet_if_description_changes()
        {
            // Arrange
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Description_string = "For Aloysius" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            console_line.Get_console_snippets(potential_match);
            var new_description = "New " + console_line.Description_string;
            console_line.Description_string = new_description;

            // Act
            var new_result = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual(new_description, new_result[3].Text, "Description text should be updated");
        }

        [Test]
        public void Will_set_date_colour_to_green_if_rankings_are_null()
        {
            // Arrange
            var index = 23;
            var potential_match = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var console_line = new ConsoleLine { Index = index };

            // Act
            var console_snippets = console_line.Get_console_snippets(potential_match);

            // Assert
            Assert.AreEqual($"{index}. ", console_snippets[0].Text);
        }
    }
}