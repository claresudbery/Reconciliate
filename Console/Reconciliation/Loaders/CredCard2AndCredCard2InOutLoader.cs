using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class CredCard2AndCredCard2InOutLoader : ILoader<CredCard2Record, CredCard2InOutRecord>
    {
        public DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> LoadingInfo()
        {
            return new DataLoadingInformation<CredCard2Record, CredCard2InOutRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultCredCard2FileName,
                    OwnedFileName = ReconConsts.DefaultCredCard2InOutFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultCredCard2InOutPendingFileName,
                SheetName = MainSheetNames.CredCard2,
                ThirdPartyDescriptor = ReconConsts.CredCard2Descriptor,
                OwnedFileDescriptor = ReconConsts.CredCard2InOutDescriptor,
                Loader = this,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.CredCard2,
                    EndDivider = Dividers.SODDTotal,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 5
                },
                AnnualBudgetData = null
            };
        }

        public IDataFile<CredCard2Record> CreateNewThirdPartyFile(IFileIO<CredCard2Record> thirdPartyFileIO)
        {
            var csvFile = new CSVFile<CredCard2Record>(thirdPartyFileIO);
            return new GenericFile<CredCard2Record>(csvFile);
        }

        public IDataFile<CredCard2InOutRecord> CreateNewOwnedFile(IFileIO<CredCard2InOutRecord> ownedFileIO)
        {
            var csvFile = new CSVFile<CredCard2InOutRecord>(ownedFileIO);
            return new GenericFile<CredCard2InOutRecord>(csvFile);
        }

        public void MergeBespokeDataWithPendingFile(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<CredCard2InOutRecord> pendingFile,
            BudgetingMonths budgetingMonths,
            DataLoadingInformation<CredCard2Record, CredCard2InOutRecord> dataLoadingInfo)
        {
            // When we don't have loaders any more, this functionality will have to exist in another switch statement,
            // in ReconciliationIntro, with separate functions for each type. See ReconciliationIntro.MergeOtherData.
            // Just copy this Code wholesale - no need to trace back through commits.
            var mostRecentCredCardDirectDebit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut, 
                ReconConsts.CredCard2DdDescription, 
                new List<int>{ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn});

            var statementDate = new DateTime();
            var nextDate = mostRecentCredCardDirectDebit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.CredCard2Name,
                nextDate.ToShortDateString()));
            double newBalance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out newBalance))
                {
                    pendingFile.Records.Add(new CredCard2InOutRecord
                    {
                        Date = nextDate,
                        Description = ReconConsts.CredCard2RegularPymtDescription,
                        UnreconciledAmount = newBalance
                    });
                }
                statementDate = nextDate.AddMonths(-1);
                nextDate = nextDate.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.CredCard2Name, 
                    nextDate.ToShortDateString()));
            }

            spreadsheet.UpdateBalanceOnTotalsSheet(
                Codes.CredCard2Bal,
                newBalance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.CredCard2Name,
                    $"{statementDate.ToString("MMM")} {statementDate.Year}"),
                balanceColumn: 5,
                textColumn: 6,
                codeColumn: 4);
        }
    }
}