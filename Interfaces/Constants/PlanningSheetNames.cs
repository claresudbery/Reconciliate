namespace Interfaces.Constants
{
    public static class PlanningSheetNames
    {
        static readonly MyXmlReader XmlReader = new MyXmlReader();

        public static string Expenses => XmlReader.Read_xml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Expenses)}");
        public static string Deposits => XmlReader.Read_xml($"{nameof(PlanningSheetNames)}.{nameof(PlanningSheetNames.Deposits)}");
    }
}