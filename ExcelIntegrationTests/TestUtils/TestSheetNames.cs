using Interfaces.Constants;

namespace ExcelIntegrationTests.TestUtils
{
    public static class TestSheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Bank => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Bank)}");
        public static string Cred_card1 => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Cred_card1)}");
        public static string Cred_card => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Cred_card)}");
        public static string Expected_out => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Expected_out)}");
        public static string Budget_out => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Budget_out)}");
        public static string Test_record => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Test_record)}");
        public static string Bad_divider => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Bad_divider)}");
        public static string Actual_bank => XmlReader.Read_xml($"{nameof(TestSheetNames)}.{nameof(TestSheetNames.Actual_bank)}");
    }
}