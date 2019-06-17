namespace Interfaces.Constants
{
    public static class PlanningSheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Expenses => XmlReader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Expenses)}");
        public static string Deposits => XmlReader.ReadXml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Deposits)}");
    }
}