using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces.Constants
{
    public static class ReconConsts
    {
        static readonly XmlReader _xmlReader = new XmlReader();

        public const string ConfigFileName = "Config.xml";
        public const string SampleConfigFileName = "SampleConfig.xml";
        // This will get overwritten if a different path is passed into Program.cs from the command line
        // - eg if you are running the code in .Net Core on a Mac
        public static string ConfigFilePath = @"C:/Config";
        public const string ConfigPathProperty = "MainConfigFilePath";

        public static string TestValueDoNotChange => _xmlReader.ReadXml(nameof(TestValueDoNotChange));

        public static string MainSpreadsheetPath => _xmlReader.ReadXml(nameof(MainSpreadsheetPath));
        public static string DefaultFilePath => _xmlReader.ReadXml(nameof(DefaultFilePath));
        public static string MainSpreadsheetFileName => _xmlReader.ReadXml(nameof(MainSpreadsheetFileName));
        public static string TestBackupFilePath => _xmlReader.ReadXml(nameof(TestBackupFilePath));
        public static string SourceDebugSpreadsheetPath => _xmlReader.ReadXml(nameof(SourceDebugSpreadsheetPath));
        public static string BackupSubFolder => _xmlReader.ReadXml(nameof(BackupSubFolder));
        public static string DebugSpreadsheetFileName => _xmlReader.ReadXml(nameof(DebugSpreadsheetFileName));
        public static string PocketMoneySpreadsheetPath => _xmlReader.ReadXml(nameof(PocketMoneySpreadsheetPath));

        public static string FiveFileOptions => _xmlReader.ReadXml(nameof(FiveFileOptions));
        public static string FourAccountingTypes => _xmlReader.ReadXml(nameof(FourAccountingTypes));
        public static string FourFileNameOptions => _xmlReader.ReadXml(nameof(FourFileNameOptions));
        public static string FiveFileDetails => _xmlReader.ReadXml(nameof(FiveFileDetails));

        public static string File_Option_02 => _xmlReader.ReadXml(nameof(File_Option_02));
        public static string File_Option_03 => _xmlReader.ReadXml(nameof(File_Option_03));
        public static string File_Option_04 => _xmlReader.ReadXml(nameof(File_Option_04));
        public static string File_Option_05 => _xmlReader.ReadXml(nameof(File_Option_05));

        public static string Accounting_Type_01 => _xmlReader.ReadXml(nameof(Accounting_Type_01));
        public static string Accounting_Type_02 => _xmlReader.ReadXml(nameof(Accounting_Type_02));
        public static string Accounting_Type_03 => _xmlReader.ReadXml(nameof(Accounting_Type_03));
        public static string Accounting_Type_04 => _xmlReader.ReadXml(nameof(Accounting_Type_04));

        public static string File_Name_Option_02 => _xmlReader.ReadXml(nameof(File_Name_Option_02));
        public static string File_Name_Option_03 => _xmlReader.ReadXml(nameof(File_Name_Option_03));
        public static string File_Name_Option_04 => _xmlReader.ReadXml(nameof(File_Name_Option_04));

        public static string File_Details_02 => _xmlReader.ReadXml(nameof(File_Details_02));
        public static string File_Details_03 => _xmlReader.ReadXml(nameof(File_Details_03));
        public static string File_Details_04 => _xmlReader.ReadXml(nameof(File_Details_04));
        public static string File_Details_05 => _xmlReader.ReadXml(nameof(File_Details_05));

        public static string DefaultBankFileName => _xmlReader.ReadXml(nameof(DefaultBankFileName));
        public static string DefaultCredCard1FileName => _xmlReader.ReadXml(nameof(DefaultCredCard1FileName));
        public static string DefaultCredCard2FileName => _xmlReader.ReadXml(nameof(DefaultCredCard2FileName));
        public static string DefaultCredCard1InOutFileName => _xmlReader.ReadXml(nameof(DefaultCredCard1InOutFileName));
        public static string DefaultCredCard2InOutFileName => _xmlReader.ReadXml(nameof(DefaultCredCard2InOutFileName));

        public static string DefaultCredCard2InOutPendingFileName => _xmlReader.ReadXml(nameof(DefaultCredCard2InOutPendingFileName));
        public static string DefaultCredCard1InOutPendingFileName => _xmlReader.ReadXml(nameof(DefaultCredCard1InOutPendingFileName));

        public static string BankDescriptor => _xmlReader.ReadXml(nameof(BankDescriptor));
        public static string CredCard1Descriptor => _xmlReader.ReadXml(nameof(CredCard1Descriptor));
        public static string CredCard1InOutDescriptor => _xmlReader.ReadXml(nameof(CredCard1InOutDescriptor));
        public static string CredCard2Descriptor => _xmlReader.ReadXml(nameof(CredCard2Descriptor));
        public static string CredCard2InOutDescriptor => _xmlReader.ReadXml(nameof(CredCard2InOutDescriptor));
        public static string EmployerExpenseDescription => _xmlReader.ReadXml(nameof(EmployerExpenseDescription));

        public static string CredCard1Name => _xmlReader.ReadXml(nameof(CredCard1Name));
        public static string CredCard2Name => _xmlReader.ReadXml(nameof(CredCard2Name));
        public static string CredCard1DdDescription => _xmlReader.ReadXml(nameof(CredCard1DdDescription));
        public static string CredCard2DdDescription => _xmlReader.ReadXml(nameof(CredCard2DdDescription));
        public static string CredCard1RegularPymtDescription => _xmlReader.ReadXml(nameof(CredCard1RegularPymtDescription));
        public static string CredCard2RegularPymtDescription => _xmlReader.ReadXml(nameof(CredCard2RegularPymtDescription));

        public static string Instructions_Line_01 => _xmlReader.ReadXml(nameof(Instructions_Line_01));
        public static string Instructions_Line_02 => _xmlReader.ReadXml(nameof(Instructions_Line_02));
        public static string Instructions_Line_03 => _xmlReader.ReadXml(nameof(Instructions_Line_03));
        public static string Instructions_Line_04 => _xmlReader.ReadXml(nameof(Instructions_Line_04));
        public static string Instructions_Line_05 => _xmlReader.ReadXml(nameof(Instructions_Line_05));
        public static string Instructions_Line_06 => _xmlReader.ReadXml(nameof(Instructions_Line_06));
        public static string Instructions_Line_07 => _xmlReader.ReadXml(nameof(Instructions_Line_07));
        public static string Instructions_Line_08 => _xmlReader.ReadXml(nameof(Instructions_Line_08));
        public static string Instructions_Line_09 => _xmlReader.ReadXml(nameof(Instructions_Line_09));
        public static string Instructions_Line_10 => _xmlReader.ReadXml(nameof(Instructions_Line_10));
        public static string Instructions_Line_11 => _xmlReader.ReadXml(nameof(Instructions_Line_11));

        public static string BankOutHeader => _xmlReader.ReadXml(nameof(BankOutHeader));
        public static string BankInHeader => _xmlReader.ReadXml(nameof(BankInHeader));
        public static string CredCard2Header => _xmlReader.ReadXml(nameof(CredCard2Header));

        public static string LoadPendingCsvs => _xmlReader.ReadXml(nameof(LoadPendingCsvs));
        public static string LoadingExpenses => _xmlReader.ReadXml(nameof(LoadingExpenses));

        public const string MissingCodeInWorksheet =
            "There is no \"{0}\" row in this worksheet ({1}), so we can't find the data we are looking for.";

        public const string CodeInWrongPlace =
            "The word/phrase \"{0}\" has been used somewhere unexpected in {1} (probably in a text field).";

        public const string ExpectedOutInsertionPoint = "***** EVERYTHING BELOW THIS LINE NOT INCLUDED IN TRACKING";

        public const string MissingCell = "Cell does not exist: ";

        public const string EnterMonths =
                "Enter the last month you want to plan for. \nBudget amounts will be added from {0} to the month you specify (inclusive). \nEnter 0 if no budgeting required.";

        public const string ConfirmBadMonth = "Your input was 0 or invalid. Would you like to proceed without adding monthly / annual budget amounts? Enter Y for Yes";

        public const string ConfirmMonthInterval = "Going from {0} to {1} means you will budgeting for {2} months. Would you like to proceed? Enter Y for Yes";

        public const string ConfirmNoMonthlyBudgeting =
            "You have chosen to continue without adding any monthly or annual budgeting amounts.";

        public const string CantFindMortgageRow = "Can't find mortgage row (used for calculating next unplanned month, for budget out). \nCheck Budget Out has mortgage description that matches Bank Out. \nIf you've just changed it, it will be fine next time. \nFor now, please enter number for the first month you would like to schedule budgeted amounts:";
        public const string DividerText = "Divider";

        public const string DefaultUnplannedMonth =
                "Your input was 0 or invalid. Using default of current month for next unplanned month. \nIf this doesn't work for you, exit and return with better input!";

        public const string LoadingDataFromPendingFile =
            "Loading data from pending file (which you should have already split out, if necessary)...";

        public const string MergingSomeBudgetData =
            "Merging some budget data with pending data, and writing some direct to spreadsheet...";

        public const string PendingOrReconciliate = "1.PendingCsvs, 2.Reconciliate";
        public const string DebugOrReal = "1.Debug, 2.Real";
        public const string PathOrDefault = "1.EnterPath, 2.UseDefault";
        public const string EnterCsvPath = "Enter the path for your csv files:";
        public const string EnterThirdPartyFileName = "Enter the file name for your third party csv file (eg from your bank's website) - don't include the csv extension:";
        public const string EnterOwnedFileName = "Enter the file name for your own csv file (eg Bank In) - don't include the csv extension:";

        public const string DefaultBankInFileName = "BankIn";
        public const string DefaultBankOutFileName = "BankOut";

        public const string DefaultBankInPendingFileName = "BankInPending";
        public const string DefaultBankOutPendingFileName = "BankOutPending";

        public const string GoAgainFinish = "1. GoAgain; 2. Finish";
        public const string EnterNumberOfMatch = "Enter the number of the matching item, or I for ignore, or D for delete.";
        public const string WhetherToDeleteThirdParty = "Delete third party record? Y/N";
        public const string EnterAnyKeyToContinue = "Enter any character to continue, or Exit if you want to abandon ship.";
        public const string EnterDeletionIndex = "Enter the index of the record to delete.";
        public const string NoMatchesLeft = "There are no matches left for this record, so we'll move onto the next record.";
        public const string WritingNewData = "Writing new data...";
        public const string Finished = "All done!";
        public const string BadTallyMatchedItems = "Something is wrong! The number of matched owned records doesn't tally with the number of matched third party records!";
        public const string BadTallyNumMatchedThirdParty = "Something is wrong! The number of matched third party records is greater than the number of owned records!";
        public const string BadTallyNumMatchedOwned = "Something is wrong! The number of matched owned records is greater than the number of third party records!";
        public const string ChooseWhatToDoWithMatches = "Press Enter to continue without doing anything:";
        public const string AutoMatches = "Auto matches";
        public const string FinalMatches = "Final matches";
        public const string OriginalAmountWas = " !! original amount was ";

        public const string BankInDescriptor = "Bank In";
        public const string BankOutDescriptor = "Bank Out";
        public const string SeveralExpenses = "Several expenses";

        public const int PartialDateMatchThreshold = 3;
        public const int PartialAmountMatchThreshold = 2;

        public const string CredCardBalanceDescription = "!! {0} bal recorded from statement dated {1}";
        public const string AskForCredCardDirectDebit = "If you have it, please enter the expected {0} Direct Debit for around {1}. Otherwise enter 0.";

        public const int DdDescriptionColumn = 15;
        public const int DescriptionColumn = 5;

        public static string BadMatchNumber = "Bad match number.";
    }
}