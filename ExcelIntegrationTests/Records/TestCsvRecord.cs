using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;
using Interfaces.DTOs;
using Interfaces.Extensions;

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
        public string OutputSourceLine { get; set; }

        public TestCsvRecord Build()
        {
            Date = DateTime.Today;
            Description = "Test record";
            Index = 1234;
            Amount = 12.34;
            
            return this;
        }

        public void Copy_from(ICSVRecord source)
        {
            throw new NotImplementedException();
        }

        public ICSVRecord Copy()
        {
            throw new NotImplementedException();
        }

        ICSVRecord ICSVRecord.With_date(DateTime new_date)
        {
            return With_date(new_date);
        }

        public void Update_source_line_for_output(char output_separator)
        {
            OutputSourceLine = To_csv();
        }

        public TestCsvRecord With_date(DateTime new_date)
        {
            Date = new_date;
            return this;
        }

        public TestCsvRecord With_amount(double new_amount)
        {
            Amount = new_amount;
            return this;
        }

        public TestCsvRecord With_description(String new_description)
        {
            Description = new_description;
            return this;
        }

        public TestCsvRecord With_index(Int16 new_index)
        {
            Index = new_index;
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

        public void Populate_spreadsheet_row(ICellSet cells, int row_number)
        {
            cells.Populate_cell(row_number, 1, Date);
            cells.Populate_cell(row_number, 2, Amount);
            cells.Populate_cell(row_number, 3, Description);
            cells.Populate_cell(row_number, 4, Index);
        }

        public void Read_from_spreadsheet_row(ICellRow cells)
        {
            Date = DateTime.FromOADate((double)cells.Read_cell(0));
            Amount = (Double)cells.Read_cell(1);
            Description = ((String)cells.Read_cell(2)).Strip_enclosing_quotes();
            Index = Convert.ToInt16((Double)cells.Read_cell(3));
        }

        public string To_csv(bool format_currency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + ","
                   + (Amount == 0 ? "" : Amount.To_csv_string(format_currency)) + ","
                   + Description.Encase_in_escaped_quotes_if_not_already_encased() + ","
                   + (Index == 0 ? "" : Index.ToString());
        }

        public void Create_from_match(DateTime date, double amount, string type, string description, int extra_info,
            ICSVRecord matched_record)
        {
            throw new NotImplementedException();
        }

        public void Load(string csv_line, char? override_separator = null)
        {
            throw new NotImplementedException();
        }

        public void Load_from_original_line()
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

        public void Change_main_amount(double new_value)
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

        public void Convert_source_line_separators(char original_separator, char new_separator)
        {
            throw new NotImplementedException();
        }
    }
}
