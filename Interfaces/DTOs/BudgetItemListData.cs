namespace Interfaces.DTOs
{
    public class BudgetItemListData
    {
        public string Budget_sheet_name { get; set; }
        public string Owned_sheet_name { get; set; }
        public string Start_divider { get; set; }
        public string End_divider { get; set; }
        public int First_column_number { get; set; }
        public int Last_column_number { get; set; }
        public int Third_party_desc_col { get; set; }

        public static bool operator ==(BudgetItemListData lhs, BudgetItemListData rhs)
        {
            bool status = lhs?.Budget_sheet_name == rhs?.Budget_sheet_name
                    && lhs?.Start_divider == rhs?.Start_divider
                    && lhs?.End_divider == rhs?.End_divider
                    && lhs?.First_column_number == rhs?.First_column_number
                    && lhs?.Last_column_number == rhs?.Last_column_number;
            return status;
        }

        public static bool operator !=(BudgetItemListData lhs, BudgetItemListData rhs)
        {
            bool status = lhs?.Budget_sheet_name != rhs?.Budget_sheet_name
                              || lhs?.Start_divider != rhs?.Start_divider
                              || lhs?.End_divider != rhs?.End_divider
                              || lhs?.First_column_number != rhs?.First_column_number
                              || lhs?.Last_column_number != rhs?.Last_column_number;
            return status;
        }
        protected bool Equals(BudgetItemListData other)
        {
            return string.Equals(Budget_sheet_name, other.Budget_sheet_name) 
                && string.Equals(Start_divider, other.Start_divider) 
                && string.Equals(End_divider, other.End_divider) 
                && First_column_number == other.First_column_number 
                && Last_column_number == other.Last_column_number;
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
                var hash_code = (Budget_sheet_name != null ? Budget_sheet_name.GetHashCode() : 0);
                hash_code = (hash_code * 397) ^ (Start_divider != null ? Start_divider.GetHashCode() : 0);
                hash_code = (hash_code * 397) ^ (End_divider != null ? End_divider.GetHashCode() : 0);
                hash_code = (hash_code * 397) ^ First_column_number;
                hash_code = (hash_code * 397) ^ Last_column_number;
                return hash_code;
            }
        }
    }
}