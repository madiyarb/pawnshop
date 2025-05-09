using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pawnshop.Services.Estimation.Images
{
    public interface IApplicationOnlineEstimationImageService
    {
        public Task<string> UploadImage(Stream stream, string fileName, CancellationToken cancellationToken);
        public Task UploadImagesToEstimationService(Guid applicationOnlineId, CancellationToken cancellationToken);
    }
}
