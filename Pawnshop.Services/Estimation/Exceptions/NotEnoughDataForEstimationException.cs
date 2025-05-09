using System;

namespace Pawnshop.Services.Estimation.Exceptions
{
    public sealed class NotEnoughDataForEstimationException : Exception
    {
        public string FieldName { get; set; }

        public string CurrentValue { get; set; }

        public string Comment { get; set; }

        public NotEnoughDataForEstimationException(string fieldName, string currentValue, string comment) 
            : base($"Невозможно отправить заявку на оценку не хватает данных : {fieldName} , текущее значение : {currentValue} комментарий {comment}")
        {
            FieldName = fieldName;
            CurrentValue = currentValue;
            Comment = comment;
        }
    }
}
