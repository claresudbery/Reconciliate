namespace Interfaces.Constants
{
    public static class MainSheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Bank_in => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Bank_in)}");
        public static string Bank_out => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Bank_out)}");
        public static string Cred_card1 => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Cred_card1)}");
        public static string Cred_card2 => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Cred_card2)}");
        public static string Expected_in => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Expected_in)}");
        public static string Expected_out => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Expected_out)}");
        public static string Totals => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Totals)}");
        public static string Cred_card3 => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Cred_card3)}");
        public static string Savings => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Savings)}");
        public static string Budget_in => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Budget_in)}");
        public static string Budget_out => XmlReader.Read_xml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Budget_out)}");
    }
}