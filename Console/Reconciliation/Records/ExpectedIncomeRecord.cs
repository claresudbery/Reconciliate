using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class ExpectedIncomeRecord : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; private set; }

        // Source data - loaded on startup (if any new essential fields are added, update EssentialFields value below):
        public DateTime Date { get; set; }
        public double UnreconciledAmount { get; set; }
        public string Code { get; set; }
        public double ReconciledAmount { get; set; }
        public DateTime DatePaid { get; set; }
        public double TotalPaid { get; set; }
        public string Description { get; set; }

        private char _separator = ',';

        public const int DateIndex = 0;
        public const int UnreconciledAmountIndex = 1;
        public const int DividerIndex = 1;
        public const int CodeIndex = 2;
        public const int ReconciledAmountIndex = 3;
        public const int DatePaidIndex = 4;
        public const int TotalPaidIndex = 5;
        public const int DescriptionIndex = 6;

        public const string EssentialFields = "unreconciled amount, code or description";

        public void CreateFromMatch(DateTime date, double amount, string type, string description, int extraInfo,
            ICSVRecord matchedRecord)
        {
            throw new NotImplementedException();
        }

        public void Load(string csvLine, char? overrideSeparator = null)
        {
            throw new NotImplementedException();
        }

        public bool MainAmountIsNegative()
        {
            return UnreconciledAmount < 0;
        }

        public void MakeMainAmountPositive()
        {
            UnreconciledAmount = Math.Abs(UnreconciledAmount);
        }

        public void SwapSignOfMainAmount()
        {
            UnreconciledAmount = UnreconciledAmount * -1;
        }

        public void Reconcile()
        {
            ReconciledAmount = UnreconciledAmount;
            UnreconciledAmount = 0;
        }

        public string ToCsv(bool formatCurrency = true)
        {
            return ToString(',', true, formatCurrency);
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
                cellSet.PopulateCell(rowNumber, CodeIndex + 1, Code);
                PopulateCellWithAmount(cellSet, rowNumber, ReconciledAmountIndex, ReconciledAmount);
                cellSet.PopulateCell(rowNumber, DatePaidIndex + 1, DatePaid);
                PopulateCellWithAmount(cellSet, rowNumber, TotalPaidIndex, TotalPaid);
                cellSet.PopulateCell(rowNumber, DescriptionIndex + 1, Description);
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

        public void ReadFromSpreadsheetRow(ICellRow cellRow)
        {
            Date = DateTime.FromOADate(cellRow.ReadCell(DateIndex) != null 
                ? (double)cellRow.ReadCell(DateIndex)
                : 0);
            UnreconciledAmount = cellRow.Count > UnreconciledAmountIndex && cellRow.ReadCell(UnreconciledAmountIndex) != null
                ? (Double)cellRow.ReadCell(UnreconciledAmountIndex)
                : 0;
            Code = (String)cellRow.ReadCell(CodeIndex);
            ReconciledAmount = cellRow.Count > ReconciledAmountIndex && cellRow.ReadCell(ReconciledAmountIndex) != null
                ? (Double)cellRow.ReadCell(ReconciledAmountIndex)
                : 0;
            DatePaid = DateTime.FromOADate(cellRow.ReadCell(DatePaidIndex) != null
                ? (double)cellRow.ReadCell(DatePaidIndex)
                : 0);
            TotalPaid = cellRow.Count > TotalPaidIndex && cellRow.ReadCell(TotalPaidIndex) != null
                ? (Double)cellRow.ReadCell(TotalPaidIndex)
                : 0;
            Description = (String)cellRow.ReadCell(DescriptionIndex);

            SourceLine = ToString(_separator, false);
        }

        private String ToString(char separator, bool encaseDescriptionInQuotes = true, bool formatCurrency = true)
        {
            return (Date.ToOADate() == 0 ? "" : Date.ToString(@"dd\/MM\/yyyy")) + separator
                   + (UnreconciledAmount == 0 ? "" : UnreconciledAmount.ToCsvString(formatCurrency)) + separator
                   + Code + separator
                   + (ReconciledAmount == 0 ? "" : ReconciledAmount.ToCsvString(formatCurrency)) + separator
                   + (DatePaid.ToOADate() == 0 ? "" : DatePaid.ToString(@"dd\/MM\/yyyy")) + separator
                   + (TotalPaid == 0 ? "" : TotalPaid.ToCsvString(formatCurrency)) + separator
                   + (string.IsNullOrEmpty(Description)
                        ? ""
                        : (encaseDescriptionInQuotes ? Description.EncaseInEscapedQuotesIfNotAlreadyEncased() : Description));
        }

        public void ConvertSourceLineSeparators(char originalSeparator, char newSeparator)
        {
            throw new NotImplementedException();
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
            return Code;
        }

        public int ExtraInfo()
        {
            return 0;
        }

        public ICSVRecord Copy()
        {
            return new ExpectedIncomeRecord
            {
                Date = Date,
                UnreconciledAmount = UnreconciledAmount,
                Code = Code,
                ReconciledAmount = ReconciledAmount,
                DatePaid = DatePaid,
                TotalPaid = TotalPaid,
                Description = Description,
                SourceLine = SourceLine
            };
        }

        public ICSVRecord WithDate(DateTime newDate)
        {
            Date = newDate;
            return this;
        }

        public void UpdateSourceLineForOutput(char outputSeparator)
        {
            SourceLine = ToString(outputSeparator);
        }

        public BankRecord ConvertToBankRecord()
        {
            return new BankRecord
            {
                Date = Date,
                UnreconciledAmount = UnreconciledAmount,
                Type = Code,
                Description = Description
            };
        }
    }
}