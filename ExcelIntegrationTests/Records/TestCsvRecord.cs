using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;
using Interfaces.DTOs;

namespace ExcelIntegrationTests.Records
{
    class TestCsvRecord : ICSVRecord
    {
        public bool Matched { get; set; }
        public bool Divider { get; set; }

        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public Int16 Index { get; set; }

        public ICSVRecord Match { get; set; }
        public string SourceLine { get; set; }

        public TestCsvRecord Build()
        {
            Date = DateTime.Today;
            Description = "Test record";
            Index = 1234;
            Amount = 12.34;
            
            return this;
        }

        public ICSVRecord Copy()
        {
            throw new NotImplementedException();
        }

        ICSVRecord ICSVRecord.WithDate(DateTime newDate)
        {
            return WithDate(newDate);
        }

        public void UpdateSourceLineForOutput(char outputSeparator)
        {
            SourceLine = ToCsv();
        }

        public TestCsvRecord WithDate(DateTime newDate)
        {
            Date = newDate;
            return this;
        }

        public TestCsvRecord WithAmount(double newAmount)
        {
            Amount = newAmount;
            return this;
        }

        public TestCsvRecord WithDescription(String newDescription)
        {
            Description = newDescription;
            return this;
        }

        public TestCsvRecord WithIndex(Int16 newIndex)
        {
            Index = newIndex;
            return this;
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

        public void PopulateSpreadsheetRow(ICellSet cells, int rowNumber)
        {
            cells.PopulateCell(rowNumber, 1, Date);
            cells.PopulateCell(rowNumber, 2, Amount);
            cells.PopulateCell(rowNumber, 3, Description);
            cells.PopulateCell(rowNumber, 4, Index);
        }

        public void ReadFromSpreadsheetRow(ICellRow cells)
        {
            Date = DateTime.FromOADate((double)cells.ReadCell(0));
            Amount = (Double)cells.ReadCell(1);
            Description = (String)cells.ReadCell(2);
            Index = Convert.ToInt16((Double)cells.ReadCell(3));
        }

        public string ToCsv(bool formatCurrency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + ","
                   + (Amount == 0 ? "" : Amount.ToCsvString(formatCurrency)) + ","
                   + Description.EncaseInEscapedQuotesIfNotAlreadyEncased() + ","
                   + (Index == 0 ? "" : Index.ToString());
        }

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
            throw new NotImplementedException();
        }

        public void MakeMainAmountPositive()
        {
            throw new NotImplementedException();
        }

        public void SwapSignOfMainAmount()
        {
        }

        public void Reconcile()
        {
            throw new NotImplementedException();
        }

        public double MainAmount()
        {
            throw new NotImplementedException();
        }

        public void ChangeMainAmount(double newValue)
        {
            throw new NotImplementedException();
        }

        public string TransactionType()
        {
            throw new NotImplementedException();
        }

        public int ExtraInfo()
        {
            throw new NotImplementedException();
        }

        public void ConvertSourceLineSeparators(char originalSeparator, char newSeparator)
        {
            throw new NotImplementedException();
        }
    }
}
