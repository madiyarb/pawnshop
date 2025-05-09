using MimeTypes;

namespace Pawnshop.Core.Extensions
{
    public static class ContentTypeExtensions
    {
        public static string GetExtension(this string contentType)
        {
            return MimeTypeMap.GetExtension(contentType);
        }
    }
}
