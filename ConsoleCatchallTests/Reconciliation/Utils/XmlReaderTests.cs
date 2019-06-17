using System.IO;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces.Constants;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Utils
{
    [TestFixture]
    public class MyXmlReaderTests
    {
        private string _sampleXmlConfigFilePath;

        public MyXmlReaderTests()
        {
            string current_path = TestContext.CurrentContext.TestDirectory;
            _sampleXmlConfigFilePath = Path.Combine(TestHelper.FullyQualifiedSpreadsheetFilePath(current_path), FilePathConsts.SampleConfigFileName);
        }

        [Test]
        public void Can_Read_Path_From_Config()
        {
            var xml_reader = new MyXmlReader();
            Assert.AreEqual("This value should always be this string.", xml_reader.ReadXml(nameof(ReconConsts.TestValueDoNotChange), _sampleXmlConfigFilePath));
        }

        [TestCase(nameof(ReconConsts.MainSpreadsheetPath), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging")]
        [TestCase(nameof(ReconConsts.DefaultFilePath), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging")]
        [TestCase(nameof(ReconConsts.MainSpreadsheetFileName), @"Your-Spreadsheet.xlsx")]
        [TestCase(nameof(ReconConsts.TestBackupFilePath), @"C:/Temp/ManualTesting/TestSpreadsheetBackups")]
        [TestCase(nameof(ReconConsts.SourceDebugSpreadsheetPath), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging")]
        [TestCase(nameof(ReconConsts.BackupSubFolder), "SpreadsheetBackups")]
        [TestCase(nameof(ReconConsts.DebugSpreadsheetFileName), @"debug-backup-spreadsheet.xlsx")]
        [TestCase(nameof(ReconConsts.PocketMoneySpreadsheetPath), @"C:/Development/Reconciliate/reconciliation-samples/For-debugging/Pocket-Money-Spreadsheet.xlsx")]
        [TestCase(nameof(ReconConsts.FiveFileOptions), "1.EnterFile, 2.CredCard1InOut, 3.CredCard2InOut, 4.BankIn, 5.BankOut")]
        [TestCase(nameof(ReconConsts.FourAccountingTypes), "1.CredCard1, 2.CredCard2, 3.ActualBankIn, 4.ActualBankOut")]
        [TestCase(nameof(ReconConsts.FourFileNameOptions), "1.EnterFileName, 2.CredCard1File, 3.CredCard2File, 4.ActualBankFile")]
        [TestCase(nameof(ReconConsts.FiveFileDetails), "1.OwnFileDetails, 2.CredCard1, 3.CredCard2, 4.ActualBankIn, 5.ActualBankOut")]
        [TestCase(nameof(ReconConsts.File_Option_02), "2. CredCard1 InOut default ({0}.csv) (3rd party format will be CredCard1)")]
        [TestCase(nameof(ReconConsts.File_Option_03), "3. CredCard2 InOut default ({0}.csv) (3rd party format will be CredCard2)")]
        [TestCase(nameof(ReconConsts.File_Option_04), "4. Bank In default ({0}.csv) (3rd party format will be ActualBank)")]
        [TestCase(nameof(ReconConsts.File_Option_05), "5. Bank Out default ({0}.csv) (3rd party format will be ActualBank)")]
        [TestCase(nameof(ReconConsts.Accounting_Type_01), "1. CredCard1 and CredCard1 InOut")]
        [TestCase(nameof(ReconConsts.Accounting_Type_02), "2. CredCard2 and CredCard2 InOut")]
        [TestCase(nameof(ReconConsts.Accounting_Type_03), "3. ActualBank and Bank In")]
        [TestCase(nameof(ReconConsts.Accounting_Type_04), "4. ActualBank and Bank Out")]
        [TestCase(nameof(ReconConsts.File_Name_Option_02), "2. Use the default for CredCard1 ({0}.csv)")]
        [TestCase(nameof(ReconConsts.File_Name_Option_03), "3. Use the default for CredCard2 ({0}.csv)")]
        [TestCase(nameof(ReconConsts.File_Name_Option_04), "4. Use the default for ActualBank ({0}.csv)")]
        [TestCase(nameof(ReconConsts.File_Details_02), "2. Use defaults for CredCard1 and CredCard1 InOut")]
        [TestCase(nameof(ReconConsts.File_Details_03), "3. Use defaults for CredCard2 and CredCard2 InOut")]
        [TestCase(nameof(ReconConsts.File_Details_04), "4. Use defaults for ActualBank and Bank In")]
        [TestCase(nameof(ReconConsts.File_Details_05), "5. Use defaults for ActualBank and Bank Out")]
        [TestCase(nameof(ReconConsts.DefaultBankFileName), "ActualBank")]
        [TestCase(nameof(ReconConsts.DefaultCredCard1FileName), "CredCard1")]
        [TestCase(nameof(ReconConsts.DefaultCredCard2FileName), "CredCard2")]
        [TestCase(nameof(ReconConsts.DefaultCredCard1InOutFileName), "CredCard1InOut")]
        [TestCase(nameof(ReconConsts.DefaultCredCard2InOutFileName), "CredCard2InOut")]
        [TestCase(nameof(ReconConsts.DefaultCredCard2InOutPendingFileName), "CredCard2InOutPending")]
        [TestCase(nameof(ReconConsts.DefaultCredCard1InOutPendingFileName), "CredCard1InOutPending")]
        [TestCase(nameof(ReconConsts.BankDescriptor), "ActualBank")]
        [TestCase(nameof(ReconConsts.CredCard1Descriptor), "CredCard1")]
        [TestCase(nameof(ReconConsts.CredCard1InOutDescriptor), "CredCard1 InOut")]
        [TestCase(nameof(ReconConsts.CredCard2Descriptor), "CredCard2")]
        [TestCase(nameof(ReconConsts.CredCard2InOutDescriptor), "CredCard2 InOut")]
        [TestCase(nameof(ReconConsts.EmployerExpenseDescription), "ACME LTD")]
        [TestCase(nameof(ReconConsts.CredCard1Name), "CredCard1")]
        [TestCase(nameof(ReconConsts.CredCard2Name), "CredCard2")]
        [TestCase(nameof(ReconConsts.CredCard1DdDescription), "CREDIT CARD 1")]
        [TestCase(nameof(ReconConsts.CredCard2DdDescription), "CREDIT CARD 2")]
        [TestCase(nameof(ReconConsts.CredCard1RegularPymtDescription), "CRED CARD 1 PAYMENT DESCRIPTION ON STATEMENTS")]
        [TestCase(nameof(ReconConsts.CredCard2RegularPymtDescription), "CRED CARD 2 PAYMENT DESCRIPTION ON STATEMENTS")]
        [TestCase(nameof(ReconConsts.Instructions_Line_01), "  (See ReconciliationProcess.txt for full process, and Trello for current bugs)")]
        [TestCase(nameof(ReconConsts.Instructions_Line_02), "  Go to your third party website and download a statement as a csv file.")]
        [TestCase(nameof(ReconConsts.Instructions_Line_03), "  Name it ActualBank.csv, CredCard1.csv or CredCard2.csv.")]
        [TestCase(nameof(ReconConsts.Instructions_Line_04), "  Create csvs of pending transactions, named Pending.txt and CredCard1InOutPending.csv.")]
        [TestCase(nameof(ReconConsts.Instructions_Line_05), "  !! For Bank In and Bank Out, check Type columns before starting !!")]
        [TestCase(nameof(ReconConsts.Instructions_Line_06), "  Check VERY carefully - it's really easy to miss an example of this.")]
        [TestCase(nameof(ReconConsts.Instructions_Line_07), "  This is because if the description is in the Type column, you'll get errors.")]
        [TestCase(nameof(ReconConsts.Instructions_Line_08), "  But you won't see those errors until you're halfway through processing")]
        [TestCase(nameof(ReconConsts.Instructions_Line_09), "  the file, and then you'll lose all your work.")]
        [TestCase(nameof(ReconConsts.Instructions_Line_10), "  Run the 'load pending csvs' step before starting reconciliation.")]
        [TestCase(nameof(ReconConsts.Instructions_Line_11), "  Follow the on-screen instructions.")]
        [TestCase(nameof(ReconConsts.BankOutHeader), "Bank Out")]
        [TestCase(nameof(ReconConsts.BankInHeader), "Bank In")]
        [TestCase(nameof(ReconConsts.CredCard2Header), "CredCard2")]
        [TestCase(nameof(ReconConsts.LoadPendingCsvs), "1. Load pending csvs for CredCard2, Bank In and Bank Out (from phone).")]
        [TestCase(nameof(ReconConsts.LoadingExpenses), "Loading ACME expenses from Expected In...")]
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
        [TestCase(nameof(Codes.BankBal), "ActualBank Balance")]
        [TestCase(nameof(Codes.CredCard1Bal), "CredCard1 Balance")]
        [TestCase(nameof(Codes.CredCard2Bal), "CredCard2 Balance")]
        [TestCase(nameof(Codes.Expenses), "Acme Expenses")]
        public void Test_Config_Values_All_Present(string elementName, string elementValue)
        {
            var xml_reader = new MyXmlReader();
            Assert.AreEqual(elementValue, xml_reader.ReadXml(elementName, _sampleXmlConfigFilePath));
        }

        [Test]
        public void Temp_Test_For_New_Config_Values_With_Compound_Names()
        {
            var xml_reader = new MyXmlReader();

            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BudgetOut)}", _sampleXmlConfigFilePath), "Budget Out", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BudgetIn)}", _sampleXmlConfigFilePath), "Budget In", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.ExpectedOut)}", _sampleXmlConfigFilePath), "Expected Out", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.ExpectedIn)}", _sampleXmlConfigFilePath), "Expected In", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Totals)}", _sampleXmlConfigFilePath), "Totals", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Savings)}", _sampleXmlConfigFilePath), "Savings", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BankOut)}", _sampleXmlConfigFilePath), "Bank Out", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BankIn)}", _sampleXmlConfigFilePath), "Bank In", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard1)}", _sampleXmlConfigFilePath), "CredCard1", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard2)}", _sampleXmlConfigFilePath), "CredCard2", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard3)}", _sampleXmlConfigFilePath), "CredCard3", _sampleXmlConfigFilePath);

            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.DividerText)}", _sampleXmlConfigFilePath), "divider", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Expenses)}", _sampleXmlConfigFilePath), "Expenses", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.ExpensesTotal)}", _sampleXmlConfigFilePath), "Expenses Total", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.SODDs)}", _sampleXmlConfigFilePath), "SODDs", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.CredCard1)}", _sampleXmlConfigFilePath), "CredCard1 cred card", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.CredCard2)}", _sampleXmlConfigFilePath), "CredCard2 cred card", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.SODDTotal)}", _sampleXmlConfigFilePath), "SODDTotal", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.AnnualSODDs)}", _sampleXmlConfigFilePath), "AnnualSODDs", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.AnnualTotal)}", _sampleXmlConfigFilePath), "AnnualTotal", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Date)}", _sampleXmlConfigFilePath), "Date", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Total)}", _sampleXmlConfigFilePath), "Total", _sampleXmlConfigFilePath);

            Assert.AreEqual(xml_reader.ReadXml($"{nameof(PocketMoneySheetNames)}.{nameof(PocketMoneySheetNames.SecondChild)}", _sampleXmlConfigFilePath), "SecondChild", _sampleXmlConfigFilePath);

            Assert.AreEqual(xml_reader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Expenses)}", _sampleXmlConfigFilePath), "Expenses", _sampleXmlConfigFilePath);
            Assert.AreEqual(xml_reader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Deposits)}", _sampleXmlConfigFilePath), "Deposits", _sampleXmlConfigFilePath);
        }
    }
}