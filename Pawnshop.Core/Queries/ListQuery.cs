namespace Pawnshop.Core.Queries
{
    public class ListQuery
    {
        public string Filter { get; set; }

        public Page Page { get; set; } = new Page { Limit = 20 };

        public Sort Sort { get; set; }
    }
}