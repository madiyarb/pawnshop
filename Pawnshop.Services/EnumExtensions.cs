using System;
using System.ComponentModel;

namespace Pawnshop.Services
{
    /// <summary>
    /// Класс для получения текстового описания поля Enum полученного из атрибута: [Description("Text")]
    /// </summary>
    /// 
    /// <example>
    /// [Description("Отклонен")]
    /// Prohibited = 20
    /// Метод GetDescription() вернёт "Отклонен"
    /// 
    /// <code>
    /// string Status = Order.ApproveStatus.GetDescription()
    /// </code>
    /// </example>
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));

            return attribute?.Description ?? value.ToString();
        }
    }
}