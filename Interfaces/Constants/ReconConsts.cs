using System.Collections.Generic;
using Interfaces.DTOs;

namespace Interfaces.Constants
{
    public static class ReconConsts
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Test_value_do_not_change => XmlReader.Read_xml(nameof(Test_value_do_not_change));

        public static string Main_spreadsheet_path => XmlReader.Read_xml(nameof(Main_spreadsheet_path));
        public static string Default_file_path => XmlReader.Read_xml(nameof(Default_file_path));
        public static string Main_spreadsheet_file_name => XmlReader.Read_xml(nameof(Main_spreadsheet_file_name));
        public static string Test_backup_file_path => XmlReader.Read_xml(nameof(Test_backup_file_path));
        public static string Source_debug_spreadsheet_path => XmlReader.Read_xml(nameof(Source_debug_spreadsheet_path));
        public static string Backup_sub_folder => XmlReader.Read_xml(nameof(Backup_sub_folder));
        public static string Debug_spreadsheet_file_name => XmlReader.Read_xml(nameof(Debug_spreadsheet_file_name));
        public static string Pocket_money_spreadsheet_path => XmlReader.Read_xml(nameof(Pocket_money_spreadsheet_path));

        public static string Five_file_options => XmlReader.Read_xml(nameof(Five_file_options));
        public static string Four_accounting_types => XmlReader.Read_xml(nameof(Four_accounting_types));
        public static string Four_file_name_options => XmlReader.Read_xml(nameof(Four_file_name_options));
        public static string Five_file_details => XmlReader.Read_xml(nameof(Five_file_details));

        public static string File_option_02 => XmlReader.Read_xml(nameof(File_option_02));
        public static string File_option_03 => XmlReader.Read_xml(nameof(File_option_03));
        public static string File_option_04 => XmlReader.Read_xml(nameof(File_option_04));
        public static string File_option_05 => XmlReader.Read_xml(nameof(File_option_05));

        public static string Accounting_type_01 => XmlReader.Read_xml(nameof(Accounting_type_01));
        public static string Accounting_type_02 => XmlReader.Read_xml(nameof(Accounting_type_02));
        public static string Accounting_type_03 => XmlReader.Read_xml(nameof(Accounting_type_03));
        public static string Accounting_type_04 => XmlReader.Read_xml(nameof(Accounting_type_04));

        public static string File_name_option_02 => XmlReader.Read_xml(nameof(File_name_option_02));
        public static string File_name_option_03 => XmlReader.Read_xml(nameof(File_name_option_03));
        public static string File_name_option_04 => XmlReader.Read_xml(nameof(File_name_option_04));

        public static string File_details_02 => XmlReader.Read_xml(nameof(File_details_02));
        public static string File_details_03 => XmlReader.Read_xml(nameof(File_details_03));
        public static string File_details_04 => XmlReader.Read_xml(nameof(File_details_04));
        public static string File_details_05 => XmlReader.Read_xml(nameof(File_details_05));

        public static string Default_bank_file_name => XmlReader.Read_xml(nameof(Default_bank_file_name));
        public static string Default_cred_card1_file_name => XmlReader.Read_xml(nameof(Default_cred_card1_file_name));
        public static string Default_cred_card2_file_name => XmlReader.Read_xml(nameof(Default_cred_card2_file_name));
        public static string Default_cred_card1_in_out_file_name => XmlReader.Read_xml(nameof(Default_cred_card1_in_out_file_name));
        public static string Default_cred_card2_in_out_file_name => XmlReader.Read_xml(nameof(Default_cred_card2_in_out_file_name));

        public static string Default_cred_card2_in_out_pending_file_name => XmlReader.Read_xml(nameof(Default_cred_card2_in_out_pending_file_name));
        public static string Default_cred_card1_in_out_pending_file_name => XmlReader.Read_xml(nameof(Default_cred_card1_in_out_pending_file_name));

        public static string Bank_descriptor => XmlReader.Read_xml(nameof(Bank_descriptor));
        public static string Cred_card1_descriptor => XmlReader.Read_xml(nameof(Cred_card1_descriptor));
        public static string Cred_card1_in_out_descriptor => XmlReader.Read_xml(nameof(Cred_card1_in_out_descriptor));
        public static string Cred_card2_descriptor => XmlReader.Read_xml(nameof(Cred_card2_descriptor));
        public static string Cred_card2_in_out_descriptor => XmlReader.Read_xml(nameof(Cred_card2_in_out_descriptor));
        public static string Employer_expense_description => XmlReader.Read_xml(nameof(Employer_expense_description));

