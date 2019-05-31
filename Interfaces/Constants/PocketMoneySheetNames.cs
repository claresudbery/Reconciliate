namespace Interfaces.Constants
{
    public static class PocketMoneySheetNames
    {
        static readonly XmlReader _xmlReader = new XmlReader();

        public static string SecondChild => _xmlReader.ReadXml($"{nameof(PocketMoneySheetNames)}.{nameof(PocketMoneySheetNames.SecondChild)}");
    }
}