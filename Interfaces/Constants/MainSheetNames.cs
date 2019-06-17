namespace Interfaces.Constants
{
    public static class MainSheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string BankIn => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BankIn)}");
        public static string BankOut => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BankOut)}");
        public static string CredCard1 => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard1)}");
        public static string CredCard2 => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard2)}");
        public static string ExpectedIn => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.ExpectedIn)}");
        public static string ExpectedOut => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.ExpectedOut)}");
        public static string Totals => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Totals)}");
        public static string CredCard3 => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard3)}");
        public static string Savings => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Savings)}");
        public static string BudgetIn => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BudgetIn)}");
        public static string BudgetOut => XmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BudgetOut)}");
    }
}