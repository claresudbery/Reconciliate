namespace Interfaces.Constants
{
    public static class PlanningSheetNames
    {
        static readonly MyXmlReader _xmlReader = new MyXmlReader();

        public static string Expenses => _xmlReader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Expenses)}");
        public static string Deposits => _xmlReader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Deposits)}");
    }
}