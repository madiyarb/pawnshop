using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection.Documents;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionDocumentsService : ILegalCollectionDocumentsService
    {
        private readonly FileRepository _fileRepository;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly ISessionContext _sessionContext;
        private readonly UserRepository _usersRepository;

        public LegalCollectionDocumentsService(
            FileRepository fileRepository,
            ILegalCaseHttpService legalCaseHttpService,
            ISessionContext sessionContext,
            UserRepository usersRepository
            )
        {
            _fileRepository = fileRepository;
            _legalCaseHttpService = legalCaseHttpService;
            _sessionContext = sessionContext;
            _usersRepository = usersRepository;
        }
        
        public async Task<int> UploadDocumentAsync(UploadLegalCaseDocumentCommand request)
        {
            var file = await _fileRepository.GetAsync(request.FileId);
            var createRequest = new CreateLegalCaseDocumentCommand
            {
                LegalCaseActionId = request.LegalCaseActionId,
                FileName = file.FileName,
                FileId = request.FileId,
                AuthorId = _sessionContext.UserId,
                AuthorFullName = (await _usersRepository.GetAsync(_sessionContext.UserId))?.Fullname,
                LegalCaseId = request.LegalCaseId,
                ContentType = file.ContentType,
                FilePath = file.FilePath,
                DocumentTypeId = request.DocumentTypeId
            };

            var LegalCaseDocumentId = await _legalCaseHttpService.CreateLegalCaseDocument(createRequest);
            return LegalCaseDocumentId;
        }

        public async Task<int> DeleteDocumentAsync(DeleteLegalCaseDocumentCommand request)
        {
            var result = await _legalCaseHttpService.DeleteLegalCaseDocument(request.LegalCaseDocumentId);
            if(result < 1)
            {
                throw new PawnshopApplicationException($"Не удалось удалить запись из таблицы документов с ИД: {request.LegalCaseDocumentId}");
            }

            return result;
        }
    }
}