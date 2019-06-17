using Interfaces.Constants;

namespace ExcelIntegrationTests.TestUtils
{
    public static class TestSheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Bank => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Bank)}");
        public static string CredCard1 => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.CredCard1)}");
        public static string CredCard => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.CredCard)}");
        public static string ExpectedOut => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.ExpectedOut)}");
        public static string BudgetOut => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.BudgetOut)}");
        public static string TestRecord => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.TestRecord)}");
        public static string BadDivider => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.BadDivider)}");
        public static string ActualBank => XmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.ActualBank)}");
    }
}