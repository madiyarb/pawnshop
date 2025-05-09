namespace Pawnshop.Data.Models.Kato
{
    public class KatoRow
    {
        public string KatoCode { get; set; }
        public KatoColumn Ab { get; set; }
        public KatoColumn Cd { get; set; }
        public KatoColumn Ef { get; set; }
        public KatoColumn Hij { get; set; }
        public string NameKaz { get; set; }
        public string NameRus { get; set; }
        public int Nn { get; set; }

        public KatoRow()
        {
            KatoCode = "";
            Ab = new KatoColumn();
            Cd = new KatoColumn();
            Ef = new KatoColumn();
            Hij = new KatoColumn();
            NameKaz = "";
            NameRus = "";
            Nn = 0;
        }

        public KatoRow(string katoCode,
            KatoColumn ab,
            KatoColumn cd,
            KatoColumn ef,
            KatoColumn hij,
            string nameKaz,
            string nameRus,
            int nn)
        {
            KatoCode = katoCode;
            Ab = ab;
            Cd = cd;
            Ef = ef;
            Hij = hij;
            NameKaz = nameKaz;
            NameRus = nameRus;
            Nn = nn;
        }
    }
}
