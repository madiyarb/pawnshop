namespace Pawnshop.Core
{
    public interface ILoggable
    {
        object Format();
    }

    public interface ILoggableToEntity
    {
        int GetLinkedEntityId();
    }
}