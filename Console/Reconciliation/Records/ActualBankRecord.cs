using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class ActualBankRecord : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; private set; }
        public string OutputSourceLine { get; private set; }

        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }

        private char _default_separator = ',';
        private int _expected_number_of_fields_per_row = 5;

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
            OutputSourceLine = "";
        }

        public void Create_from_match(DateTime date, double amount, string type, string description, int extra_info, ICSVRecord matched_record)
        {
            Match = matched_record;
            Matched = true;

            Date = date;
            Amount = amount;
            Type = type;
            Description = description;
        }

        public void Load(string csv_line, char? override_separator = null)
        {
            csv_line = csv_line.Replace_commas_surrounded_by_spaces();
            SourceLine = csv_line;
            OutputSourceLine = csv_line;
            Load_from_original_line(override_separator);
        }

        public void Load_from_original_line()
        {
            var csv_line = SourceLine;
            var values = csv_line.Split(_default_separator);
            values = StringHelper.Make_sure_there_are_at_least_enough_string_values(_expected_number_of_fields_per_row, values);

            Date = values[DateIndex] != "" && values[DateIndex].Is_numeric()
                ? Convert.ToDateTime(values[DateIndex], StringHelper.Culture()) 
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            Type = values[TypeIndex];
            Description = values[DescriptionIndex].Strip_enclosing_quotes();

            var simple_amount = string.IsNullOrEmpty(values[AmountIndex])
                ? String.Empty
                : values[AmountIndex].Trim_amount();
            Amount = Convert.ToDouble(simple_amount != "" && simple_amount.Is_numeric()
                ? simple_amount 
                : "0");

            var simple_balance = string.IsNullOrEmpty(values[BalanceIndex])
                ? String.Empty
                : values[BalanceIndex].Trim_amount();
            Balance = Convert.ToDouble(simple_balance != "" && simple_balance.Is_numeric()
                ? simple_balance
                : "0");
        }

        public string To_csv(bool format_currency = true)
        {
            return To_string(',', format_currency);
        }

        public string To_string(char separator = ',', bool format_currency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator
                   + Amount.To_csv_string(format_currency) + separator
                   + Type + ","
                   + Description;
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

        public void Populate_spreadsheet_row(ICellSet cell_set, int row_number)
        {
            cell_set.Populate_cell(row_number, DateSpreadsheetIndex + 1, Date);
            cell_set.Populate_cell(row_number, AmountSpreadsheetIndex + 1, Amount);
            cell_set.Populate_cell(row_number, TypeSpreadsheetIndex + 1, Type);
            cell_set.Populate_cell(row_number, DescriptionSpreadsheetIndex + 1, Description);
        }

        public void Read_from_spreadsheet_row(ICellRow cell_set)
        {
            Date = DateTime.FromOADate((double)cell_set.Read_cell(DateIndex));
            Type = (String)cell_set.Read_cell(TypeIndex);
            Description = ((String)cell_set.Read_cell(DescriptionIndex)).Strip_enclosing_quotes();
            Amount = (Double)cell_set.Read_cell(AmountIndex);
            Balance = (Double)cell_set.Read_cell(BalanceIndex);
        }

        public bool Main_amount_is_negative()
        {
            return Amount != 0 && Amount < 0;
        }

        public void Make_main_amount_positive()
        {
            if (Amount < 0)
            {
                Amount = Amount * -1;
            }
        }

        public void Swap_sign_of_main_amount()
        {
            Amount = Amount * -1;
        }

        public double Main_amount()
        {
            return Amount;
        }

        public void Change_main_amount(double new_value)
        {
            Amount = new_value;
        }

        public string Transaction_type()
        {
            return Type;
        }

        public int Extra_info()
        {
            return 0;
        }

        public ICSVRecord With_date(DateTime new_date)
        {
            Date = new_date;
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
                SourceLine = SourceLine,
                OutputSourceLine = OutputSourceLine
            };
        }

        public void Update_source_line_for_output(char output_separator)
        {
            OutputSourceLine = To_string(output_separator);
        }

        public void Reconcile()
        {
            // Do nothing.
        }

        public void Convert_source_line_separators(char original_separator, char new_separator)
        {
            OutputSourceLine = OutputSourceLine.Convert_separators(original_separator, new_separator);
        }
    }
}