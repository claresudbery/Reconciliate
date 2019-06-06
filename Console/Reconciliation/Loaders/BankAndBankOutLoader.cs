using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankOutLoader : ILoader<ActualBankRecord, BankRecord>
    {
        public DataLoadingInformation<ActualBankRecord, BankRecord> LoadingInfo()
        {
            return new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                FilePaths = new FilePaths
                {
                    MainPath = ReconConsts.DefaultFilePath,
                    ThirdPartyFileName = ReconConsts.DefaultBankFileName,
                    OwnedFileName = ReconConsts.DefaultBankOutFileName
                },
                DefaultSeparator = ',',
                LoadingSeparator = '^',
                PendingFileName = ReconConsts.DefaultBankOutPendingFileName,
                SheetName = MainSheetNames.BankOut,
                ThirdPartyDescriptor = ReconConsts.BankDescriptor,
                OwnedFileDescriptor = ReconConsts.BankOutDescriptor,
                Loader = this,
                MonthlyBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.SODDs,
                    EndDivider = Dividers.CredCard1,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 6
                },
                AnnualBudgetData = new BudgetItemListData
                {
                    SheetName = MainSheetNames.BudgetOut,
                    StartDivider = Dividers.AnnualSODDs,
                    EndDivider = Dividers.AnnualTotal,
                    FirstColumnNumber = 2,
                    LastColumnNumber = 6
                }
            };
        }

        public IDataFile<ActualBankRecord> CreateNewThirdPartyFile(IFileIO<ActualBankRecord> thirdPartyFileIO)
        {
            var csvFile = new CSVFile<ActualBankRecord>(thirdPartyFileIO);
            return new ActualBankOutFile(csvFile);
        }

        public IDataFile<BankRecord> CreateNewOwnedFile(IFileIO<BankRecord> ownedFileIO)
        {
            var csvFile = new CSVFile<BankRecord>(ownedFileIO);
            return new GenericFile<BankRecord>(csvFile);
        }

        public void MergeBespokeDataWithPendingFile(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pendingFile,
            BudgetingMonths budgetingMonths,
            DataLoadingInformation<ActualBankRecord, BankRecord> dataLoadingInfo)
        {
            // When we don't have loaders any more, this functionality will have to exist in another switch statement,
            // in ReconciliationIntro, with separate functions for each type. See ReconciliationIntro.MergeOtherData.
            // Just copy this Code wholesale - no need to trace back through commits.
            AddMostRecentCreditCardDirectDebits(
                inputOutput,
                spreadsheet,
                pendingFile,
                ReconConsts.CredCard1Name,
                ReconConsts.CredCard1DdDescription);

            AddMostRecentCreditCardDirectDebits(
                inputOutput,
                spreadsheet,
                pendingFile,
                ReconConsts.CredCard2Name,
                ReconConsts.CredCard2DdDescription);
        }

        private void AddMostRecentCreditCardDirectDebits(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pendingFile,
            string credCardName,
            string directDebitDescription)
        {
            var mostRecentCredCardDirectDebit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut, 
                directDebitDescription,
                new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn});

            var nextDate = mostRecentCredCardDirectDebit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                credCardName,
                nextDate.ToShortDateString()));
            while (input != "0")
            {
                double amount;
                if (double.TryParse(input, out amount))
                {
                    pendingFile.Records.Add(new BankRecord
                    {
                        Date = nextDate,
                        Description = directDebitDescription,
                        Type = "POS",
                        UnreconciledAmount = amount
                    });
                }
                nextDate = nextDate.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit, 
                    credCardName,
                    nextDate.ToShortDateString()));
            }
        }
    }
}