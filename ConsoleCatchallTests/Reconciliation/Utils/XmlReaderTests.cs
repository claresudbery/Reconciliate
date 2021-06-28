using System.IO;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces.Constants;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Utils
{
    [TestFixture]
    public class XmlReaderTests
    {
        private string _sample_xml_config_file_path;

        public XmlReaderTests()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;
            _sample_xml_config_file_path = Path.Combine(TestHelper.Fully_qualified_spreadsheet_file_path(current_path), FilePathConsts.SampleConfigFileName);
        }

        [Test]
        public void Can_Read_Path_From_Config()
        {
            var xml_reader = new MyXmlReader();
            Assert.AreEqual("This value should always be this string.", xml_reader.Read_xml(nameof(ReconConsts.Test_value_do_not_change), _sample_xml_config_file_path));
        }

        [TestCase(nameof(ReconConsts.Main_spreadsheet_path), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging")]
        [TestCase(nameof(ReconConsts.Default_file_path), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging")]
        [TestCase(nameof(ReconConsts.Main_spreadsheet_file_name), @"Your-Spreadsheet.xlsx")]
        [TestCase(nameof(ReconConsts.Test_backup_file_path), @"C:/Temp/ManualTesting/TestSpreadsheetBackups")]
        [TestCase(nameof(ReconConsts.Source_debug_spreadsheet_path), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging")]
        [TestCase(nameof(ReconConsts.Backup_sub_folder), "SpreadsheetBackups")]
        [TestCase(nameof(ReconConsts.Debug_spreadsheet_file_name), @"debug-backup-spreadsheet.xlsx")]
        [TestCase(nameof(ReconConsts.Pocket_money_spreadsheet_path), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging/Pocket-Money-Spreadsheet.xlsx")]
        [TestCase(nameof(ReconConsts.Five_file_options), "1.EnterFile, 2.CredCard1InOut, 3.CredCard2InOut, 4.BankIn, 5.BankOut")]
        [TestCase(nameof(ReconConsts.Four_accounting_types), "1.CredCard1, 2.CredCard2, 3.ActualBankIn, 4.ActualBankOut")]
        [TestCase(nameof(ReconConsts.Four_file_name_options), "1.EnterFileName, 2.CredCard1File, 3.CredCard2File, 4.ActualBankFile")]
        [TestCase(nameof(ReconConsts.Five_file_details), "1.OwnFileDetails, 2.CredCard1, 3.CredCard2, 4.ActualBankIn, 5.ActualBankOut")]
        [TestCase(nameof(ReconConsts.File_option_02), "2. CredCard1 InOut default ({0}.csv) (3rd party format will be CredCard1)")]
        [TestCase(nameof(ReconConsts.File_option_03), "3. CredCard2 InOut default ({0}.csv) (3rd party format will be CredCard2)")]
        [TestCase(nameof(ReconConsts.File_option_04), "4. Bank In default ({0}.csv) (3rd party format will be ActualBank)")]
        [TestCase(nameof(ReconConsts.File_option_05), "5. Bank Out default ({0}.csv) (3rd party format will be ActualBank)")]
        [TestCase(nameof(ReconConsts.Accounting_type_01), "1. CredCard1 and CredCard1 InOut")]
        [TestCase(nameof(ReconConsts.Accounting_type_02), "2. CredCard2 and CredCard2 InOut")]
        [TestCase(nameof(ReconConsts.Accounting_type_03), "3. ActualBank and Bank In")]
        [TestCase(nameof(ReconConsts.Accounting_type_04), "4. ActualBank and Bank Out")]
        [TestCase(nameof(ReconConsts.File_name_option_02), "2. Use the default for CredCard1 ({0}.csv)")]
        [TestCase(nameof(ReconConsts.File_name_option_03), "3. Use the default for CredCard2 ({0}.csv)")]
        [TestCase(nameof(ReconConsts.File_name_option_04), "4. Use the default for ActualBank ({0}.csv)")]
        [TestCase(nameof(ReconConsts.File_details_02), "2. Use defaults for CredCard1 and CredCard1 InOut")]
        [TestCase(nameof(ReconConsts.File_details_03), "3. Use defaults for CredCard2 and CredCard2 InOut")]
        [TestCase(nameof(ReconConsts.File_details_04), "4. Use defaults for ActualBank and Bank In")]
        [TestCase(nameof(ReconConsts.File_details_05), "5. Use defaults for ActualBank and Bank Out")]
        [TestCase(nameof(ReconConsts.Default_bank_file_name), "ActualBank")]
        [TestCase(nameof(ReconConsts.Default_cred_card1_file_name), "CredCard1")]
        [TestCase(nameof(ReconConsts.Default_cred_card2_file_name), "CredCard2")]
        [TestCase(nameof(ReconConsts.Default_cred_card1_in_out_file_name), "CredCard1InOut")]
        [TestCase(nameof(ReconConsts.Default_cred_card2_in_out_file_name), "CredCard2InOut")]
        [TestCase(nameof(ReconConsts.Default_cred_card2_in_out_pending_file_name), "CredCard2InOutPending")]
        [TestCase(nameof(ReconConsts.Default_cred_card1_in_out_pending_file_name), "CredCard1InOutPending")]
        [TestCase(nameof(ReconConsts.Bank_descriptor), "ActualBank")]
        [TestCase(nameof(ReconConsts.Cred_card1_descriptor), "CredCard1")]
        [TestCase(nameof(ReconConsts.Cred_card1_in_out_descriptor), "CredCard1 InOut")]
        [TestCase(nameof(ReconConsts.Cred_card2_descriptor), "CredCard2")]
        [TestCase(nameof(ReconConsts.Cred_card2_in_out_descriptor), "CredCard2 InOut")]
        [TestCase(nameof(ReconConsts.Employer_expense_description), "ACME LTD")]
        [TestCase(nameof(ReconConsts.Amazon_description), "AMAZON")]
        [TestCase(nameof(ReconConsts.iTunes_description), ".com/Bill")]
        [TestCase(nameof(ReconConsts.Asda_description), "ASDA")]
        [TestCase(nameof(ReconConsts.Cred_card1_name), "CredCard1")]
        [TestCase(nameof(ReconConsts.Cred_card2_name), "CredCard2")]
        [TestCase(nameof(ReconConsts.Cred_card1_dd_description), "CREDIT CARD 1")]
        [TestCase(nameof(ReconConsts.Cred_card2_dd_description), "CREDIT CARD 2")]
        [TestCase(nameof(ReconConsts.Cred_card1_regular_pymt_description), "CRED CARD 1 PAYMENT DESCRIPTION ON STATEMENTS")]
        [TestCase(nameof(ReconConsts.Cred_card2_regular_pymt_description), "CRED CARD 2 PAYMENT DESCRIPTION ON STATEMENTS")]
        [TestCase(nameof(ReconConsts.Instructions_line_01), "  (See ReconciliationProcess.txt for full process, and Trello for current bugs)")]
        [TestCase(nameof(ReconConsts.Instructions_line_02), "  Go to your third party website and download a statement as a csv file.")]
        [TestCase(nameof(ReconConsts.Instructions_line_03), "  Name it ActualBank.csv, CredCard1.csv or CredCard2.csv.")]
        [TestCase(nameof(ReconConsts.Instructions_line_04), "  Create csvs of pending transactions, named Pending.txt and CredCard1InOutPending.csv.")]
        [TestCase(nameof(ReconConsts.Instructions_line_05), "  !! For Bank In and Bank Out, check Type columns before starting !!")]
        [TestCase(nameof(ReconConsts.Instructions_line_06), "  Check VERY carefully - it's really easy to miss an example of this.")]
        [TestCase(nameof(ReconConsts.Instructions_line_07), "  This is because if the description is in the Type column, you'll get errors.")]
        [TestCase(nameof(ReconConsts.Instructions_line_08), "  But you won't see those errors until you're halfway through processing")]
        [TestCase(nameof(ReconConsts.Instructions_line_09), "  the file, and then you'll lose all your work.")]
        [TestCase(nameof(ReconConsts.Instructions_line_10), "  Run the 'load pending csvs' step before starting reconciliation.")]
        [TestCase(nameof(ReconConsts.Instructions_line_11), "  Follow the on-screen instructions.")]
        [TestCase(nameof(ReconConsts.Bank_out_header), "Bank Out")]
        [TestCase(nameof(ReconConsts.Bank_in_header), "Bank In")]
        [TestCase(nameof(ReconConsts.Cred_card2_header), "CredCard2")]
        [TestCase(nameof(ReconConsts.Load_pending_csvs), "1. Load pending csvs for CredCard2, Bank In and Bank Out (from phone).")]
        [TestCase(nameof(ReconConsts.Loading_expenses), "Loading ACME expenses from Expected In...")]
        [TestCase(nameof(Codes.Code001), "Code001")]
        [TestCase(nameof(Codes.Code002), "Code002")]
        [TestCase(nameof(Codes.Code003), "Code003")]
        [TestCase(nameof(Codes.Code004), "Code004")]
        [TestCase(nameof(Codes.Code005), "Code005")]
        [TestCase(nameof(Codes.Code006), "Code006")]
        [TestCase(nameof(Codes.Code007), "Code007")]
        [TestCase(nameof(Codes.Code008), "Code008")]
        [TestCase(nameof(Codes.Code009), "Code009")]
        [TestCase(nameof(Codes.Code010), "Code010")]
        [TestCase(nameof(Codes.Code011), "Code011")]
        [TestCase(nameof(Codes.Code012), "Code012")]
        [TestCase(nameof(Codes.Code013), "Code013")]
        [TestCase(nameof(Codes.Code014), "Code014")]
        [TestCase(nameof(Codes.Code015), "Code015")]
        [TestCase(nameof(Codes.Code016), "Code016")]
        [TestCase(nameof(Codes.Code017), "Code017")]
        [TestCase(nameof(Codes.Code018), "Code018")]
        [TestCase(nameof(Codes.Code019), "Code019")]
        [TestCase(nameof(Codes.Code020), "Code020")]
        [TestCase(nameof(Codes.Code021), "Code021")]
        [TestCase(nameof(Codes.Code022), "Code022")]
        [TestCase(nameof(Codes.Code023), "Code023")]
        [TestCase(nameof(Codes.Code024), "Code024")]
        [TestCase(nameof(Codes.Code025), "Code025")]
        [TestCase(nameof(Codes.Code026), "Code026")]
        [TestCase(nameof(Codes.Code027), "Code027")]
        [TestCase(nameof(Codes.Code028), "Code028")]
        [TestCase(nameof(Codes.Code029), "Code029")]
        [TestCase(nameof(Codes.Code030), "Code030")]
        [TestCase(nameof(Codes.Code031), "Code031")]
        [TestCase(nameof(Codes.Code032), "Code032")]
        [TestCase(nameof(Codes.Code033), "Code033")]
        [TestCase(nameof(Codes.Code034), "Code034")]
        [TestCase(nameof(Codes.Code035), "Code035")]
        [TestCase(nameof(Codes.Code036), "Code036")]
        [TestCase(nameof(Codes.Code037), "Code037")]
        [TestCase(nameof(Codes.Code038), "Code038")]
        [TestCase(nameof(Codes.Code039), "Code039")]
        [TestCase(nameof(Codes.Code040), "Code040")]
        [TestCase(nameof(Codes.Code041), "Code041")]
        [TestCase(nameof(Codes.Code042), "Code042")]
        [TestCase(nameof(Codes.Code043), "Code043")]
        [TestCase(nameof(Codes.Code044), "Code044")]
        [TestCase(nameof(Codes.Code045), "Code045")]
        [TestCase(nameof(Codes.Code046), "Code046")]
        [TestCase(nameof(Codes.Code047), "Code047")]
        [TestCase(nameof(Codes.Code048), "Code048")]
        [TestCase(nameof(Codes.Code049), "Code049")]
        [TestCase(nameof(Codes.Code050), "Code050")]
        [TestCase(nameof(Codes.Code051), "Code051")]
        [TestCase(nameof(Codes.Code052), "Code052")]
        [TestCase(nameof(Codes.Code053), "Code053")]
        [TestCase(nameof(Codes.Code054), "Code054")]
        [TestCase(nameof(Codes.Code055), "Code055")]
        [TestCase(nameof(Codes.Code056), "Code056")]
        [TestCase(nameof(Codes.Code057), "Code057")]
        [TestCase(nameof(Codes.Code058), "Code058")]
        [TestCase(nameof(Codes.Code059), "Code059")]
        [TestCase(nameof(Codes.Code060), "Code060")]
        [TestCase(nameof(Codes.Code061), "Code061")]
        [TestCase(nameof(Codes.Code062), "Code062")]
        [TestCase(nameof(Codes.Code063), "Code063")]
        [TestCase(nameof(Codes.Code064), "Code064")]
        [TestCase(nameof(Codes.Code065), "Code065")]
        [TestCase(nameof(Codes.Code066), "Code066")]
        [TestCase(nameof(Codes.Code067), "Code067")]
        [TestCase(nameof(Codes.Code068), "Code068")]
        [TestCase(nameof(Codes.Code069), "Code069")]
        [TestCase(nameof(Codes.Code070), "Code070")]
        [TestCase(nameof(Codes.Code071), "Code071")]
        [TestCase(nameof(Codes.Code072), "Code072")]
        [TestCase(nameof(Codes.Code073), "Code073")]
        [TestCase(nameof(Codes.Code074), "Code074")]
        [TestCase(nameof(Codes.Code075), "Code075")]
        [TestCase(nameof(Codes.Code076), "Code076")]
        [TestCase(nameof(Codes.Code077), "Code077")]
        [TestCase(nameof(Codes.Code078), "Code078")]
        [TestCase(nameof(Codes.Code079), "Code079")]
        [TestCase(nameof(Codes.Bank_bal), "ActualBank Balance")]
        [TestCase(nameof(Codes.Cred_card1_bal), "CredCard1 Balance")]
        [TestCase(nameof(Codes.Cred_card2_bal), "CredCard2 Balance")]
        [TestCase(nameof(Codes.Expenses), "Acme Expenses")]
        [TestCase(nameof(Codes.ExpectedInBankTransaction), "Bank Transaction")]
        public void All_Config_Values_Should_Be_In_Config(string element_name, string element_value)
        {
            var xml_reader = new MyXmlReader();
            Assert.AreEqual(element_value, xml_reader.Read_xml(element_name, _sample_xml_config_file_path));
        }

        [Test]
        public void Temp_Test_For_New_Config_Values_With_Compound_Names()
        {
            var xml_reader = new MyXmlReader();

            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Budget_out)}", _sample_xml_config_file_path), "Budget Out", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Budget_in)}", _sample_xml_config_file_path), "Budget In", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Expected_out)}", _sample_xml_config_file_path), "Expected Out", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Expected_in)}", _sample_xml_config_file_path), "Expected In", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Totals)}", _sample_xml_config_file_path), "Totals", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Savings)}", _sample_xml_config_file_path), "Savings", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Bank_out)}", _sample_xml_config_file_path), "Bank Out", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Bank_in)}", _sample_xml_config_file_path), "Bank In", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Cred_card1)}", _sample_xml_config_file_path), "CredCard1", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Cred_card2)}", _sample_xml_config_file_path), "CredCard2", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Cred_card3)}", _sample_xml_config_file_path), "CredCard3", _sample_xml_config_file_path);

            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Divider_text)}", _sample_xml_config_file_path), "divider", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Expenses)}", _sample_xml_config_file_path), "Expenses", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Expenses_total)}", _sample_xml_config_file_path), "Expenses Total", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Sodds)}", _sample_xml_config_file_path), "SODDs", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Cred_card1)}", _sample_xml_config_file_path), "CredCard1 cred card", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Cred_card2)}", _sample_xml_config_file_path), "CredCard2 cred card", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.SODD_total)}", _sample_xml_config_file_path), "SODDTotal", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Annual_sodds)}", _sample_xml_config_file_path), "AnnualSODDs", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Annual_total)}", _sample_xml_config_file_path), "AnnualTotal", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Date)}", _sample_xml_config_file_path), "Date", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Total)}", _sample_xml_config_file_path), "Total", _sample_xml_config_file_path);

            Assert.AreEqual(xml_reader.Read_xml($"{nameof(PocketMoneySheetNames)}.{nameof(PocketMoneySheetNames.Second_child)}", _sample_xml_config_file_path), "SecondChild", _sample_xml_config_file_path);

            Assert.AreEqual(xml_reader.Read_xml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Expenses)}", _sample_xml_config_file_path), "Expenses", _sample_xml_config_file_path);
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Deposits)}", _sample_xml_config_file_path), "Deposits", _sample_xml_config_file_path);
        }
    }
}