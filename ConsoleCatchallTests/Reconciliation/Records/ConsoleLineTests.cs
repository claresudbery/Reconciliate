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
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { Index = index };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual($"{index}. ", consoleSnippets[0].Text);
        }

        [Test]
        public void WillSetIndexTextColourAsWhite()
        {
            // Arrange
            var index = 23;
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { Index = index };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.White, consoleSnippets[0].TextColour);
        }

        [Test]
        public void WillSetDateAsSecondConsoleSnippet()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(consoleLine.DateString + ",", consoleSnippets[1].Text);
        }

        [Test]
        [TestCase(6)]
        [TestCase(10)]
        public void WillSetDateTextColourAsWhite_IfDateRankingIsGreaterThanFive(int dateRanking)
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Date = dateRanking
                }
            };
            var consoleLine = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.White, consoleSnippets[1].TextColour);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void WillSetDateTextColourAsDarkYellow_IfDateRankingIsLessThanOrEqualToTwo(int dateRanking)
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Date = dateRanking
                },
            };
            var consoleLine = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, consoleSnippets[1].TextColour);
        }

        [Test]
        public void WillSetDateTextColourAsGreen_IfDateRankingIsZero()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Date = 0
                },
            };
            var consoleLine = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, consoleSnippets[1].TextColour);
        }

        [Test]
        public void WillSetDateTextColourAsWhite_IfRankingsAreNull()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = null
            };
            var consoleLine = new ConsoleLine { DateString = "10/10/2018" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.White, consoleSnippets[1].TextColour);
        }

        [Test]
        public void WillSetAmountAsThirdConsoleSnippet()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(consoleLine.AmountString + ",", consoleSnippets[2].Text);
        }

        [Test]
        public void WillSetAmountTextColourAsWhite_IfAmountIsNotAMatch()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Amount = 100
                },
                AmountMatch = true // AmountMatch should be ignored - AmountRanking is used instead
            };
            var consoleLine = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.White, consoleSnippets[2].TextColour);
        }

        [Test]
        public void WillSetAmountTextColourAsDarkYellow_IfAmountIsAPartialMatch()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Amount = 1
                },
                AmountMatch = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var consoleLine = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, consoleSnippets[2].TextColour);
        }

        [Test]
        public void WillSetAmountTextColourAsGreen_IfAmountRankingIsZero()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings
                {
                    Amount = 0
                },
                AmountMatch = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var consoleLine = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, consoleSnippets[2].TextColour);
        }

        [Test]
        public void WillSetAmountTextColourAsWhite_IfRankingsAreNull()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = null,
                AmountMatch = false // AmountMatch should be ignored - AmountRanking is used instead
            };
            var consoleLine = new ConsoleLine { AmountString = "£34.55" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.White, consoleSnippets[2].TextColour);
        }

        [Test]
        public void WillSetDescriptionAsFourthConsoleSnippet()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(consoleLine.DescriptionString, consoleSnippets[3].Text);
        }

        [Test]
        public void WillSetDescriptionTextColourAsWhite_IfThereIsNoTextMatch()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                FullTextMatch = false,
                PartialTextMatch = false,
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.White, consoleSnippets[3].TextColour);
        }

        [Test]
        public void WillSetDescriptionTextColourAsDarkYellow_IfThereIsAPartialTextMatch()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                FullTextMatch = false,
                PartialTextMatch = true,
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.DarkYellow, consoleSnippets[3].TextColour);
        }

        [Test]
        public void WillSetDescriptionTextColourAsGreen_IfThereIsAFullTextMatch()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                FullTextMatch = true,
                PartialTextMatch = true,
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { DescriptionString = "Some description" };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(ConsoleColour.Green, consoleSnippets[3].TextColour);
        }
        [Test]
        public void WillRegenerateConsoleSnippetIfIndexChanges()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { Index = 1 };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            consoleLine.GetConsoleSnippets(potentialMatch);
            var newIndex = consoleLine.Index + 1;
            consoleLine.Index = newIndex;

            // Act
            var newResult = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual($"{newIndex}. ", newResult[0].Text, "Index text should be updated");
        }

        [Test]
        public void WillRegenerateConsoleSnippetIfDateChanges()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { DateString = "19/10/2018" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            consoleLine.GetConsoleSnippets(potentialMatch);
            var newDate = "New " + consoleLine.DateString;
            consoleLine.DateString = newDate;

            // Act
            var newResult = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual($"{newDate},", newResult[1].Text, "Date text should be updated");
        }

        [Test]
        public void WillRegenerateConsoleSnippetIfAmountChanges()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { AmountString = "£22.34" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            consoleLine.GetConsoleSnippets(potentialMatch);
            var newAmount = "New " + consoleLine.AmountString;
            consoleLine.AmountString = newAmount;

            // Act
            var newResult = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual($"{newAmount},", newResult[2].Text, "Amount text should be updated");
        }

        [Test]
        public void WillRegenerateConsoleSnippetIfDescriptionChanges()
        {
            // Arrange
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { DescriptionString = "For Aloysius" };
            // This is to test for a buug which only happens if you call GetConsoleSnippets() twice, so call it once before the main action.
            consoleLine.GetConsoleSnippets(potentialMatch);
            var newDescription = "New " + consoleLine.DescriptionString;
            consoleLine.DescriptionString = newDescription;

            // Act
            var newResult = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual(newDescription, newResult[3].Text, "Description text should be updated");
        }

        [Test]
        public void WillSetDateColourToGreenIfRankingsAreNull()
        {
            // Arrange
            var index = 23;
            var potentialMatch = new PotentialMatch
            {
                Rankings = new Rankings()
            };
            var consoleLine = new ConsoleLine { Index = index };

            // Act
            var consoleSnippets = consoleLine.GetConsoleSnippets(potentialMatch);

            // Assert
            Assert.AreEqual($"{index}. ", consoleSnippets[0].Text);
        }
    }
}