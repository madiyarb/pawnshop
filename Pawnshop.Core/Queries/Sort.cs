namespace Pawnshop.Core.Queries
{
    public class Sort
    {
        public Sort()
        {
        }

        public Sort(string name, SortDirection direction)
        {
            Name = name;
            Direction = direction;
        }

        public string Name { get; set; }

        public SortDirection Direction { get; set; }
    }
}