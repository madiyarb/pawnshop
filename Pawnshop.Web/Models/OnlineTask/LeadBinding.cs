using System;

namespace Pawnshop.Web.Models.OnlineTask
{
    public class LeadBinding
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Phone { get; set; }
    }
}
