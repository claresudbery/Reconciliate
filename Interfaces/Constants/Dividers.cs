namespace Interfaces.Constants
{
    public static class Dividers
    {
        static readonly XmlReader _xmlReader = new XmlReader();

        public static string DividerText => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.DividerText)}");
        public static string Expenses => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Expenses)}");
        public static string ExpensesTotal => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.ExpensesTotal)}");
        public static string SODDs => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.SODDs)}");
        public static string CredCard1 => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.CredCard1)}");
        public static string CredCard2 => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.CredCard2)}");
        public static string SODDTotal => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.SODDTotal)}");
        public static string AnnualSODDs => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.AnnualSODDs)}");
        public static string AnnualTotal => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.AnnualTotal)}");
        public static string Date => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Date)}");
        public static string Total => _xmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Total)}");
    }
}