using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces.Extensions;
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
            var result = source.Replace_punctuation_with_spaces();

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
            var result = source.Remove_punctuation();

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
            var result = source.Replace_commas_surrounded_by_spaces();

            // Assert
            Assert.AreEqual(expected_result, result);
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
            var result = source.Get_rid_of_spaces_before_comma_before_digit();

            // Assert
            Assert.AreEqual(expected_result, result);
        }

        [TestCase("\"text\"", "text")]
        [TestCase("\"text", "text")]
        [TestCase("text\"", "text")]
        [TestCase("text", "text")]
        public void When_stripping_enclosing_quotes_start_and_end_quotes_will_be_removed(string source, string expected_result)
        {
            // Act
            var result = source.Strip_enclosing_quotes();

            // Assert
            Assert.AreEqual(expected_result, result);
        }
    }
}