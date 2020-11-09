using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class CredCard2Record : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; set; }
        public string OutputSourceLine { get; private set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }

        private char _default_separator = ',';
        private int _expected_number_of_fields_per_row = 5;

        public const int DateIndex = 0;
        public const int AmountIndex = 4;
        public const int DescriptionIndex = 1;

        public const int DateSpreadsheetIndex = CredCard2InOutRecord.MatchOffset + 0;
        public const int AmountSpreadsheetIndex = CredCard2InOutRecord.MatchOffset + 1;
        public const int DescriptionSpreadsheetIndex = CredCard2InOutRecord.MatchOffset + 2;

        public void Create_from_match(DateTime date, double amount, string type, string description, int extra_info, ICSVRecord matched_record)
        {
            Match = matched_record;
            Matched = true;

            Date = date;
            Amount = amount;
            Description = description;
        }

        public void Load(string csv_line, char? override_separator = null)
        {
            csv_line = csv_line.Replace_commas_surrounded_by_spaces();
            SourceLine = csv_line;
            OutputSourceLine = csv_line;
            Load_from_original_line();
        }

        public void Load_from_original_line()
        {
            var values = SourceLine.Split(_default_separator);
            values = StringHelper.Make_sure_there_are_at_least_enough_string_values(_expected_number_of_fields_per_row, values);

            Date = values[DateIndex] != "" && values[DateIndex].Is_numeric()
                ? DateTime.ParseExact(values[DateIndex], "dd/MM/yyyy", CultureInfo.InvariantCulture)
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            string simple_amount = values[AmountIndex].TrimStart(new char[]{'"', ' '}).TrimEnd('"');
            Amount = Convert.ToDouble(simple_amount != "" && simple_amount.Is_numeric()
                ? simple_amount
                : "0");

            Description = values[DescriptionIndex].Strip_enclosing_quotes();
        }

        // Not currently used because the format of the file changed and they don't seem to use commas any more - and it's not the last field any more.
        private string Get_amount_with_possible_comma(string[] values)
        {
            if (values.Length == _expected_number_of_fields_per_row + 1)
            {
                // This happens when the amount contains a comma.
                values[3] = values[3] + values[4];
            }

            return string.IsNullOrEmpty(values[3])
                ? String.Empty
                : values[3].Trim_amount(); 
        }

        public string To_csv(bool format_currency = true)
        {
            return To_string(',', format_currency);
        }

        public string To_string(char separator = ',', bool format_currency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator
                + Amount.To_csv_string(format_currency) + separator
                + Description.Encase_in_escaped_quotes_if_not_already_encased();
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
            cell_set.Populate_cell(row_number, DescriptionSpreadsheetIndex + 1, Description);
        }

        public void Read_from_spreadsheet_row(ICellRow cell_set)
        {
            Date = DateTime.FromOADate((double)cell_set.Read_cell(DateIndex));
            Amount = (Double)cell_set.Read_cell(AmountIndex);
            Description = ((String)cell_set.Read_cell(DescriptionIndex)).Strip_enclosing_quotes();
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
            return String.Empty;
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

        public void Copy_from(ICSVRecord source)
        {
            if (source.GetType() == typeof(CredCard2Record))
            {
                CredCard2Record typed_source = source as CredCard2Record;

                Date = typed_source.Date;
                Description = typed_source.Description;
                Amount = typed_source.Amount;
                SourceLine = typed_source.SourceLine;
                OutputSourceLine = typed_source.OutputSourceLine;

                Match = typed_source.Match;
                Matched = typed_source.Matched;
                Divider = typed_source.Divider;
            }
            else
            {
                throw (new Exception("Trying to copy record but it's not a CredCard2Record type."));
            }
        }

        // Remember you can use Copy_from if you're having trouble with generic types.
        public ICSVRecord Copy()
        {
            return new CredCard2Record
            {
                Date = Date,
                Description = Description,
                Amount = Amount,
                SourceLine = SourceLine,
                OutputSourceLine = OutputSourceLine,

                Match = Match,
                Matched = Matched,
                Divider = Divider
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