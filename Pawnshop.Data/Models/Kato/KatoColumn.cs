namespace Pawnshop.Data.Models.Kato
{
    public class KatoColumn
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public KatoColumn(int value) { Value = value; }
        public KatoColumn()
        {

        }
    }
}
