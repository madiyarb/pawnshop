using System;

namespace Pawnshop.Web.Models.Reports
{
    public class NumericCompare
    {
        public int Value { get; set; }

        public CompareOperator Operator { get; set; }

        public string DisplayOperator
        {
            get
            {
                switch (Operator)
                {
                    case CompareOperator.Less:
                        return "<";
                    case CompareOperator.Equal:
                        return "=";
                    case CompareOperator.NotEqual:
                        return "<>";
                    case CompareOperator.Greater:
                        return ">";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}