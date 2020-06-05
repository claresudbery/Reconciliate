using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class CredCard2InOutRecord : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string Source_line { get; set; }

        public DateTime Date { get; set; }
        public double Unreconciled_amount { get; set; }
        public string Description { get; set; }
        public double Reconciled_amount { get; set; }

        private char _separator = '^';
        private int _expected_number_of_fields_per_row = 5;

        public const int DateIndex = 0;
        public const int UnreconciledAmountIndex = 1;
        public const int DividerIndex = 1;
        public const int MatchedIndex = 2;
        public const int DescriptionIndex = 3;
        public const int ReconciledAmountIndex = 4;
        public const int MatchOffset = 7;

        public void Create_from_match(DateTime date, double amount, string type, string description, int extra_info, ICSVRecord matched_record)
        {
            Match = matched_record;
            Matched = true;

            Date = date;
            Unreconciled_amount = amount;
            Description = description;
        }

        public void Load(string csv_line, char? override_separator = null)
        {
            var separator = override_separator ?? _separator;
            Source_line = csv_line;
            var values = csv_line.Split(separator);
            values = StringHelper.Make_sure_there_are_at_least_enough_string_values(_expected_number_of_fields_per_row, values);

            Date = values[DateIndex] != "" && values[DateIndex].Is_numeric()
                ? Convert.ToDateTime(values[DateIndex], StringHelper.Culture()) 
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            var simple_unrec_amount = string.IsNullOrEmpty(values[UnreconciledAmountIndex])
                ? String.Empty
                : values[UnreconciledAmountIndex].Trim_amount();
            Unreconciled_amount = Convert.ToDouble(simple_unrec_amount != "" && simple_unrec_amount.Is_numeric()
                ? simple_unrec_amount 
                : "0");

            Description = values[DescriptionIndex] != String.Empty ? values[DescriptionIndex].Strip_enclosing_quotes() : "Source record had no description";

            var simple_rec_amount = string.IsNullOrEmpty(values[ReconciledAmountIndex])
                ? String.Empty
                : values[ReconciledAmountIndex].Trim_amount();
            Reconciled_amount = Convert.ToDouble(simple_rec_amount != "" && simple_rec_amount.Is_numeric()
                ? simple_rec_amount 
                : "0");
        }

        public string To_csv(bool format_currency = true)
        {
            return To_string(',', format_currency);
        }

        public void Populate_spreadsheet_row(ICellSet cell_set, int row_number)
        {
            if (Divider)
            {
                cell_set.Populate_cell(row_number, DividerIndex + 1, ReconConsts.DividerText);
            }
            else
            {
                cell_set.Populate_cell(row_number, DateIndex + 1, Date);
                Populate_cell_with_amount(cell_set, row_number, UnreconciledAmountIndex, Unreconciled_amount);
                cell_set.Populate_cell(row_number, MatchedIndex + 1, Matched ? "x" : String.Empty);
                cell_set.Populate_cell(row_number, DescriptionIndex + 1, Description);
                Populate_cell_with_amount(cell_set, row_number, ReconciledAmountIndex, Reconciled_amount);
                if (Match != null)
                {
                    Match.Populate_spreadsheet_row(cell_set, row_number);
                }
            }
        }

        private void Populate_cell_with_amount(ICellSet cell_set, int row_number, int amount_index, double amount)
        {
            if (amount != 0)
            {
                cell_set.Populate_cell(row_number, amount_index + 1, amount);
            }
            else
            {
                cell_set.Populate_cell(row_number, amount_index + 1, String.Empty);
            }
        }

        public void Read_from_spreadsheet_row(ICellRow cell_set)
        {
            Date = DateTime.FromOADate((double)cell_set.Read_cell(DateIndex));
            Unreconciled_amount = cell_set.Read_cell(UnreconciledAmountIndex) != null 
                ? (Double)cell_set.Read_cell(UnreconciledAmountIndex) 
                : 0;
            Description = ((String)cell_set.Read_cell(DescriptionIndex)).Strip_enclosing_quotes();
            Reconciled_amount = cell_set.Count > ReconciledAmountIndex && cell_set.Read_cell(ReconciledAmountIndex) != null 
                ? (Double)cell_set.Read_cell(ReconciledAmountIndex)
                : 0;

            Source_line = To_string(_separator, false);
        }

        private String To_string(char separator, bool encase_description_in_quotes = true, bool format_currency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator.ToString()
                   + (Unreconciled_amount == 0 ? "" : Unreconciled_amount.To_csv_string(format_currency)) +
                   separator.ToString()
                   + (Matched ? "x" : "") + separator.ToString()
                   + (encase_description_in_quotes ? Description.Encase_in_escaped_quotes_if_not_already_encased() : Description) + separator.ToString()
                   + (Reconciled_amount == 0 ? "" : Reconciled_amount.To_csv_string(format_currency)) + separator.ToString()
                   + (Match != null ? separator.ToString() + separator.ToString() + Match.To_csv(format_currency) : "");
        }

        public ConsoleLine To_console(int index = -1)
        {
            return new ConsoleLine
            {
                Index = index,
                Date_string = Date.ToString(@"dd\/MM\/yyyy"),
                Amount_string = Unreconciled_amount.To_csv_string(true),
                Description_string = Description
            };
        }

        public bool Main_amount_is_negative()
        {
            return Unreconciled_amount != 0 && Unreconciled_amount < 0;
        }

        public void Make_main_amount_positive()
        {
            if (Unreconciled_amount < 0)
            {
                Unreconciled_amount = Unreconciled_amount * -1;
            }
        }

        public void Swap_sign_of_main_amount()
        {
            Unreconciled_amount = Unreconciled_amount * -1;
        }

        public double Main_amount()
        {
            return Unreconciled_amount;
        }

        public void Change_main_amount(double new_value)
        {
            Unreconciled_amount = new_value;
        }

        public string Transaction_type()
        {
            return "";
        }

        public int Extra_info()
        {
            return 0;
        }

        public void Reconcile()
        {
            Reconciled_amount = Unreconciled_amount;
            Unreconciled_amount = 0;
        }

        public void Convert_source_line_separators(char original_separator, char new_separator)
        {
            Source_line = Source_line.Convert_separators(original_separator, new_separator);
        }

        public ICSVRecord With_date(DateTime new_date)
        {
            Date = new_date;
            return this;
        }

        public ICSVRecord Copy()
        {
            return new CredCard2InOutRecord
            {
                Date = Date,
                Unreconciled_amount = Unreconciled_amount,
                Description = Description,
                Reconciled_amount = Reconciled_amount,
                Source_line = Source_line
            };
        }

        public void Update_source_line_for_output(char output_separator)
        {
            Source_line = To_string(output_separator);
        }
    }
}