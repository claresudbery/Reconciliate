using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class CredCard2Record : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; private set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }

        private char _separator = ',';
        private int _expectedNumberOfFieldsPerRow = 4;

        public const int DateIndex = 0;
        public const int AmountIndex = 2;
        public const int DescriptionIndex = 3;

        public const int DateSpreadsheetIndex = CredCard2InOutRecord.MatchOffset + 0;
        public const int AmountSpreadsheetIndex = CredCard2InOutRecord.MatchOffset + 1;
        public const int DescriptionSpreadsheetIndex = CredCard2InOutRecord.MatchOffset + 2;

        public void CreateFromMatch(DateTime date, double amount, string type, string description, int extraInfo, ICSVRecord matchedRecord)
        {
            Match = matchedRecord;
            Matched = true;

            Date = date;
            Amount = amount;
            Description = description;
        }

        public void Load(string csvLine, char? overrideSeparator = null)
        {
            csvLine = csvLine.ReplaceCommasSurroundedBySpaces();
            SourceLine = csvLine;
            var values = csvLine.Split(_separator);
            values = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(_expectedNumberOfFieldsPerRow, values);

            Date = values[DateIndex] != "" && values[DateIndex].IsNumeric()
                ? DateTime.ParseExact(values[DateIndex], "dd/MM/yyyy", CultureInfo.InvariantCulture)
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            // values[1] is a reference field that we don't care about.

            string simpleAmount = values[AmountIndex].TrimStart(new char[]{'"', ' '}).TrimEnd('"');
            Amount = Convert.ToDouble(simpleAmount != "" && simpleAmount.IsNumeric()
                ? simpleAmount
                : "0");

            Description = values[DescriptionIndex];
        }

        // Not currently used because the format of the file changed and they don't seem to use commas any more - and it's not the last field any more.
        private string GetAmountWithPossibleComma(string[] values)
        {
            if (values.Length == _expectedNumberOfFieldsPerRow + 1)
            {
                // This happens when the amount contains a comma.
                values[3] = values[3] + values[4];
            }

            return string.IsNullOrEmpty(values[3])
                ? String.Empty
                : values[3].TrimAmount(); 
        }

        public string ToCsv(bool formatCurrency = true)
        {
            return ToString(',', formatCurrency);
        }

        public string ToString(char separator = ',', bool formatCurrency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator
                + Amount.ToCsvString(formatCurrency) + separator
                + Description.EncaseInEscapedQuotesIfNotAlreadyEncased();
        }

        public ConsoleLine ToConsole(int index = -1)
        {
            return new ConsoleLine
            {
                Index = index,
                DateString = Date.ToString(@"dd\/MM\/yyyy"),
                AmountString = Amount.ToCsvString(true),
                DescriptionString = Description
            };
        }

        public void PopulateSpreadsheetRow(ICellSet cellSet, int rowNumber)
        {
            cellSet.PopulateCell(rowNumber, DateSpreadsheetIndex + 1, Date);
            cellSet.PopulateCell(rowNumber, AmountSpreadsheetIndex + 1, Amount);
            cellSet.PopulateCell(rowNumber, DescriptionSpreadsheetIndex + 1, Description);
        }

        public void ReadFromSpreadsheetRow(ICellRow cellSet)
        {
            Date = DateTime.FromOADate((double)cellSet.ReadCell(DateIndex));
            Amount = (Double)cellSet.ReadCell(AmountIndex);
            Description = (String)cellSet.ReadCell(DescriptionIndex);
        }

        public bool MainAmountIsNegative()
        {
            return Amount != 0 && Amount < 0;
        }

        public void MakeMainAmountPositive()
        {
            if (Amount < 0)
            {
                Amount = Amount * -1;
            }
        }

        public void SwapSignOfMainAmount()
        {
            Amount = Amount * -1;
        }

        public double MainAmount()
        {
            return Amount;
        }

        public void ChangeMainAmount(double newValue)
        {
            Amount = newValue;
        }

        public string TransactionType()
        {
            return String.Empty;
        }

        public int ExtraInfo()
        {
            return 0;
        }

        public ICSVRecord WithDate(DateTime newDate)
        {
            Date = newDate;
            return this;
        }

        public ICSVRecord Copy()
        {
            return new CredCard2Record
            {
                Date = Date,
                Description = Description,
                Amount = Amount,
                SourceLine = SourceLine
            };
        }

        public void UpdateSourceLineForOutput(char outputSeparator)
        {
            SourceLine = ToString(outputSeparator);
        }

        public void Reconcile()
        {
            // Do nothing.
        }

        public void ConvertSourceLineSeparators(char originalSeparator, char newSeparator)
        {
            SourceLine = SourceLine.ConvertSeparators(originalSeparator, newSeparator);
        }
    }
}