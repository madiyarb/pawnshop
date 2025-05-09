using System;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Models.LegalCollection.DocumentType.HttpServie;

namespace Pawnshop.Data.Models.SUSNStatuses
{
    [Table("SUSNStatuses")]
    public sealed class SUSNStatus
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string Name { get; set; }
        public string NameKz { get; set; }
        public string Code { get; set; }
        public bool Permanent { get; set; }
        public bool Decline { get; set; }

        public SUSNStatus()
        {
            
        }

        public SUSNStatus(string name, string nameKz, string code, bool permanent, bool decline)
        {
            CreateDate = DateTime.Now;
            Name = name;
            NameKz = nameKz;
            Code = code;
            Permanent = permanent;
            Decline = decline;
        }

        public void Update(string name, string nameKz, string code, bool permanent, bool decline)
        {
            Name = name;
            NameKz = nameKz;
            Code = code;
            Permanent = permanent;
            Decline = decline;
        }

        public void Delete()
        {
            DeleteDate = DateTime.Now;
        }
    }
}
