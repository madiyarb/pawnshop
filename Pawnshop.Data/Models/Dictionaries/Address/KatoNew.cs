using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class KatoNew : IEntity
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string KatoCode { get; set; }
        public int Ab { get; set; }
        public int Cd { get; set; }
        public int Ef { get; set; }
        public int Hij { get; set; }
        public int K { get; set; }
        public string NameKaz { get; set; }
        public string NameRus { get; set; }
        public int Nn { get; set; }
        public bool Mapped { get; set; } = false;
        public DateTime? DeleteDate { get; set; }
        public string Note { get; set; }
    }
}
