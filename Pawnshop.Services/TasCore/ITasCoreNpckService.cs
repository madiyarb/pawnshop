using Pawnshop.Services.TasCore.Models.NpckEsign;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Pawnshop.Services.TasCore
{
    public interface ITasCoreNpckService
    {
        Task<TasCoreNpckEsignDocumentUploadResponse> DocumentUpload(Guid fileStorageId, string link, string fileName, Guid? signedFileStorageId, CancellationToken cancellationToken = default);
        Task<TasCoreNpckEsignGenerateUrlResponse> GenerateUrl(string iin, string phone, string redirectUri, string language, List<Guid> documentIds, CancellationToken cancellationToken);
        Task<TasCoreNpckEsignTokenResponse> GetToken(Guid applicationId, string code, string redirectUri, CancellationToken cancellationToken = default);
        Task<TasCoreNpckEsignSaveSignedFileResponse> SaveSingedFile(int listId, string code, string redirectUri, CancellationToken cancellationToken = default);
    }
}
