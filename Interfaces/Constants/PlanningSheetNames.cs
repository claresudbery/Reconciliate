namespace Interfaces.Constants
{
    public static class PlanningSheetNames
    {
        static readonly XmlReader _xmlReader = new XmlReader();

        public static string Expenses => _xmlReader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Expenses)}");
        public static string Deposits => _xmlReader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Deposits)}");
    }
}