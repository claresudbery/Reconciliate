using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleCatchall.Console.Reconciliation.Extensions
{
    internal static class StringExtensions
    {
        public static bool Is_alpha(this string source)
        {
            Throw_error_if_string_is_empty(source);

            return Char.IsLetter(source[0]);
        }

        public static bool Is_numeric(this string source)
        {
            Throw_error_if_string_is_empty(source);

            return Char.IsDigit(source[0]) 
                || source[0] == '-' && Char.IsDigit(source[1]);
        }

        public static string Replace_commas_surrounded_by_spaces(this string source)
        {
            return source
                .Get_rid_of_spaces_before_comma_before_digit()
                .Replace(" , ", " ; ")
                .Replace(", ", "; ")
                .Replace(" ,", " ;");
        }

        public static string Get_rid_of_spaces_before_comma_before_digit(this string source)
        {
            return Regex.Replace(source, @"(([\S])+) +,([\d-])", @"$1,$3");
        }

        public static string Encase_in_escaped_quotes_if_not_already_encased(this string source)
        {
            Throw_error_if_string_is_empty(source);

            var result = source[0] != '\"' 
                ? "\"" + source + "\""
                : source;

            return result;
        }

        public static string Trim_amount(this string amount)
        {
            amount = amount.TrimStart('\"').TrimEnd('\"');
            return amount.StartsWith("-")
                    ? '-' + amount.Substring(1).Remove_leading_non_numeric_character()
                    : amount.Remove_leading_non_numeric_character();
        }

        public static string Remove_leading_non_numeric_character(this string source)
        {
            return source.Is_numeric()
                ? source
                : source.Substring(1);
        }

        public static string Convert_separators(this string source, char original_separator, char new_separator)
        {
            return source.Replace(original_separator, new_separator);
        }

        private static void Throw_error_if_string_is_empty(string source)
        {
            if (String.IsNullOrEmpty(source))
            {
                throw new Exception("Unexpected empty string. Is there a BankIn or BankOut record where the description has been placed in the type field (probably while in Tracking)?");
            }
        }

        public static string Strip_leading_apostrophe(this string source)
        {
            return source[0] == '\''
                ? source.Substring(1)
                : source;

        }

        public static string Replace_punctuation_with_spaces(this string source)
        {
            return source.Aggregate("", (current, c) =>
                char.IsPunctuation(c)
                ? current + ' '
                : current + c);
        }

        public static string Remove_punctuation(this string source)
        {
            return source.Where(c => !char.IsPunctuation(c)).Aggregate("", (current, c) => current + c);
        }
    }
}
