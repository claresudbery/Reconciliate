namespace Interfaces.Constants
{
    public static class MainSheetNames
    {
        static readonly MyXmlReader _xmlReader = new MyXmlReader();

        public static string BankIn => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BankIn)}");
        public static string BankOut => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BankOut)}");
        public static string CredCard1 => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard1)}");
        public static string CredCard2 => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard2)}");
        public static string ExpectedIn => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.ExpectedIn)}");
        public static string ExpectedOut => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.ExpectedOut)}");
        public static string Totals => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Totals)}");
        public static string CredCard3 => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.CredCard3)}");
        public static string Savings => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.Savings)}");
        public static string BudgetIn => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BudgetIn)}");
        public static string BudgetOut => _xmlReader.ReadXml($"{nameof(MainSheetNames)}.{nameof(MainSheetNames.BudgetOut)}");
    }
}