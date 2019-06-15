namespace Interfaces.Constants
{
    public static class PocketMoneySheetNames
    {
        static readonly MyXmlReader _xmlReader = new MyXmlReader();

        public static string SecondChild => _xmlReader.ReadXml($"{nameof(PocketMoneySheetNames)}.{nameof(PocketMoneySheetNames.SecondChild)}");
    }
}