        public static string Cred_card1_name => XmlReader.Read_xml(nameof(Cred_card1_name));
        public static string Cred_card2_name => XmlReader.Read_xml(nameof(Cred_card2_name));
        public static string Cred_card1_dd_description => XmlReader.Read_xml(nameof(Cred_card1_dd_description));
        public static string Cred_card2_dd_description => XmlReader.Read_xml(nameof(Cred_card2_dd_description));
        public static string Cred_card1_regular_pymt_description => XmlReader.Read_xml(nameof(Cred_card1_regular_pymt_description));
        public static string Cred_card2_regular_pymt_description => XmlReader.Read_xml(nameof(Cred_card2_regular_pymt_description));

        public static string Instructions_line_01 => XmlReader.Read_xml(nameof(Instructions_line_01));
        public static string Instructions_line_02 => XmlReader.Read_xml(nameof(Instructions_line_02));
        public static string Instructions_line_03 => XmlReader.Read_xml(nameof(Instructions_line_03));
        public static string Instructions_line_04 => XmlReader.Read_xml(nameof(Instructions_line_04));
        public static string Instructions_line_05 => XmlReader.Read_xml(nameof(Instructions_line_05));
        public static string Instructions_line_06 => XmlReader.Read_xml(nameof(Instructions_line_06));
        public static string Instructions_line_07 => XmlReader.Read_xml(nameof(Instructions_line_07));
        public static string Instructions_line_08 => XmlReader.Read_xml(nameof(Instructions_line_08));
        public static string Instructions_line_09 => XmlReader.Read_xml(nameof(Instructions_line_09));
        public static string Instructions_line_10 => XmlReader.Read_xml(nameof(Instructions_line_10));
        public static string Instructions_line_11 => XmlReader.Read_xml(nameof(Instructions_line_11));

        public static string Bank_out_header => XmlReader.Read_xml(nameof(Bank_out_header));
        public static string Bank_in_header => XmlReader.Read_xml(nameof(Bank_in_header));
        public static string Cred_card2_header => XmlReader.Read_xml(nameof(Cred_card2_header));

        public static string Load_pending_csvs => XmlReader.Read_xml(nameof(Load_pending_csvs));
        public static string Loading_expenses => XmlReader.Read_xml(nameof(Loading_expenses));

        public const string MissingCodeInWorksheet =
            "There is no \"{0}\" row in this worksheet ({1}), so we can't find the data we are looking for.";

        public const string CodeInWrongPlace =
            "The word/phrase \"{0}\" has been used somewhere unexpected in {1} (probably in a text field).";

        public const string ExpectedOutInsertionPoint = "***** EVERYTHING BELOW THIS LINE NOT INCLUDED IN TRACKING";

        public const string MissingCell = "Cell does not exist: ";

        public const string EnterMonths =
                "Enter the last month you want to plan for. \nBudget amounts will be added from {0} to the month you specify (inclusive). \nEnter 0 if no budgeting required.";

        public const string ConfirmBadMonth = "Your input was 0 or invalid. Would you like to proceed without adding monthly / annual budget amounts? Enter Y for Yes";

        public const string ConfirmMonthInterval = "Going from {0} to {1} means you will budgeting for {2} month(s). Would you like to proceed? Enter Y for Yes";

        public const string ConfirmNoMonthlyBudgeting =
            "You have chosen to continue without adding any monthly or annual budgeting amounts.";

        public const string CantFindMortgageRow = "Can't find mortgage row (used for calculating next unplanned month, for budget out). \nCheck Budget Out has mortgage description that matches Bank Out. \nIf you've just changed it, it will be fine next time. \nFor now, please enter number for the first month you would like to schedule budgeted amounts:";
        public const string DividerText = "Divider";

        public const string DefaultUnplannedMonth =
                "Your input was 0 or invalid. Using default of current month for next unplanned month. \nIf this doesn't work for you, exit and return with better input!";

        public const string LoadingDataFromPendingFile = "Loading data from pending file (which you should have already split out, if necessary)...";
        public const string MergingBudgetDataWithPendingData = "Merging budget data with pending data...";
        public const string ConvertingSourceLineSeparators = "Converting source line separators...";
        public const string MergingBespokeData = "Merging bespoke data with pending file...";
        public const string MergingUnreconciledRows = "Merging unreconciled rows from spreadsheet with pending and budget data...";
        public const string CopyingMergedData = "Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...";
        public const string StuffIsHappening = "...";
        public const string LoadingDataFromFiles = "Loading data back in from 'owned' and 'third party' files...";
        public const string CreatingReconciliationInterface = "Creating reconciliation interface...";

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