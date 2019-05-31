using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleCatchall.Console.Reconciliation.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsAlpha(this string source)
        {
            ThrowErrorIfStringIsEmpty(source);

            return Char.IsLetter(source[0]);
        }

        public static bool IsNumeric(this string source)
        {
            ThrowErrorIfStringIsEmpty(source);

            return Char.IsDigit(source[0]) 
                || source[0] == '-' && Char.IsDigit(source[1]);
        }

        public static string ReplaceCommasSurroundedBySpaces(this string source)
        {
            return source
                .GetRidOfSpacesBeforeCommaBeforeDigit()
                .Replace(" , ", " ; ")
                .Replace(", ", "; ")
                .Replace(" ,", " ;");
        }

        public static string GetRidOfSpacesBeforeCommaBeforeDigit(this string source)
        {
            return Regex.Replace(source, @"(([\S])+) +,([\d-])", @"$1,$3");
        }

        public static string EncaseInEscapedQuotesIfNotAlreadyEncased(this string source)
        {
            ThrowErrorIfStringIsEmpty(source);

            var result = source[0] != '\"' 
                ? "\"" + source + "\""
                : source;

            return result;
        }

        public static string TrimAmount(this string amount)
        {
            amount = amount.TrimStart('\"').TrimEnd('\"');
            return amount.StartsWith("-")
                    ? '-' + amount.Substring(1).RemoveLeadingNonNumericCharacter()
                    : amount.RemoveLeadingNonNumericCharacter();
        }

        public static string RemoveLeadingNonNumericCharacter(this string source)
        {
            return source.IsNumeric()
                ? source
                : source.Substring(1);
        }

        public static string ConvertSeparators(this string source, char originalSeparator, char newSeparator)
        {
            return source.Replace(originalSeparator, newSeparator);
        }

        private static void ThrowErrorIfStringIsEmpty(string source)
        {
            if (String.IsNullOrEmpty(source))
            {
                throw new Exception("Unexpected empty string. Is there a BankIn or BankOut record where the description has been placed in the type field (probably while in Tracking)?");
            }
        }

        public static string StripLeadingApostrophe(this string source)
        {
            return source[0] == '\''
                ? source.Substring(1)
                : source;

        }

        public static string ReplacePunctuationWithSpaces(this string source)
        {
            return source.Aggregate("", (current, c) =>
                char.IsPunctuation(c)
                ? current + ' '
                : current + c);
        }

        public static string RemovePunctuation(this string source)
        {
            return source.Where(c => !char.IsPunctuation(c)).Aggregate("", (current, c) => current + c);
        }
    }
}
