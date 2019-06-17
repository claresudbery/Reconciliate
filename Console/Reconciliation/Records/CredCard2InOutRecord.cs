using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class CredCard2InOutRecord : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; set; }

        public DateTime Date { get; set; }
        public double UnreconciledAmount { get; set; }
        public string Description { get; set; }
        public double ReconciledAmount { get; set; }

        private char _separator = '^';
        private int _expectedNumberOfFieldsPerRow = 5;

        public const int DateIndex = 0;
        public const int UnreconciledAmountIndex = 1;
        public const int DividerIndex = 1;
        public const int MatchedIndex = 2;
        public const int DescriptionIndex = 3;
        public const int ReconciledAmountIndex = 4;
        public const int MatchOffset = 7;

        public void CreateFromMatch(DateTime date, double amount, string type, string description, int extraInfo, ICSVRecord matchedRecord)
        {
            Match = matchedRecord;
            Matched = true;

            Date = date;
            UnreconciledAmount = amount;
            Description = description;
        }

        public void Load(string csvLine, char? overrideSeparator = null)
        {
            var separator = overrideSeparator ?? _separator;
            SourceLine = csvLine;
            var values = csvLine.Split(separator);
            values = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(_expectedNumberOfFieldsPerRow, values);

            Date = values[DateIndex] != "" && values[DateIndex].IsNumeric()
                ? Convert.ToDateTime(values[DateIndex], StringHelper.Culture()) 
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            var simple_unrec_amount = string.IsNullOrEmpty(values[UnreconciledAmountIndex])
                ? String.Empty
                : values[UnreconciledAmountIndex].TrimAmount();
            UnreconciledAmount = Convert.ToDouble(simple_unrec_amount != "" && simple_unrec_amount.IsNumeric()
                ? simple_unrec_amount 
                : "0");

            Description = values[DescriptionIndex] != String.Empty ? values[DescriptionIndex] : "Source record had no description";

            var simple_rec_amount = string.IsNullOrEmpty(values[ReconciledAmountIndex])
                ? String.Empty
                : values[ReconciledAmountIndex].TrimAmount();
            ReconciledAmount = Convert.ToDouble(simple_rec_amount != "" && simple_rec_amount.IsNumeric()
                ? simple_rec_amount 
                : "0");
        }

        public string ToCsv(bool formatCurrency = true)
        {
            return ToString(',', formatCurrency);
        }

        public void PopulateSpreadsheetRow(ICellSet cellSet, int rowNumber)
        {
            if (Divider)
            {
                cellSet.PopulateCell(rowNumber, DividerIndex + 1, ReconConsts.DividerText);
            }
            else
            {
                cellSet.PopulateCell(rowNumber, DateIndex + 1, Date);
                PopulateCellWithAmount(cellSet, rowNumber, UnreconciledAmountIndex, UnreconciledAmount);
                cellSet.PopulateCell(rowNumber, MatchedIndex + 1, Matched ? "x" : String.Empty);
                cellSet.PopulateCell(rowNumber, DescriptionIndex + 1, Description);
                PopulateCellWithAmount(cellSet, rowNumber, ReconciledAmountIndex, ReconciledAmount);
                if (Match != null)
                {
                    Match.PopulateSpreadsheetRow(cellSet, rowNumber);
                }
            }
        }

        private void PopulateCellWithAmount(ICellSet cellSet, int rowNumber, int amountIndex, double amount)
        {
            if (amount != 0)
            {
                cellSet.PopulateCell(rowNumber, amountIndex + 1, amount);
            }
            else
            {
                cellSet.PopulateCell(rowNumber, amountIndex + 1, String.Empty);
            }
        }

        public void ReadFromSpreadsheetRow(ICellRow cellSet)
        {
            Date = DateTime.FromOADate((double)cellSet.ReadCell(DateIndex));
            UnreconciledAmount = cellSet.ReadCell(UnreconciledAmountIndex) != null ? (Double)cellSet.ReadCell(UnreconciledAmountIndex) : 0;
            Description = (String)cellSet.ReadCell(DescriptionIndex);
            ReconciledAmount = cellSet.Count > ReconciledAmountIndex && cellSet.ReadCell(ReconciledAmountIndex) != null 
                ? (Double)cellSet.ReadCell(ReconciledAmountIndex)
                : 0;

            SourceLine = ToString(_separator, false);
        }

        private String ToString(char separator, bool encaseDescriptionInQuotes = true, bool formatCurrency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator.ToString()
                   + (UnreconciledAmount == 0 ? "" : UnreconciledAmount.ToCsvString(formatCurrency)) +
                   separator.ToString()
                   + (Matched ? "x" : "") + separator.ToString()
                   + (encaseDescriptionInQuotes ? Description.EncaseInEscapedQuotesIfNotAlreadyEncased() : Description) + separator.ToString()
                   + (ReconciledAmount == 0 ? "" : ReconciledAmount.ToCsvString(formatCurrency)) + separator.ToString()
                   + (Match != null ? separator.ToString() + separator.ToString() + Match.ToCsv(formatCurrency) : "");
        }

        public ConsoleLine ToConsole(int index = -1)
        {
            return new ConsoleLine
            {
                Index = index,
                DateString = Date.ToString(@"dd\/MM\/yyyy"),
                AmountString = UnreconciledAmount.ToCsvString(true),
                DescriptionString = Description
            };
        }

        public bool MainAmountIsNegative()
        {
            return UnreconciledAmount != 0 && UnreconciledAmount < 0;
        }

        public void MakeMainAmountPositive()
        {
            if (UnreconciledAmount < 0)
            {
                UnreconciledAmount = UnreconciledAmount * -1;
            }
        }

        public void SwapSignOfMainAmount()
        {
            UnreconciledAmount = UnreconciledAmount * -1;
        }

        public double MainAmount()
        {
            return UnreconciledAmount;
        }

        public void ChangeMainAmount(double newValue)
        {
            UnreconciledAmount = newValue;
        }

        public string TransactionType()
        {
            return "";
        }

        public int ExtraInfo()
        {
            return 0;
        }

        public void Reconcile()
        {
            ReconciledAmount = UnreconciledAmount;
            UnreconciledAmount = 0;
        }

        public void ConvertSourceLineSeparators(char originalSeparator, char newSeparator)
        {
            SourceLine = SourceLine.ConvertSeparators(originalSeparator, newSeparator);
        }

        public ICSVRecord WithDate(DateTime newDate)
        {
            Date = newDate;
            return this;
        }

        public ICSVRecord Copy()
        {
            return new CredCard2InOutRecord
            {
                Date = Date,
                UnreconciledAmount = UnreconciledAmount,
                Description = Description,
                ReconciledAmount = ReconciledAmount,
                SourceLine = SourceLine
            };
        }

        public void UpdateSourceLineForOutput(char outputSeparator)
        {
            SourceLine = ToString(outputSeparator);
        }
    }
}