using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.Leads
{
    [Table("Leads")]
    public sealed class Lead
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Phone { get; set; }

        public Lead()
        {
            
        }

        public Lead(Guid? id, string name, string surname, string patronymic, string phone)
        {
            if (id.HasValue)
            {
                Id = id.Value;
            }
            else
            {
                Id = Guid.NewGuid();
            }
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            Phone = phone;
        }

    }
}
