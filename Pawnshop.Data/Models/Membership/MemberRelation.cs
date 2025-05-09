using Pawnshop.Core;

namespace Pawnshop.Data.Models.Membership
{
    /// <summary>
    /// Связь между участниками
    /// </summary>
    public class MemberRelation : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор участника с левой стороны
        /// </summary>
        public int LeftMemberId { get; set; }

        /// <summary>
        /// Идентификатор участника с правой стороны
        /// </summary>
        public int RightMemberId { get; set; }

        /// <summary>
        /// Тип связи
        /// </summary>
        public MemberRelationType RelationType { get; set; }

        /// <summary>
        /// Источник связи
        /// </summary>
        public int? SourceId { get; set; }
    }
}