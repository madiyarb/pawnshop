using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pawnshop.Services.ApplicationOnlineFileStorage
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Метод создания списка в storage команды ДРПП, в данный список загружаются файлы, только для отладки
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Возвращает идентификатор списка</returns>
        public Task<int> CreateListId(CancellationToken cancellationToken);

        /// <summary>
        /// Загружает файл из хранилища ДРПП
        /// </summary>
        /// <param name="fileId">Идентификатор файла</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<FileWithContentType> Download(Guid fileId, CancellationToken cancellationToken);

        /// <summary>
        /// Метод загрузки файла в хранилище ДРПП
        /// </summary>
        /// <param name="listId">Идентификатор списка(в рамках их хранилища они храняться по спискам)</param>
        /// <param name="stream">Стрим с файлом</param>
        /// <param name="comment"> Комментарий</param>
        /// <param name="businessType">Тип файла удостоверение, фото машины итд</param>
        /// <param name="filename">Название файла</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<UploadFileResponse> Upload(string listId, Stream stream, string comment,
            string businessType, string filename, CancellationToken cancellationToken);

    }
}
