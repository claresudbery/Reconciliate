using ExcelIntegrationTests.TestUtils;
using Interfaces.Constants;
using NUnit.Framework;

namespace ExcelIntegrationTests
{
    [TestFixture]
    public class XmlReaderTests
    {
        [Test]
        public void Temp_Test_For_New_Config_Values()
        {
            var xmlReader = new MyXmlReader();

            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.CredCard1)}"), "CredCard1");
            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.TestRecord)}"), "TestRecord");
            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.CredCard)}"), "CredCard");
            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Bank)}"), "Bank");
            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.ActualBank)}"), "ActualBank");
            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.BadDivider)}"), "BadDivider");
            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.BudgetOut)}"), "Budget Out");
            Assert.AreEqual(xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.ExpectedOut)}"), "Expected Out");
        }

        [Test]
        public void Temp_Test_For_New_Static_Const_Values()
        {
            Assert.AreEqual(TestSheetNames.CredCard1, "CredCard1");
            Assert.AreEqual(TestSheetNames.TestRecord, "TestRecord");
            Assert.AreEqual(TestSheetNames.CredCard, "CredCard");
            Assert.AreEqual(TestSheetNames.Bank, "Bank");
            Assert.AreEqual(TestSheetNames.ActualBank, "ActualBank");
            Assert.AreEqual(TestSheetNames.BadDivider, "BadDivider");
            Assert.AreEqual(TestSheetNames.BudgetOut, "Budget Out");
            Assert.AreEqual(TestSheetNames.ExpectedOut, "Expected Out");
        }
    }
}