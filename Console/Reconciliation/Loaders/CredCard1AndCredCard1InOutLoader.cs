using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class CredCard1AndCredCard1InOutLoader : ILoader<CredCard1Record, CredCard1InOutRecord>
    {
        public DataLoadingInformation<CredCard1Record, CredCard1InOutRecord> LoadingInfo()
        {
            return new DataLoadingInformation<CredCard1Record, CredCard1InOutRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultCredCard1FileName,
                    OwnedFileName = ReconConsts.DefaultCredCard1InOutFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultCredCard1InOutPendingFileName,
                SheetName = MainSheetNames.CredCard1,
                ThirdPartyDescriptor = ReconConsts.CredCard1Descriptor,
                OwnedFileDescriptor = ReconConsts.CredCard1InOutDescriptor,
                Loader = this,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.CredCard1,
                    EndDivider = Dividers.CredCard2,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 5
                },
                AnnualBudgetData = null
            };
        }

        public IDataFile<CredCard1Record> CreateNewThirdPartyFile(IFileIO<CredCard1Record> thirdPartyFileIO)
        {
            var csvFile = new CSVFile<CredCard1Record>(thirdPartyFileIO);
            return new CredCard1File(csvFile);
        }

        public IDataFile<CredCard1InOutRecord> CreateNewOwnedFile(IFileIO<CredCard1InOutRecord> ownedFileIO)
        {
            var csvFile = new CSVFile<CredCard1InOutRecord>(ownedFileIO);
            return new GenericFile<CredCard1InOutRecord>(csvFile);
        }

        public void MergeBespokeDataWithPendingFile(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<CredCard1InOutRecord> pendingFile,
            BudgetingMonths budgetingMonths,
            DataLoadingInformation<CredCard1Record, CredCard1InOutRecord> dataLoadingInfo)
        {
            // When we don't have loaders any more, this functionality will have to exist in another switch statement,
            // in ReconciliationIntro, with separate functions for each type. See ReconciliationIntro.MergeOtherData.
            // Just copy this Code wholesale - no need to trace back through commits.
            var mostRecentCredCardDirectDebit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut, 
                ReconConsts.CredCard1DdDescription,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var statementDate = new DateTime();
            var nextDate = mostRecentCredCardDirectDebit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.CredCard1Name,
                nextDate.ToShortDateString()));
            double newBalance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out newBalance))
                {
                    pendingFile.Records.Add(new CredCard1InOutRecord
                    {
                        Date = nextDate,
                        Description = ReconConsts.CredCard1RegularPymtDescription,
                        UnreconciledAmount = newBalance
                    });
                }
                statementDate = nextDate.AddMonths(-1);
                nextDate = nextDate.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.CredCard1Name,
                    nextDate.ToShortDateString()));
            }

            spreadsheet.UpdateBalanceOnTotalsSheet(
                Codes.CredCard1Bal,
                newBalance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.CredCard1Name,
                    $"{statementDate.ToString("MMM")} {statementDate.Year}"),
                balanceColumn: 5,
                textColumn: 6,
                codeColumn: 4);
        }
    }
}