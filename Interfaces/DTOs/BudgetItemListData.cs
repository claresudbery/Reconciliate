namespace Interfaces.DTOs
{
    public class BudgetItemListData
    {
        public string SheetName { get; set; }
        public string StartDivider { get; set; }
        public string EndDivider { get; set; }
        public int FirstColumnNumber { get; set; }
        public int LastColumnNumber { get; set; }

        public static bool operator ==(BudgetItemListData lhs, BudgetItemListData rhs)
        {
            bool status = lhs?.SheetName == rhs?.SheetName
                    && lhs?.StartDivider == rhs?.StartDivider
                    && lhs?.EndDivider == rhs?.EndDivider
                    && lhs?.FirstColumnNumber == rhs?.FirstColumnNumber
                    && lhs?.LastColumnNumber == rhs?.LastColumnNumber;
            return status;
        }

        public static bool operator !=(BudgetItemListData lhs, BudgetItemListData rhs)
        {
            bool status = lhs?.SheetName != rhs?.SheetName
                              || lhs?.StartDivider != rhs?.StartDivider
                              || lhs?.EndDivider != rhs?.EndDivider
                              || lhs?.FirstColumnNumber != rhs?.FirstColumnNumber
                              || lhs?.LastColumnNumber != rhs?.LastColumnNumber;
            return status;
        }
        protected bool Equals(BudgetItemListData other)
        {
            return string.Equals(SheetName, other.SheetName) 
                && string.Equals(StartDivider, other.StartDivider) 
                && string.Equals(EndDivider, other.EndDivider) 
                && FirstColumnNumber == other.FirstColumnNumber 
                && LastColumnNumber == other.LastColumnNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BudgetItemListData)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (SheetName != null ? SheetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StartDivider != null ? StartDivider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EndDivider != null ? EndDivider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FirstColumnNumber;
                hashCode = (hashCode * 397) ^ LastColumnNumber;
                return hashCode;
            }
        }
    }
}