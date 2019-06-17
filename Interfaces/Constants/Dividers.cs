namespace Interfaces.Constants
{
    public static class Dividers
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Divider_text => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Divider_text)}");
        public static string Expenses => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Expenses)}");
        public static string Expenses_total => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Expenses_total)}");
        public static string Sodds => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Sodds)}");
        public static string Cred_card1 => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Cred_card1)}");
        public static string Cred_card2 => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Cred_card2)}");
        public static string SODD_total => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.SODD_total)}");
        public static string Annual_sodds => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Annual_sodds)}");
        public static string Annual_total => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Annual_total)}");
        public static string Date => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Date)}");
        public static string Total => XmlReader.Read_xml($"{nameof(Dividers)}.{nameof(Dividers.Total)}");
    }
}