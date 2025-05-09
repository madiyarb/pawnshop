using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access
{
    public class InscriptionRepository : RepositoryBase, IRepository<Inscription>
    {
        private readonly ContractRateRepository _contractRateRepository;
        private readonly PositionEstimatesRepository _positionEstimatesRepository;
        private readonly ContractRepository _contractRepository;
        
        public InscriptionRepository(IUnitOfWork unitOfWork, ContractRepository contractRepository) : base(unitOfWork)
        {
            _contractRepository = contractRepository;
        }
        
        public void Insert(Inscription entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Inscriptions ( ContractId, TotalCost, Date, Status, CreateDate, AuthorId)
VALUES ( @ContractId, @TotalCost, @Date, @Status, @CreateDate, @AuthorId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                foreach (var action in entity.Actions)
                {
                    action.InscriptionId = entity.Id;
                    action.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO InscriptionActions ( Date, ActionType, AuthorId, CreateDate, InscriptionId)
VALUES ( @Date, @ActionType, @AuthorId, @CreateDate, @InscriptionId)
SELECT SCOPE_IDENTITY()", action, UnitOfWork.Transaction);
                    foreach (var row in action.Rows)
                    {
                        row.InscriptionId = entity.Id;
                        row.InscriptionActionId = action.Id;
                        row.OrderId = null;
                        row.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO InscriptionRows ( InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)
SELECT SCOPE_IDENTITY()", row, UnitOfWork.Transaction);
                    }
                }
                UpdateContractInscription(entity.ContractId, entity.Id);
                transaction.Commit();
            }
        }
        
        public void UpdateContractInscription(int contractId, int id)
        {
            UnitOfWork.Session.Execute(@"
UPDATE Contracts SET InscriptionId = @id WHERE Id = @contractId",
                new { id = id, contractId = contractId }, UnitOfWork.Transaction);
        }
        
        public void Update(Inscription entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Inscriptions
SET ContractId = @ContractId, TotalCost = @TotalCost, Status = @Status
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void DeleteInscriptionActionRows(int actionId)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"DELETE FROM InscriptionRows WHERE InscriptionActionId = @id", new { id = actionId }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        
        public void InsertRows(List<InscriptionRow> rows)
        {
            using (var transaction = BeginTransaction())
            {
                foreach (var row in rows)
                {
                    row.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO InscriptionRows ( InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)
SELECT SCOPE_IDENTITY()", row, UnitOfWork.Transaction);
                }
                transaction.Commit();
            }
        }
        
        public void InsertAction(InscriptionAction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO InscriptionActions ( Date, ActionType, AuthorId, CreateDate, InscriptionId)
VALUES ( @Date, @ActionType, @AuthorId, @CreateDate, @InscriptionId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                if (entity.Rows != null)
                {
                    foreach (var row in entity.Rows)
                    {
                        row.InscriptionId = entity.InscriptionId;
                        row.InscriptionActionId = entity.Id;
                        row.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO InscriptionRows ( InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
                    VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)
                    SELECT SCOPE_IDENTITY()", row, UnitOfWork.Transaction);
                    }
                }

                transaction.Commit();
            }
        }
        
        public void SaveActionRows(InscriptionAction entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                if (entity.Rows != null)
                {
                    foreach (var row in entity.Rows)
                    {
                        if (row.Id == 0)
                        {
                            row.InscriptionActionId = entity.Id;
                            row.InscriptionActionId = entity.InscriptionId;
                            row.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                            INSERT INTO InscriptionRows ( InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
                            VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)
                            SELECT SCOPE_IDENTITY()", row, UnitOfWork.Transaction);
                        }
                        else
                        {
                            UnitOfWork.Session.Execute(@"
                            UPDATE InscriptionRows
                            SET OrderId = @OrderId
                            WHERE Id = @Id", row, UnitOfWork.Transaction);
                        }
                    }
                }
                transaction.Commit();
            }
        }
        
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Inscriptions SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id",
                    new { id }, UnitOfWork.Transaction);
                UnitOfWork.Session.Execute(@"UPDATE Contracts SET InscriptionId = NULL WHERE InscriptionId = @id",
                    new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        
        public void DeleteAction(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InscriptionActions SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id",
                    new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Inscription GetOnlyInscription(int id)
        {
            return UnitOfWork.Session.Query<Inscription>(@"
                SELECT *
                FROM Inscriptions
                WHERE Inscriptions.Id = @id",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Inscription Get(int id)
        {
            var inscription = UnitOfWork.Session.Query<Inscription, User, Inscription>(@"
SELECT *
FROM Inscriptions
LEFT JOIN Users ON Inscriptions.AuthorId=Users.Id
WHERE Inscriptions.Id = @id AND Inscriptions.DeleteDate IS NULL
", (i, u) =>
        {
            i.Author = u;
            return i;
        }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
            inscription.Actions = UnitOfWork.Session.Query<InscriptionAction, User, InscriptionAction>(@"
SELECT *
FROM InscriptionActions
LEFT JOIN Users ON InscriptionActions.AuthorId=Users.Id
WHERE InscriptionId = @id AND InscriptionActions.DeleteDate IS NULL", (i, u) =>
        {
            i.Author = u;
            return i;
        }, new { id }, UnitOfWork.Transaction).ToList();
            foreach (var action in inscription.Actions)
            {
                action.Rows = UnitOfWork.Session.Query<InscriptionRow>(@"
SELECT *
FROM InscriptionRows
WHERE InscriptionActionId = @id", new { id = action.Id }).ToList();
            }
            return inscription;
        }
        
        public async Task<Inscription> GetAsync(int inscriptionId)
        {
            var inscriptionQuery = "SELECT * FROM Inscriptions WHERE Id = @Id";

            var inscription = await UnitOfWork.Session
                    .QueryFirstOrDefaultAsync<Inscription>(inscriptionQuery, new { Id = inscriptionId }, UnitOfWork.Transaction);
            
            if (inscription is null)
            {
                return null;
            }

            inscription.Rows = await GetInscriptionRowsAsync(inscriptionId);
            inscription.Actions = await GetInscriptionActionsAsync(inscriptionId);
            
            return inscription;
        }

        public Inscription GetInscriptionByContractId(int id, int inscriptionId)
        {
            Inscription inscription = UnitOfWork.Session.Query<Inscription, User, Inscription>(@"
SELECT *
FROM Inscriptions WITH(NOLOCK)
LEFT JOIN Users ON Inscriptions.AuthorId=Users.Id
WHERE ContractId = @id AND Inscriptions.DeleteDate IS NULL
", (i, u) =>
                {
                    i.Author = u;
                    return i;
                }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
            inscription.Actions = UnitOfWork.Session.Query<InscriptionAction, User, InscriptionAction>(@"
SELECT *
FROM InscriptionActions WITH(NOLOCK)
LEFT JOIN Users ON InscriptionActions.AuthorId=Users.Id
WHERE InscriptionId = @id AND InscriptionActions.DeleteDate IS NULL", (i, u) =>
                {
                    i.Author = u;
                    return i;
                }, new { id = inscriptionId }, UnitOfWork.Transaction).ToList();

            foreach (var action in inscription.Actions)
            {
                action.Rows = UnitOfWork.Session.Query<InscriptionRow, Account, Account, InscriptionRow>(@"
SELECT r.*, debit.*, credit.*
FROM InscriptionRows r WITH(NOLOCK)
LEFT JOIN Accounts debit ON debit.Id = r.DebitAccountId
LEFT JOIN Accounts credit ON credit.Id = r.CreditAccountId
WHERE InscriptionActionId = @id", (row, debit, credit) =>
                {
                    row.DebitAccount = debit;
                    row.CreditAccount = credit;
                    return row;
                },
                    new { id = action.Id }, UnitOfWork.Transaction).ToList();
            }

            return inscription;
        }

        public Inscription Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<Inscription> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<Inscription>> GetByContractId(int contractId)
        {
            return await UnitOfWork.Session.QueryAsync<Inscription>(@"SELECT *
  FROM Inscriptions
 WHERE DeleteDate IS NULL
   AND ContractId = @contractId",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<Inscription> SaveContractWithInscriptionAsync(Contract contract, Inscription inscription)
        {
            using var transaction = BeginTransaction();
            try
            {
                _contractRepository.Update(contract);
                inscription.ContractId = contract.Id;
                var savedInscription = await SaveInscriptionAsync(inscription);

                transaction.Commit();
                return savedInscription;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new PawnshopApplicationException($"Не удалось создать исполнительную надпись. {e.Message}");
            }
        }
        
        public async Task<Inscription> GetOnlyInscriptionAsync(int inscriptionId)
        {
            var inscriptionQuery = "SELECT * FROM Inscriptions WHERE Id = @Id";

            var inscription = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Inscription>(inscriptionQuery, new { Id = inscriptionId },
                    UnitOfWork.Transaction);
            
            return inscription;
        }

        public Inscription GetOnlyInscription(int? inscriptionId)
        {
            if (inscriptionId == null) return null;

            var inscriptionQuery = "SELECT * FROM Inscriptions WHERE Id = @Id";

            var inscription = UnitOfWork.Session
                .QueryFirstOrDefault<Inscription>(inscriptionQuery, new { Id = inscriptionId }, UnitOfWork.Transaction);
            
            return inscription;
        }

        public async Task SaveInscriptionWithActions(Contract contract, Inscription inscription, InscriptionAction action)
        {
            using var transaction = UnitOfWork.BeginTransaction();
            try
            {
                if (contract.Id > 0)
                {
                    _contractRepository.Update(contract);
                }
                await UpdateInscriptionAsync(inscription);
                await SaveInscriptionAction(action);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new PawnshopApplicationException($"Не удалось сохранить InscriptionAction. {ex.Message}");
            }
        }
        
        public async Task AddInscriptionRow(Inscription inscription, InscriptionRow row)
        {
            var sqlQuery = @"
                INSERT INTO InscriptionRows ( InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
                VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)";

            using var transaction = BeginTransaction();
            try
            {
                await UnitOfWork.Session.ExecuteAsync(sqlQuery, new
                {
                    InscriptionId = inscription.Id,
                    PaymentType = row.PaymentType,
                    Cost = row.Cost,
                    InscriptionActionId = row.InscriptionActionId,
                    DebitAccountId = row.DebitAccountId,
                    CreditAccountId = row.CreditAccountId,
                    Period = row.Period,
                    Percent = row.Percent,
                    OrderId = row.OrderId
                }, transaction);

                inscription.TotalCost += row.Cost;

                var updateInscriptionSql = "UPDATE Inscriptions SET TotalCost = @TotalCost WHERE Id = @InscriptionId";

                await UnitOfWork.Session.ExecuteAsync(updateInscriptionSql, new
                {
                    TotalCost = inscription.TotalCost,
                    InscriptionId = inscription.Id
                }, transaction);

                var getActionQuery = @"
                select * from InscriptionActions
                where InscriptionId = @InscriptionId";

                var actions = await GetInscriptionActionsAsync(inscription.Id);

                if (actions != null && actions.Any())
                {
                    foreach (var action in actions)
                    {
                        await UnitOfWork.Session.ExecuteAsync(sqlQuery, new
                        {
                            InscriptionId = inscription.Id,
                            PaymentType = row.PaymentType,
                            Cost = row.Cost,
                            InscriptionActionId = action.Id,
                            DebitAccountId = row.DebitAccountId,
                            CreditAccountId = row.CreditAccountId,
                            Period = row.Period,
                            Percent = row.Percent,
                            OrderId = row.OrderId
                        }, transaction);
                    }
                }
            }
            catch (Exception e)
            {
               transaction.Rollback();
                throw new PawnshopApplicationException(
                    $"При попытке добавления новой записи \"InscriptionRow\" произошла ошибка. {e.Message}");
            }
        }
        
        public async Task<Inscription> SaveInscriptionAsync(Inscription inscription)
        {
            try
            {
                if (inscription is null)
                    throw new ArgumentNullException(nameof(inscription), "Исполнительная надпись не найдена");

                if (inscription.Id == 0)
                {
                    await InsertInscriptionAsync(inscription);
                }
                else
                {
                    await UpdateInscriptionAsync(inscription);
                    foreach (var action in inscription.Actions.ToList())
                    {
                        if (action.Id > 0) continue;
                        
                        var newAction = new InscriptionAction
                        {
                            ActionType = action.ActionType,
                            AuthorId = action.AuthorId,
                            CreateDate = action.CreateDate,
                            Date = action.Date,
                            InscriptionId = action.InscriptionId,
                            Rows = action.Rows.ToList()
                        };

                        var actionParameters = new
                        {
                            InscriptionId = inscription.Id,
                            newAction.Date,
                            newAction.ActionType,
                            newAction.AuthorId,
                            CreateDate = DateTime.Now
                        };

                        var actionId = await UnitOfWork.Session.ExecuteScalarAsync<int>(
                            @"INSERT INTO InscriptionActions (InscriptionId, Date, ActionType, AuthorId, CreateDate)
                                VALUES (@InscriptionId, @Date, @ActionType, @AuthorId, @CreateDate);
                                SELECT CAST(SCOPE_IDENTITY() AS INT)",
                            actionParameters, UnitOfWork.Transaction);

                        if (newAction.Rows != null && newAction.Rows.Any())
                        {
                            foreach (var row in newAction.Rows)
                            {
                                var rowParameters = new
                                {
                                    InscriptionId = inscription.Id,
                                    InscriptionActionId = actionId,
                                    row.PaymentType,
                                    row.DebitAccountId,
                                    row.CreditAccountId,
                                    row.Percent,
                                    row.Period,
                                    row.Cost,
                                    row.OrderId
                                };

                                await UnitOfWork.Session.ExecuteAsync(
                                    @"INSERT INTO InscriptionRows (InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
                                        VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)",
                                    rowParameters, UnitOfWork.Transaction);
                            }
                        }
                    }
                }

                return inscription;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Не удалось сохранить исп. надпись с Id: {inscription.Id} {e.Message}");
            }
        }

        private async Task<int> SaveInscriptionAction(InscriptionAction action)
        {
            var insertQuery = @"
            INSERT INTO InscriptionActions (InscriptionId, Date, ActionType, AuthorId, CreateDate, DeleteDate)
            VALUES (@InscriptionId, @Date, @ActionType, @AuthorId, @CreateDate, @DeleteDate);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var insertedActionId = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<int>(insertQuery, action, UnitOfWork.Transaction);

            if (action.Rows != null && action.Rows.Any())
            {
                foreach (var row in action.Rows)
                {
                    row.InscriptionId = action.InscriptionId;
                    row.InscriptionActionId = insertedActionId; // Привязываем запись InscriptionRow к InscriptionAction
                    var insertRowQuery = @"
                                INSERT INTO InscriptionRows ( InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
                    VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)";

                    await UnitOfWork.Session.ExecuteAsync(insertRowQuery, row, UnitOfWork.Transaction);
                }
            }

            return insertedActionId;
        }
        
        private void UpdateInscription(Inscription inscription)
        {
            try
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Inscriptions
                    SET ContractId = @ContractId, TotalCost = @TotalCost, Status = @Status
                    WHERE Id = @Id",
                    inscription, UnitOfWork.Transaction);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Не удалось обновить Inscription. {e.Message}");
            }
        }
        
        private async Task UpdateInscriptionAsync(Inscription inscription)
        {
            try
            {
                UnitOfWork.Session.ExecuteAsync(@"
                    UPDATE Inscriptions
                    SET ContractId = @ContractId, TotalCost = @TotalCost, Status = @Status
                    WHERE Id = @Id",
                    inscription, UnitOfWork.Transaction);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Не удалось обновить Inscription. {e.Message}");
            }
        }
        
        private async Task InsertInscriptionAsync(Inscription inscription)
        {
            try
            {
                var inscriptionParameters = new
                {
                    inscription.ContractId,
                    inscription.TotalCost,
                    inscription.Date,
                    inscription.Status,
                    inscription.CreateDate,
                    inscription.AuthorId
                };

                var inscriptionId = await UnitOfWork.Session.ExecuteScalarAsync<int>(
                    @"INSERT INTO Inscriptions (ContractId, TotalCost, Date, Status, CreateDate, AuthorId)
                            VALUES (@ContractId, @TotalCost, @Date, @Status, @CreateDate, @AuthorId);
                            SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    inscriptionParameters, UnitOfWork.Transaction);

                foreach (var action in inscription.Actions)
                {
                    var actionParameters = new
                    {
                        InscriptionId = inscriptionId,
                        action.Date,
                        action.ActionType,
                        action.AuthorId,
                        CreateDate = DateTime.Now.Date
                    };

                    var actionId = await UnitOfWork.Session.ExecuteScalarAsync<int>(
                        @"INSERT INTO InscriptionActions (InscriptionId, Date, ActionType, AuthorId, CreateDate)
                    VALUES (@InscriptionId, @Date, @ActionType, @AuthorId, @CreateDate);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)",
                        actionParameters, UnitOfWork.Transaction);


                    if (action.Rows != null && action.Rows.Any())
                    {
                        foreach (var row in action.Rows)
                        {
                            var rowParameters = new
                            {
                                InscriptionId = inscriptionId,
                                InscriptionActionId = actionId,
                                row.PaymentType,
                                row.DebitAccountId,
                                row.CreditAccountId,
                                row.Percent,
                                row.Period,
                                row.Cost,
                                row.OrderId
                            };

                            await UnitOfWork.Session.ExecuteAsync(
                                @"INSERT INTO InscriptionRows (InscriptionId, InscriptionActionId, PaymentType, DebitAccountId, CreditAccountId, Period, Cost, [Percent], OrderId)
                                    VALUES ( @InscriptionId, @InscriptionActionId, @PaymentType, @DebitAccountId, @CreditAccountId, @Period, @Cost, @Percent, @OrderId)",
                                rowParameters, UnitOfWork.Transaction);
                        }
                    }
                }

                await UpdateContractInscriptionAsync(inscription.ContractId, inscriptionId);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Не удалось сделать запись в Inscriptions. {e.Message}" );
            }
        }
        
        private async Task UpdateContractInscriptionAsync(int contractId, int id)
        {
            try
            {
                await UnitOfWork.Session.ExecuteAsync(@"
                    UPDATE Contracts SET InscriptionId = @id WHERE Id = @contractId",
                    new { id = id, contractId = contractId }, UnitOfWork.Transaction);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        private async Task<List<InscriptionRow>> GetInscriptionRowsAsync(int inscriptionId)
        {
            var parameters = new { InscriptionId = inscriptionId };
            var sqlQuery = @"
                SELECT *
                FROM InscriptionRows
                WHERE InscriptionId = @InscriptionId";

            var inscriptionRows = await UnitOfWork.Session
                .QueryAsync<InscriptionRow>(sqlQuery, parameters, UnitOfWork.Transaction);
            
            if (!inscriptionRows.Any())
            {
                return null;
            }

            inscriptionRows = inscriptionRows.ToList()
                .GroupBy(row => new
                {
                    row.PaymentType,
                    row.Cost
                })
                .Select(group => group.First())
                .ToList();

            return inscriptionRows.ToList();
        }
        
        private async Task<List<InscriptionAction>> GetInscriptionActionsAsync(int inscriptionId)
        {
            var parameters = new { InscriptionId = inscriptionId };
            var sqlQuery = @"
                SELECT *
                FROM InscriptionActions
                WHERE DeleteDate IS NULL
                  AND InscriptionId = @InscriptionId";

            var actions = await UnitOfWork.Session
                .QueryAsync<InscriptionAction>(sqlQuery, parameters, UnitOfWork.Transaction);

            if (!actions.Any())
            {
                return null;
            }

            foreach (var action in actions)
            {
                action.Rows = await GetActionInscriptionRowsAsync(action.Id);
            }

            return actions.ToList();
        }
        
        private async Task<List<InscriptionRow>> GetActionInscriptionRowsAsync(int actionId)
        {
            var parameters = new { InscriptionActionId = actionId };
            var sqlQuery = @"
                SELECT *
                FROM InscriptionRows
                WHERE InscriptionActionId = @InscriptionActionId";

            var result = await UnitOfWork.Session
                .QueryAsync<InscriptionRow>(sqlQuery, parameters, UnitOfWork.Transaction);

            if (!result.Any())
            {
                return null;
            }

            return result.ToList();
        }
    }
}