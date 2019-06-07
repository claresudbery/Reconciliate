﻿using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces.DTOs;

namespace Interfaces
{
    internal interface ISpreadsheet
    {
        ICellRow ReadLastRow(String sheetName);
        int FindRowNumberOfLastDividerRow(string sheetName);
        String ReadLastRowAsCsv(String sheetName, ICSVRecord csvRecord);
        String ReadSpecifiedRowAsCsv(String sheetName, int rowNumber, ICSVRecord csvRecord);
        void AddUnreconciledRowsToCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new();
        void AppendCsvFile<TRecordType>(string sheetName, ICSVFile<TRecordType> csvFile) where TRecordType : ICSVRecord, new();
        ICSVFile<TRecordType> ReadUnreconciledRowsAsCsvFile<TRecordType>(
            ICSVFileFactory<TRecordType> csvFileFactory,
            String sheetName) where TRecordType : ICSVRecord, new();
        void DeleteUnreconciledRows(string sheetName);
        double GetSecondChildPocketMoneyAmount(string shortDateTime);
        TRecordType GetMostRecentRowContainingText<TRecordType>(
            string sheetName,
            string textToSearchFor,
            List<int> expectedColumnNumbers)
            where TRecordType : ICSVRecord, new();
        double GetPlanningExpensesAlreadyDone();
        double GetPlanningMoneyPaidByGuests();
        void InsertNewRowOnExpectedOut(double newAmount, string newNotes);
        void AddNewTransactionToSavings(DateTime newDate, double newAmount);
        void UpdateBalanceOnTotalsSheet(
            string balanceCode,
            double newBalance,
            string newText,
            int balanceColumn,
            int textColumn,
            int codeColumn);
        DateTime GetNextUnplannedMonth();

        void AddBudgetedBankInDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData budgetItemListData);
        void AddBudgetedBankOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<BankRecord> pendingFile,
            BudgetItemListData monthlyBudgetItemListData,
            BudgetItemListData annualBudgetItemListData);
        void AddBudgetedCredCard1InOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard1InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData);
        void AddBudgetedCredCard2InOutDataToPendingFile(
            BudgetingMonths budgetingMonths,
            ICSVFile<CredCard2InOutRecord> pendingFile,
            BudgetItemListData budgetItemListData);
    }
}