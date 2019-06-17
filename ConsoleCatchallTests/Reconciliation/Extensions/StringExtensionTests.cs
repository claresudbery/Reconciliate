using ConsoleCatchall.Console.Reconciliation.Extensions;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Extensions
{
    [TestFixture]
    public class StringExtensionTests
    {
        [TestCase('(')]
        [TestCase(')')]
        [TestCase('-')]
        [TestCase('_')]
        [TestCase(';')]
        [TestCase(':')]
        [TestCase(',')]
        [TestCase('.')]
        [TestCase('\'')]
        [TestCase('#')]
        [TestCase('!')]
        [TestCase('?')]
        public void WhenReplacingPunctuation_AllPunctuationCharsWillBeReplacedBySpaces(char punctuation)
        {
            // Arrange
            var source = $"A{punctuation}string{punctuation}with{punctuation}much{punctuation}punctuation{punctuation}";

            // Act
            var result = source.ReplacePunctuationWithSpaces();

            // Assert
            Assert.AreEqual("A string with much punctuation ", result);
        }

        [TestCase('(')]
        [TestCase(')')]
        [TestCase('-')]
        [TestCase('_')]
        [TestCase(';')]
        [TestCase(':')]
        [TestCase(',')]
        [TestCase('.')]
        [TestCase('\'')]
        [TestCase('#')]
        [TestCase('!')]
        [TestCase('?')]
        public void WhenRemovingPunctuation_AllPunctuationCharsWillBeRemoved(char punctuation)
        {
            // Arrange
            var source = $"A{punctuation}string{punctuation}with {punctuation}much{punctuation}punctuation{punctuation}";

            // Act
            var result = source.RemovePunctuation();

            // Assert
            Assert.AreEqual("Astringwith muchpunctuation", result);
        }

        [TestCase("text, text", "text; text")]
        [TestCase("text ,text", "text ;text")]
        [TestCase("text , text", "text ; text")]
        [TestCase("text ,23.45", "text,23.45")]
        [TestCase("text, 23.45", "text; 23.45")]
        [TestCase("text , 23.45", "text ; 23.45")]
        public void Can_replace_commas_surrounded_by_spaces(string source, string expected_result)
        {
            // Act
            var result = source.ReplaceCommasSurroundedBySpaces();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("text ,text", "text ,text")]
        [TestCase("text     ,text", "text     ,text")]
        [TestCase("text ,23.45", "text,23.45")]
        [TestCase("text     ,23.45", "text,23.45")]
        [TestCase("just the letter R ,23.45", "just the letter R,23.45")]
        [TestCase("just the letter T      ,23.45", "just the letter T,23.45")]
        [TestCase("text ,-23.45", "text,-23.45")]
        [TestCase("text     ,-23.45", "text,-23.45")]
        [TestCase("PAYMENT FOR STUFF -  ,-433.96", "PAYMENT FOR STUFF -,-433.96")]
        public void Can_get_rid_of_spaces_before_comma_before_digit(string source, string expected_result)
        {
            // Act
            var result = source.GetRidOfSpacesBeforeCommaBeforeDigit();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}