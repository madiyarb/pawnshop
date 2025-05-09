using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access.Auction.Interfaces;
using Pawnshop.Data.Models.Auction;

namespace Pawnshop.Data.Access
{
    public class AuctionContractExpenseRepository : RepositoryBase, IAuctionContractExpenseRepository
    {
        public AuctionContractExpenseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task InsertAsync(AuctionContractExpense auctionContractExpense)
        {
            if (auctionContractExpense == null)
            {
                throw new ArgumentNullException(nameof(auctionContractExpense), "Данные для сохранения не переданы");
            }
            
            var sqlQuery = @"
                INSERT INTO AuctionContractExpenses (ContractId, ContractExpenseId, AuthorName, CreateDate)
                OUTPUT INSERTED.Id
                VALUES (@ContractId, @ContractExpenseId, @AuthorName, @CreateDate)";

            try
            {
                auctionContractExpense.Id = await UnitOfWork.Session
                    .ExecuteScalarAsync<int>(sqlQuery, auctionContractExpense, UnitOfWork.Transaction);
            }
            catch (SqlException ex)
            {
                throw new Exception("Ошибка при сохранении записи в базу данных", ex);
            }
        }
    }
}