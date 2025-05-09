using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Files;

namespace Pawnshop.Data.Access
{
    public class FileRepository : RepositoryBase, IRepository<FileRow>
    {
        public FileRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(FileRow entity)
        {
            entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO FileRows
           (FilePath, ContentType, CreateDate, IsDelete, FileName)
     OUTPUT Inserted.Id
     VALUES
           (@filePath, @contentType, @createDate, @isDelete, @fileName)", entity, UnitOfWork.Transaction);
        }

        public void Update(FileRow entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public FileRow Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<FileRow>("SELECT * FROM FileRows WHERE Id = @id", new { id });
        }

        public async Task<FileRow> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<FileRow>("SELECT * FROM FileRows WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public FileRow Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<FileRow> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }
    }
}