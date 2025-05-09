using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientsBlackListDto
    {
        public int Id { get; set; }

        [RequiredId(ErrorMessage = "Поле причина включения в Черный список обязательно для заполнения")]
        public int ReasonId { get; set; }

        public BlackListReason BlackListReason { get; set; }

        [Required(ErrorMessage = "Поле 'Основание включения в Черный список' обязательно для заполнения")]
        public string AddReason { get; set; }

        [Required(ErrorMessage = "Поле 'Дата включения' обязально для заполнения")]
        public DateTime? AddedAt { get; set; }

        public int? AddedBy { get; set; }

        public FileRow AddedFile { get; set; }

        public FileRow RemovedFile { get; set; }

        public string RemoveReason { get; set; }

        public DateTime? RemoveDate { get; set; }

        public int? RemovedBy { get; set; }
    }
}
