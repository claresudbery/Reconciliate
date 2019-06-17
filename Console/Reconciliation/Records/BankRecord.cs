using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class BankRecord : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; private set; }
        public static string EmptyType = "Empty Type";

        // Source data - loaded on startup (if any new essential fields are added, update EssentialFields value below):
        public DateTime Date { get; set; }
        public double UnreconciledAmount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double ReconciledAmount { get; set; }
        public int ChequeNumber { get; set; }

        public const string EssentialFields = "date, unreconciled amount, type or description";

        private char _separator = '^';
        private int _expectedNumberOfFieldsPerRow = 10;

        public const int DateIndex = 0;
        public const int UnreconciledAmountIndex = 1;
        public const int DividerIndex = 1;
        public const int MatchedIndex = 2;
        public const int TypeIndex = 3;
        public const int DescriptionIndex = 4;
        public const int ChequeNumberIndex = 5;
        public const int ReconciledAmountIndex = 6;
        public const int MatchOffset = 11;

        public void CreateFromMatch(DateTime date, double amount, string type, string description, int extraInfo, ICSVRecord matchedRecord)
        {
            Match = matchedRecord;
            Matched = true;

            Date = date;
            UnreconciledAmount = amount;
            Type = type;
            Description = description;
            ChequeNumber = extraInfo;
        }

        public void Load(string csvLine, char? overrideSeparator = null)
        {
            char separator = overrideSeparator ?? _separator;
            SourceLine = csvLine;
            string[] values = SplitValuesBasedOnSeparatorAndHandleCommasInAmounts(csvLine, separator);
            values = StringHelper.MakeSureThereAreAtLeastEnoughStringValues(_expectedNumberOfFieldsPerRow, values);

            Date = values[DateIndex] != "" && values[DateIndex].IsNumeric()
                ? Convert.ToDateTime(values[DateIndex], StringHelper.Culture())
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            var simple_unrec_amount = string.IsNullOrEmpty(values[UnreconciledAmountIndex])
                ? string.Empty
                : values[UnreconciledAmountIndex].TrimAmount();
            UnreconciledAmount = Convert.ToDouble(simple_unrec_amount != "" && simple_unrec_amount.IsNumeric()
                ? simple_unrec_amount 
                : "0");

            Type = values[TypeIndex];
            Description = values[DescriptionIndex];

            ChequeNumber = Convert.ToInt32(values[ChequeNumberIndex] != "" && values[ChequeNumberIndex].IsNumeric()
                ? values[ChequeNumberIndex] 
                : "0");

            var simple_rec_amount = string.IsNullOrEmpty(values[ReconciledAmountIndex])
                ? String.Empty
                : values[ReconciledAmountIndex].TrimAmount();
            ReconciledAmount = Convert.ToDouble(simple_rec_amount != "" && simple_rec_amount.IsNumeric()
                ? simple_rec_amount 
                : "0");

            CheckForEmptyFields();
        }

        private void CheckForEmptyFields()
        {
            if (string.IsNullOrEmpty(Description))
            {
                if (!string.IsNullOrEmpty(Type))
                {
                    Description = Type;
                    Type = EmptyType;
                }
            }

            if (WeHaveAnEmptyEssentialField())
            {
                throw new Exception(String.Format("One of your essential owned (non-third-party) bank record fields is empty ({0}).", EssentialFields));
            }
        }

        private bool WeHaveAnEmptyEssentialField()
        {
            return Date == Convert.ToDateTime("9/9/9999", StringHelper.Culture())
                   || UnreconciledAmount == 0
                   || string.IsNullOrEmpty(Type)
                   || string.IsNullOrEmpty(Description);
        }

        private string[] SplitValuesBasedOnSeparatorAndHandleCommasInAmounts(string csvLine, char separator)
        {
            var values = csvLine.Split(separator);

            if (values.Length > _expectedNumberOfFieldsPerRow)
            {
                // This happens when the amount contains a comma.
                if (!string.IsNullOrEmpty(values[UnreconciledAmountIndex]))
                {
                    values = RectifyCommaProblem(values, UnreconciledAmountIndex);
                }
                else if (!string.IsNullOrEmpty(values[ReconciledAmountIndex]))
                {
                    values = RectifyCommaProblem(values, ReconciledAmountIndex);
                }
            }

            return values;
        }

        private string[] RectifyCommaProblem(string[] values, int targetIndex)
        {
            values[targetIndex] = values[targetIndex] + values[targetIndex + 1];
            for (int index = targetIndex + 1; index < values.Length - 1; index++)
            {
                values[index] = values[index + 1];
            }
            values[values.Length - 1] = String.Empty;

            return values;
        }

        public string ToCsv(bool formatCurrency = true)
        {
            return ToString(',', true, formatCurrency);
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
                cellSet.PopulateCell(rowNumber, TypeIndex + 1, Type);
                cellSet.PopulateCell(rowNumber, DescriptionIndex + 1, Description);
                PopulateCellWithAmount(cellSet, rowNumber, ChequeNumberIndex, ChequeNumber);
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
            UnreconciledAmount = cellSet.ReadCell(UnreconciledAmountIndex) != null
                ? (Double)cellSet.ReadCell(UnreconciledAmountIndex)
                : 0;
            Type = (String)cellSet.ReadCell(TypeIndex);
            Description = (String)cellSet.ReadCell(DescriptionIndex);
            ChequeNumber = cellSet.Count > ChequeNumberIndex && cellSet.ReadCell(ChequeNumberIndex) != null 
                ? Convert.ToInt32((Double)cellSet.ReadCell(ChequeNumberIndex)) 
                : 0;
            ReconciledAmount = cellSet.Count > ReconciledAmountIndex && cellSet.ReadCell(ReconciledAmountIndex) != null 
                ? (Double)cellSet.ReadCell(ReconciledAmountIndex)
                : 0;

            SourceLine = ToString(_separator, false);
        }

        private String ToString(char separator, bool encaseDescriptionInQuotes = true, bool formatCurrency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator
                + (UnreconciledAmount == 0 ? "" : UnreconciledAmount.ToCsvString(formatCurrency)) + separator
                + (Matched ? "x" : "") + separator
                + Type + separator
                + (encaseDescriptionInQuotes ? Description.EncaseInEscapedQuotesIfNotAlreadyEncased() : Description) + separator
                + (ChequeNumber == 0 ? "" : ChequeNumber.ToString()) + separator
                + (ReconciledAmount == 0 ? "" : ReconciledAmount.ToCsvString(formatCurrency)) + separator + separator + separator
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
            return Type;
        }

        public int ExtraInfo()
        {
            return ChequeNumber;
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
            return new BankRecord
            {
                Date = Date,
                UnreconciledAmount = UnreconciledAmount,
                Type = Type,
                Description = Description,
                ReconciledAmount = ReconciledAmount,
                ChequeNumber = ChequeNumber,
                SourceLine = SourceLine
            };
        }

        public void UpdateSourceLineForOutput(char outputSeparator)
        {
            SourceLine = ToString(outputSeparator);
        }
    }
}