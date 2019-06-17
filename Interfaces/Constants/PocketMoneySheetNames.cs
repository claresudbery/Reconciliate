namespace Interfaces.Constants
{
    public static class PocketMoneySheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string SecondChild => XmlReader.ReadXml($"{nameof(PocketMoneySheetNames)}.{nameof(PocketMoneySheetNames.SecondChild)}");
    }
}