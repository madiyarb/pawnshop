namespace Pawnshop.Data.Models.Membership
{
    /// <summary>
    /// Типы связей между участниками
    /// </summary>            
    public enum MemberRelationType : short
    {
        /// <summary>
        /// Сам на себя
        /// </summary>
        Self = 0,

        /// <summary>
        /// Прямая
        /// </summary>
        Direct = 10,

        /// <summary>
        /// Последствие (ParentId - ссылка на Direct-связь)
        /// </summary>
        Effect = 20
    }
}