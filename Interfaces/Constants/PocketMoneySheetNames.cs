namespace Interfaces.Constants
{
    public static class PocketMoneySheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Second_child => XmlReader.Read_xml($"{nameof(PocketMoneySheetNames)}.{nameof(PocketMoneySheetNames.Second_child)}");
    }
}