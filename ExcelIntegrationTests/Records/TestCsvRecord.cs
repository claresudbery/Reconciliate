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
        public string Source_line { get; set; }

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

        ICSVRecord ICSVRecord.With_date(DateTime newDate)
        {
            return With_date(newDate);
        }

        public void Update_source_line_for_output(char outputSeparator)
        {
            Source_line = To_csv();
        }

        public TestCsvRecord With_date(DateTime newDate)
        {
            Date = newDate;
            return this;
        }

        public TestCsvRecord With_amount(double newAmount)
        {
            Amount = newAmount;
            return this;
        }

        public TestCsvRecord With_description(String newDescription)
        {
            Description = newDescription;
            return this;
        }

        public TestCsvRecord With_index(Int16 newIndex)
        {
            Index = newIndex;
            return this;
        }

        public ConsoleLine To_console(int index = -1)
        {
            return new ConsoleLine
            {
                Index = index,
                Date_string = Date.ToString(@"dd\/MM\/yyyy"),
                Amount_string = Amount.To_csv_string(true),
                Description_string = Description
            };
        }

        public void Populate_spreadsheet_row(ICellSet cells, int rowNumber)
        {
            cells.Populate_cell(rowNumber, 1, Date);
            cells.Populate_cell(rowNumber, 2, Amount);
            cells.Populate_cell(rowNumber, 3, Description);
            cells.Populate_cell(rowNumber, 4, Index);
        }

        public void Read_from_spreadsheet_row(ICellRow cells)
        {
            Date = DateTime.FromOADate((double)cells.Read_cell(0));
            Amount = (Double)cells.Read_cell(1);
            Description = (String)cells.Read_cell(2);
            Index = Convert.ToInt16((Double)cells.Read_cell(3));
        }

        public string To_csv(bool formatCurrency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + ","
                   + (Amount == 0 ? "" : Amount.To_csv_string(formatCurrency)) + ","
                   + Description.Encase_in_escaped_quotes_if_not_already_encased() + ","
                   + (Index == 0 ? "" : Index.ToString());
        }

        public void Create_from_match(DateTime date, double amount, string type, string description, int extraInfo,
            ICSVRecord matchedRecord)
        {
            throw new NotImplementedException();
        }

        public void Load(string csvLine, char? overrideSeparator = null)
        {
            throw new NotImplementedException();
        }

        public bool Main_amount_is_negative()
        {
            throw new NotImplementedException();
        }

        public void Make_main_amount_positive()
        {
            throw new NotImplementedException();
        }

        public void Swap_sign_of_main_amount()
        {
        }

        public void Reconcile()
        {
            throw new NotImplementedException();
        }

        public double Main_amount()
        {
            throw new NotImplementedException();
        }

        public void Change_main_amount(double newValue)
        {
            throw new NotImplementedException();
        }

        public string Transaction_type()
        {
            throw new NotImplementedException();
        }

        public int Extra_info()
        {
            throw new NotImplementedException();
        }

        public void Convert_source_line_separators(char originalSeparator, char newSeparator)
        {
            throw new NotImplementedException();
        }
    }
}
