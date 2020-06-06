using System;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Interfaces.Extensions;

namespace ConsoleCatchall.Console.Reconciliation.Records
{
    internal class ExpectedIncomeRecord : ICSVRecord
    {
        public ICSVRecord Match { get; set; }
        public bool Matched { get; set; }
        public bool Divider { get; set; }
        public string SourceLine { get; private set; }
        public string OutputSourceLine { get; private set; }

        // Source data - loaded on startup (if any new essential fields are added, update EssentialFields value below):
        public DateTime Date { get; set; }
        public double Unreconciled_amount { get; set; }
        public string Code { get; set; }
        public double Reconciled_amount { get; set; }
        public DateTime Date_paid { get; set; }
        public double Total_paid { get; set; }
        public string Description { get; set; }

        private char _default_separator = ',';

        public const int DateIndex = 0;
        public const int UnreconciledAmountIndex = 1;
        public const int DividerIndex = 1;
        public const int CodeIndex = 2;
        public const int ReconciledAmountIndex = 3;
        public const int DatePaidIndex = 4;
        public const int TotalPaidIndex = 5;
        public const int DescriptionIndex = 6;

        public const string EssentialFields = "unreconciled amount, code or description";

        public void Create_from_match(DateTime date, double amount, string type, string description, int extra_info,
            ICSVRecord matched_record)
        {
        }

        public void Load(string csv_line, char? override_separator = null)
        {
        }

        public void Load_from_original_line()
        {
        }

        public bool Main_amount_is_negative()
        {
            return Unreconciled_amount < 0;
        }

        public void Make_main_amount_positive()
        {
            Unreconciled_amount = Math.Abs(Unreconciled_amount);
        }

        public void Swap_sign_of_main_amount()
        {
            Unreconciled_amount = Unreconciled_amount * -1;
        }

        public void Reconcile()
        {
            Reconciled_amount = Unreconciled_amount;
            Unreconciled_amount = 0;
        }

        public string To_csv(bool format_currency = true)
        {
            return To_string(',', true, format_currency);
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
                cell_set.Populate_cell(row_number, CodeIndex + 1, Code);
                Populate_cell_with_amount(cell_set, row_number, ReconciledAmountIndex, Reconciled_amount);
                cell_set.Populate_cell(row_number, DatePaidIndex + 1, Date_paid);
                Populate_cell_with_amount(cell_set, row_number, TotalPaidIndex, Total_paid);
                cell_set.Populate_cell(row_number, DescriptionIndex + 1, Description);
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

        public void Read_from_spreadsheet_row(ICellRow cell_row)
        {
            Date = DateTime.FromOADate(cell_row.Read_cell(DateIndex) != null 
                ? (double)cell_row.Read_cell(DateIndex)
                : 0);
            Unreconciled_amount = cell_row.Count > UnreconciledAmountIndex && cell_row.Read_cell(UnreconciledAmountIndex) != null
                ? (Double)cell_row.Read_cell(UnreconciledAmountIndex)
                : 0;
            Code = (String)cell_row.Read_cell(CodeIndex);
            Reconciled_amount = cell_row.Count > ReconciledAmountIndex && cell_row.Read_cell(ReconciledAmountIndex) != null
                ? (Double)cell_row.Read_cell(ReconciledAmountIndex)
                : 0;
            Date_paid = DateTime.FromOADate(cell_row.Read_cell(DatePaidIndex) != null
                ? (double)cell_row.Read_cell(DatePaidIndex)
                : 0);
            Total_paid = cell_row.Count > TotalPaidIndex && cell_row.Read_cell(TotalPaidIndex) != null
                ? (Double)cell_row.Read_cell(TotalPaidIndex)
                : 0;
            Description = ((String)cell_row.Read_cell(DescriptionIndex)).Strip_enclosing_quotes();

            SourceLine = To_string(_default_separator, false);
            OutputSourceLine = To_string(_default_separator, false);
        }

        private String To_string(char separator, bool encase_description_in_quotes = true, bool format_currency = true)
        {
            return (Date.ToOADate() == 0 ? "" : Date.ToString(@"dd\/MM\/yyyy")) + separator
                   + (Unreconciled_amount == 0 ? "" : Unreconciled_amount.To_csv_string(format_currency)) + separator
                   + Code + separator
                   + (Reconciled_amount == 0 ? "" : Reconciled_amount.To_csv_string(format_currency)) + separator
                   + (Date_paid.ToOADate() == 0 ? "" : Date_paid.ToString(@"dd\/MM\/yyyy")) + separator
                   + (Total_paid == 0 ? "" : Total_paid.To_csv_string(format_currency)) + separator
                   + (string.IsNullOrEmpty(Description)
                        ? ""
                        : (encase_description_in_quotes ? Description.Encase_in_escaped_quotes_if_not_already_encased() : Description));
        }

        public void Convert_source_line_separators(char original_separator, char new_separator)
        {
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
            return Code;
        }

        public int Extra_info()
        {
            return 0;
        }

        public void Copy_from(ICSVRecord source)
        {
            if (source.GetType() == typeof(ExpectedIncomeRecord))
            {
                ExpectedIncomeRecord typed_source = source as ExpectedIncomeRecord;

                Date = typed_source.Date;
                Unreconciled_amount = typed_source.Unreconciled_amount;
                Code = typed_source.Code;
                Reconciled_amount = typed_source.Reconciled_amount;
                Date_paid = typed_source.Date_paid;
                Total_paid = typed_source.Total_paid;
                Description = typed_source.Description;
                SourceLine = typed_source.SourceLine;
                OutputSourceLine = typed_source.OutputSourceLine;

                Match = typed_source.Match;
                Matched = typed_source.Matched;
                Divider = typed_source.Divider;
            }
            else
            {
                throw (new Exception("Trying to copy record but it's not a ExpectedIncomeRecord type."));
            }
        }

        // Remember you can use Copy_from if you're having trouble with generic types.
        public ICSVRecord Copy()
        {
            return new ExpectedIncomeRecord
            {
                Date = Date,
                Unreconciled_amount = Unreconciled_amount,
                Code = Code,
                Reconciled_amount = Reconciled_amount,
                Date_paid = Date_paid,
                Total_paid = Total_paid,
                Description = Description,
                SourceLine = SourceLine,
                OutputSourceLine = OutputSourceLine,

                Match = Match,
                Matched = Matched,
                Divider = Divider
            };
        }

        public ICSVRecord With_date(DateTime new_date)
        {
            Date = new_date;
            return this;
        }

        public void Update_source_line_for_output(char output_separator)
        {
            OutputSourceLine = To_string(output_separator);
        }

        public BankRecord Convert_to_bank_record()
        {
            return new BankRecord
            {
                Date = Date,
                Unreconciled_amount = Unreconciled_amount,
                Type = Code,
                Description = Description
            };
        }
    }
}