using System;

namespace Pawnshop.Core
{
    public interface ILoggerEntity : IEntity
    {
        public int OperationAuthorId { get; set; }
        public OperationType OperationType { get; set; }
        public string LogReason { get; set; }

        public DateTime LogDateTime { get; set; }
    }
}