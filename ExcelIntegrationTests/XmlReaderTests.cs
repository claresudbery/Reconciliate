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
            var xml_reader = new MyXmlReader();

            Assert.AreEqual(xml_reader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Cred_card1)}"), "CredCard1");
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Test_record)}"), "TestRecord");
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Cred_card)}"), "CredCard");
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Bank)}"), "Bank");
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Bad_divider)}"), "BadDivider");
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Budget_out)}"), "Budget Out");
            Assert.AreEqual(xml_reader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Expected_out)}"), "Expected Out");
        }

        [Test]
        public void Temp_Test_For_New_Static_Const_Values()
        {
            Assert.AreEqual(TestSheetNames.Cred_card1, "CredCard1");
            Assert.AreEqual(TestSheetNames.Test_record, "TestRecord");
            Assert.AreEqual(TestSheetNames.Cred_card, "CredCard");
            Assert.AreEqual(TestSheetNames.Bank, "Bank");
            Assert.AreEqual(TestSheetNames.Bad_divider, "BadDivider");
            Assert.AreEqual(TestSheetNames.Budget_out, "Budget Out");
            Assert.AreEqual(TestSheetNames.Expected_out, "Expected Out");
        }
    }
}