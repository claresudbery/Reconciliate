using Interfaces.Constants;

namespace ExcelIntegrationTests.TestUtils
{
    public static class TestSheetNames
    {
        static readonly MyXmlReader _xmlReader = new MyXmlReader();

        public static string Bank => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Bank)}");
        public static string CredCard1 => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.CredCard1)}");
        public static string CredCard => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.CredCard)}");
        public static string ExpectedOut => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.ExpectedOut)}");
        public static string BudgetOut => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.BudgetOut)}");
        public static string TestRecord => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.TestRecord)}");
        public static string BadDivider => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.BadDivider)}");
        public static string ActualBank => _xmlReader.ReadXml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.ActualBank)}");
    }
}