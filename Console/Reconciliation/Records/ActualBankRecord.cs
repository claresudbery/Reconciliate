using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class ActualBankRecord : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; private set; }

        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }

        private char _separator = ',';
        private int _expectedNumberOfFieldsPerRow = 5;

        public const int DateIndex = 0;
        public const int TypeIndex = 1;
        public const int DescriptionIndex = 2;
        public const int AmountIndex = 3;
        public const int BalanceIndex = 4;

        public const int DateSpreadsheetIndex = BankRecord.MatchOffset + 0;
        public const int AmountSpreadsheetIndex = BankRecord.MatchOffset + 1;
        public const int TypeSpreadsheetIndex = BankRecord.MatchOffset + 2;
        public const int DescriptionSpreadsheetIndex = BankRecord.MatchOffset + 3;

        public ActualBankRecord()
        {
            SourceLine = "";
        }

        public void CreateFromMatch(DateTime date, double amount, string type, string description, int extraInfo, ICSVRecord matchedRecord)
        {
            Match = matchedRecord;
            Matched = true;

            Date = date;
            Amount = amount;
            Type = type;
            Description = description;
        }

        public void Load(string csvLine, char? overrideSeparator = null)
        {
            csvLine = csvLine.ReplaceCommasSurroundedBySpaces();
            SourceLine = csvLine;
            var values = csvLine.Split(_separator);
            values = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(_expectedNumberOfFieldsPerRow, values);

            Date = values[DateIndex] != "" && values[DateIndex].IsNumeric()
                ? Convert.ToDateTime(values[DateIndex], StringHelper.Culture()) 
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            Type = values[TypeIndex];
            Description = values[DescriptionIndex];

            var simple_amount = string.IsNullOrEmpty(values[AmountIndex])
                ? String.Empty
                : values[AmountIndex].TrimAmount();
            Amount = Convert.ToDouble(simple_amount != "" && simple_amount.IsNumeric()
                ? simple_amount 
                : "0");

            var simple_balance = string.IsNullOrEmpty(values[BalanceIndex])
                ? String.Empty
                : values[BalanceIndex].TrimAmount();
            Balance = Convert.ToDouble(simple_balance != "" && simple_balance.IsNumeric()
                ? simple_balance
                : "0");
        }

        public string ToCsv(bool formatCurrency = true)
        {
            return ToString(',', formatCurrency);
        }

        public string ToString(char separator = ',', bool formatCurrency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator
                   + Amount.ToCsvString(formatCurrency) + separator
                   + Type + ","
                   + Description;
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
            cellSet.PopulateCell(rowNumber, TypeSpreadsheetIndex + 1, Type);
            cellSet.PopulateCell(rowNumber, DescriptionSpreadsheetIndex + 1, Description);
        }

        public void ReadFromSpreadsheetRow(ICellRow cellSet)
        {
            Date = DateTime.FromOADate((double)cellSet.ReadCell(DateIndex));
            Type = (String)cellSet.ReadCell(TypeIndex);
            Description = (String)cellSet.ReadCell(DescriptionIndex);
            Amount = (Double)cellSet.ReadCell(AmountIndex);
            Balance = (Double)cellSet.ReadCell(BalanceIndex);
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
            return Type;
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
            return new ActualBankRecord
            {
                Date = Date,
                Type = Type,
                Description = Description,
                Amount = Amount,
                Balance = Balance,
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