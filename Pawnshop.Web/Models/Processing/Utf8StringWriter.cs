using System.IO;
using System.Text;

namespace Pawnshop.Web.Models.Processing
{
    public sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
