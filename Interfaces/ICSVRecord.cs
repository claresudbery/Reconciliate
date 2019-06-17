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
        string Source_line { get; }
        ICSVRecord Match { get; set; }

        void Create_from_match(DateTime date, double amount, string type, string description, int extraInfo, ICSVRecord matchedRecord);
        void Load(string csvLine, char? overrideSeparator = null);
        bool Main_amount_is_negative();
        void Make_main_amount_positive();
        void Swap_sign_of_main_amount();
        void Reconcile();
        string To_csv(bool formatCurrency = true);
        ConsoleLine To_console(int index = -1);
        void Populate_spreadsheet_row(ICellSet cellSet, int rowNumber);
        void Read_from_spreadsheet_row(ICellRow cellRow);
        void Convert_source_line_separators(char originalSeparator, char newSeparator);

        double Main_amount();
        void Change_main_amount(double newValue);
        string Transaction_type();
        int Extra_info();
        ICSVRecord Copy();
        ICSVRecord With_date(DateTime newDate);
        void Update_source_line_for_output(char outputSeparator);
    }
}