using System;
using Interfaces.DTOs;

namespace Interfaces
{
    public interface ICSVRecord
    {
        bool Matched { get; set; }
        bool Divider { get; set; }
        DateTime Date { get; set; }
        string Description { get; set; }
        string SourceLine { get; }
        string OutputSourceLine { get; }
        ICSVRecord Match { get; set; }

        void Create_from_match(DateTime date, double amount, string type, string description, int extra_info, ICSVRecord matched_record);
        void Load(string csv_line, char? override_separator = null);
        void Load_from_original_line();
        bool Main_amount_is_negative();
        void Make_main_amount_positive();
        void Swap_sign_of_main_amount();
        void Reconcile();
        string To_csv(bool format_currency = true);
        ConsoleLine To_console(int index = -1);
        void Populate_spreadsheet_row(ICellSet cell_set, int row_number);
        void Read_from_spreadsheet_row(ICellRow cell_row);
        void Convert_source_line_separators(char original_separator, char new_separator);

        double Main_amount();
        void Change_main_amount(double new_value);
        string Transaction_type();
        int Extra_info();
        ICSVRecord Copy();
        ICSVRecord With_date(DateTime new_date);
        void Update_source_line_for_output(char output_separator);
    }
}