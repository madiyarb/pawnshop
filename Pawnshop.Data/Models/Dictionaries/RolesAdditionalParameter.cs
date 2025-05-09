using System;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class RolesAdditionalParameter
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int RoleId { get; set; }
        public bool CanSelectForOnlineFunction { get; set; }
        public bool IsManager { get; set; }
        public bool IsVerificator { get; set; }
    }
}
