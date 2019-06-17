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
        public string Source_line { get; private set; }
        public static string EmptyType = "Empty Type";

        // Source data - loaded on startup (if any new essential fields are added, update EssentialFields value below):
        public DateTime Date { get; set; }
        public double Unreconciled_amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double Reconciled_amount { get; set; }
        public int Cheque_number { get; set; }

        public const string EssentialFields = "date, unreconciled amount, type or description";

        private char _separator = '^';
        private int _expected_number_of_fields_per_row = 10;

        public const int DateIndex = 0;
        public const int UnreconciledAmountIndex = 1;
        public const int DividerIndex = 1;
        public const int MatchedIndex = 2;
        public const int TypeIndex = 3;
        public const int DescriptionIndex = 4;
        public const int ChequeNumberIndex = 5;
        public const int ReconciledAmountIndex = 6;
        public const int MatchOffset = 11;

        public void Create_from_match(DateTime date, double amount, string type, string description, int extraInfo, ICSVRecord matchedRecord)
        {
            Match = matchedRecord;
            Matched = true;

            Date = date;
            Unreconciled_amount = amount;
            Type = type;
            Description = description;
            Cheque_number = extraInfo;
        }

        public void Load(string csvLine, char? overrideSeparator = null)
        {
            char separator = overrideSeparator ?? _separator;
            Source_line = csvLine;
            string[] values = Split_values_based_on_separator_and_handle_commas_in_amounts(csvLine, separator);
            values = StringHelper.Make_sure_there_are_at_least_enough_string_values(_expected_number_of_fields_per_row, values);

            Date = values[DateIndex] != "" && values[DateIndex].Is_numeric()
                ? Convert.ToDateTime(values[DateIndex], StringHelper.Culture())
                : Convert.ToDateTime("9/9/9999", StringHelper.Culture());

            var simple_unrec_amount = string.IsNullOrEmpty(values[UnreconciledAmountIndex])
                ? string.Empty
                : values[UnreconciledAmountIndex].Trim_amount();
            Unreconciled_amount = Convert.ToDouble(simple_unrec_amount != "" && simple_unrec_amount.Is_numeric()
                ? simple_unrec_amount 
                : "0");

            Type = values[TypeIndex];
            Description = values[DescriptionIndex];

            Cheque_number = Convert.ToInt32(values[ChequeNumberIndex] != "" && values[ChequeNumberIndex].Is_numeric()
                ? values[ChequeNumberIndex] 
                : "0");

            var simple_rec_amount = string.IsNullOrEmpty(values[ReconciledAmountIndex])
                ? String.Empty
                : values[ReconciledAmountIndex].Trim_amount();
            Reconciled_amount = Convert.ToDouble(simple_rec_amount != "" && simple_rec_amount.Is_numeric()
                ? simple_rec_amount 
                : "0");

            Check_for_empty_fields();
        }

        private void Check_for_empty_fields()
        {
            if (string.IsNullOrEmpty(Description))
            {
                if (!string.IsNullOrEmpty(Type))
                {
                    Description = Type;
                    Type = EmptyType;
                }
            }

            if (We_have_an_empty_essential_field())
            {
                throw new Exception(String.Format("One of your essential owned (non-third-party) bank record fields is empty ({0}).", EssentialFields));
            }
        }

        private bool We_have_an_empty_essential_field()
        {
            return Date == Convert.ToDateTime("9/9/9999", StringHelper.Culture())
                   || Unreconciled_amount == 0
                   || string.IsNullOrEmpty(Type)
                   || string.IsNullOrEmpty(Description);
        }

        private string[] Split_values_based_on_separator_and_handle_commas_in_amounts(string csvLine, char separator)
        {
            var values = csvLine.Split(separator);

            if (values.Length > _expected_number_of_fields_per_row)
            {
                // This happens when the amount contains a comma.
                if (!string.IsNullOrEmpty(values[UnreconciledAmountIndex]))
                {
                    values = Rectify_comma_problem(values, UnreconciledAmountIndex);
                }
                else if (!string.IsNullOrEmpty(values[ReconciledAmountIndex]))
                {
                    values = Rectify_comma_problem(values, ReconciledAmountIndex);
                }
            }

            return values;
        }

        private string[] Rectify_comma_problem(string[] values, int targetIndex)
        {
            values[targetIndex] = values[targetIndex] + values[targetIndex + 1];
            for (int index = targetIndex + 1; index < values.Length - 1; index++)
            {
                values[index] = values[index + 1];
            }
            values[values.Length - 1] = String.Empty;

            return values;
        }

        public string To_csv(bool formatCurrency = true)
        {
            return To_string(',', true, formatCurrency);
        }

        public void Populate_spreadsheet_row(ICellSet cellSet, int rowNumber)
        {
            if (Divider)
            {
                cellSet.Populate_cell(rowNumber, DividerIndex + 1, ReconConsts.DividerText);
            }
            else
            {
                cellSet.Populate_cell(rowNumber, DateIndex + 1, Date);
                Populate_cell_with_amount(cellSet, rowNumber, UnreconciledAmountIndex, Unreconciled_amount);
                cellSet.Populate_cell(rowNumber, MatchedIndex + 1, Matched ? "x" : String.Empty);
                cellSet.Populate_cell(rowNumber, TypeIndex + 1, Type);
                cellSet.Populate_cell(rowNumber, DescriptionIndex + 1, Description);
                Populate_cell_with_amount(cellSet, rowNumber, ChequeNumberIndex, Cheque_number);
                Populate_cell_with_amount(cellSet, rowNumber, ReconciledAmountIndex, Reconciled_amount);
                if (Match != null)
                {
                    Match.Populate_spreadsheet_row(cellSet, rowNumber);
                }
            }
        }

        private void Populate_cell_with_amount(ICellSet cellSet, int rowNumber, int amountIndex, double amount)
        {
            if (amount != 0)
            {
                cellSet.Populate_cell(rowNumber, amountIndex + 1, amount);
            }
            else
            {
                cellSet.Populate_cell(rowNumber, amountIndex + 1, String.Empty);
            }
        }

        public void Read_from_spreadsheet_row(ICellRow cellSet)
        {
            Date = DateTime.FromOADate((double)cellSet.Read_cell(DateIndex));
            Unreconciled_amount = cellSet.Read_cell(UnreconciledAmountIndex) != null
                ? (Double)cellSet.Read_cell(UnreconciledAmountIndex)
                : 0;
            Type = (String)cellSet.Read_cell(TypeIndex);
            Description = (String)cellSet.Read_cell(DescriptionIndex);
            Cheque_number = cellSet.Count > ChequeNumberIndex && cellSet.Read_cell(ChequeNumberIndex) != null 
                ? Convert.ToInt32((Double)cellSet.Read_cell(ChequeNumberIndex)) 
                : 0;
            Reconciled_amount = cellSet.Count > ReconciledAmountIndex && cellSet.Read_cell(ReconciledAmountIndex) != null 
                ? (Double)cellSet.Read_cell(ReconciledAmountIndex)
                : 0;

            Source_line = To_string(_separator, false);
        }

        private String To_string(char separator, bool encaseDescriptionInQuotes = true, bool formatCurrency = true)
        {
            return Date.ToString(@"dd\/MM\/yyyy") + separator
                + (Unreconciled_amount == 0 ? "" : Unreconciled_amount.To_csv_string(formatCurrency)) + separator
                + (Matched ? "x" : "") + separator
                + Type + separator
                + (encaseDescriptionInQuotes ? Description.Encase_in_escaped_quotes_if_not_already_encased() : Description) + separator
                + (Cheque_number == 0 ? "" : Cheque_number.ToString()) + separator
                + (Reconciled_amount == 0 ? "" : Reconciled_amount.To_csv_string(formatCurrency)) + separator + separator + separator
                + (Match != null ? separator.ToString() + separator.ToString() + Match.To_csv(formatCurrency) : "");
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

        public void Change_main_amount(double newValue)
        {
            Unreconciled_amount = newValue;
        }

        public string Transaction_type()
        {
            return Type;
        }

        public int Extra_info()
        {
            return Cheque_number;
        }

        public void Reconcile()
        {
            Reconciled_amount = Unreconciled_amount;
            Unreconciled_amount = 0;
        }

        public void Convert_source_line_separators(char originalSeparator, char newSeparator)
        {
            Source_line = Source_line.Convert_separators(originalSeparator, newSeparator);
        }

        public ICSVRecord With_date(DateTime newDate)
        {
            Date = newDate;
            return this;
        }

        public ICSVRecord Copy()
        {
            return new BankRecord
            {
                Date = Date,
                Unreconciled_amount = Unreconciled_amount,
                Type = Type,
                Description = Description,
                Reconciled_amount = Reconciled_amount,
                Cheque_number = Cheque_number,
                Source_line = Source_line
            };
        }

        public void Update_source_line_for_output(char outputSeparator)
        {
            Source_line = To_string(outputSeparator);
        }
    }
}