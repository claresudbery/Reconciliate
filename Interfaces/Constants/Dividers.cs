namespace Interfaces.Constants
{
    public static class Dividers
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string DividerText => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.DividerText)}");
        public static string Expenses => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Expenses)}");
        public static string ExpensesTotal => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.ExpensesTotal)}");
        public static string SODDs => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.SODDs)}");
        public static string CredCard1 => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.CredCard1)}");
        public static string CredCard2 => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.CredCard2)}");
        public static string SODDTotal => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.SODDTotal)}");
        public static string AnnualSODDs => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.AnnualSODDs)}");
        public static string AnnualTotal => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.AnnualTotal)}");
        public static string Date => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Date)}");
        public static string Total => XmlReader.ReadXml($"{nameof(Dividers)}.{nameof(Dividers.Total)}");
    }
}