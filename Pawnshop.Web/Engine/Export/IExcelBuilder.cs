using System.IO;

namespace Pawnshop.Web.Engine.Export
{
    public interface IExcelBuilder<in T>
    {
        Stream Build(T model);
    }
}