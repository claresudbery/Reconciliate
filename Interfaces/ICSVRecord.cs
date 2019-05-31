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
        ICSVRecord Match { get; set; }

        void CreateFromMatch(DateTime date, double amount, string type, string description, int extraInfo, ICSVRecord matchedRecord);
        void Load(string csvLine, char? overrideSeparator = null);
        bool MainAmountIsNegative();
        void MakeMainAmountPositive();
        void SwapSignOfMainAmount();
        void Reconcile();
        string ToCsv(bool formatCurrency = true);
        ConsoleLine ToConsole(int index = -1);
        void PopulateSpreadsheetRow(ICellSet cellSet, int rowNumber);
        void ReadFromSpreadsheetRow(ICellRow cellRow);
        void ConvertSourceLineSeparators(char originalSeparator, char newSeparator);

        double MainAmount();
        void ChangeMainAmount(double newValue);
        string TransactionType();
        int ExtraInfo();
        ICSVRecord Copy();
        ICSVRecord WithDate(DateTime newDate);
        void UpdateSourceLineForOutput(char outputSeparator);
    }
}