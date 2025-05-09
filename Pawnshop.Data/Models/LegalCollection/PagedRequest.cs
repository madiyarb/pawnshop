namespace Pawnshop.Data.Models.LegalCollection
{
    public abstract class PagedRequest
    {
        public int Page { get; set; }
        public int Size { get; set; } = 20;
    }
}