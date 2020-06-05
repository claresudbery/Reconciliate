using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleCatchall.Console.Reconciliation.Exceptions
{
    public class MonthlyBudgetedRowNotFoundException : Exception
    {
        public MonthlyBudgetedRowNotFoundException()
        {
        }

        public MonthlyBudgetedRowNotFoundException(string message)
            : base(message)
        {
        }

        public MonthlyBudgetedRowNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
