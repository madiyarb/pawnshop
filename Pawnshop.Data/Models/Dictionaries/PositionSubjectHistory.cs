using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class PositionSubjectHistory : PositionSubject, IEntity
    {
        public PositionSubjectHistory()
        {

        }

        public PositionSubjectHistory(int positionId, int subjectId, int clientId, DateTime beginDate, int authorId)
        {
            PositionId = positionId;
            ClientId = clientId;
            SubjectId = subjectId;
            BeginDate = beginDate;
            CreateDate = DateTime.Now;
            AuthorId = authorId;
        }

        public DateTime BeginDate { get; set; }
    }
}
