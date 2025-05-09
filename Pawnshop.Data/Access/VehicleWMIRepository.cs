using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class VehicleWMIRepository : RepositoryBase, IRepository<VehicleWMI>
    {
        public VehicleWMIRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(VehicleWMI entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO VehicleWMIs ( VehicleMarkId, Code, AdditionalInfo, VehicleManufacturerId, VehicleCountryCodeId  )
VALUES ( @VehicleMarkId, @Code, @AdditionalInfo, @VehicleManufacturerId, @VehicleCountryCodeId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(VehicleWMI entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE VehicleWMIs
SET VehicleMarkId = @VehicleMarkId, Code = @Code, AdditionalInfo = @AdditionalInfo, 
VehicleManufacturerId = @VehicleManufacturerId, VehicleCountryCodeId = @VehicleCountryCodeId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE VehicleWMIs SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public VehicleWMI Get(int id)
        {
            return UnitOfWork.Session.Query<VehicleWMI, VehicleMark, VehicleManufacturer, VehicleCountryCode, VehicleWMI>(@"
SELECT wmi.*, vmr.*, vmf.*, vcc.*
FROM VehicleWMIs wmi
LEFT JOIN VehicleMarks vmr ON vmr.Id = wmi.VehicleMarkId
LEFT JOIN VehicleManufacturers vmf ON vmf.Id = wmi.VehicleManufacturerId
LEFT JOIN VehicleCountryCodes vcc ON vcc.Id = wmi.VehicleCountryCodeId
WHERE wmi.Id=@id", (wmi, vmr, vmf, vcc) =>
            {
                wmi.VehicleMark = vmr;
                wmi.VehicleManufacturer = vmf;
                wmi.VehicleCountryCode = vcc;
                return wmi;
            },
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public VehicleWMI Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<VehicleWMI> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "wmi.DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "wmi.Code", "vmr.Name", "vmf.Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "wmi.Code",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<VehicleWMI, VehicleMark, VehicleManufacturer, VehicleCountryCode, VehicleWMI>($@"
SELECT wmi.*, vmr.*, vmf.*, vcc.* 
FROM VehicleWMIs wmi
LEFT JOIN VehicleMarks vmr ON vmr.Id = wmi.VehicleMarkId
LEFT JOIN VehicleManufacturers vmf ON vmf.Id = wmi.VehicleManufacturerId
LEFT JOIN VehicleCountryCodes vcc ON vcc.Id = wmi.VehicleCountryCodeId
{condition} {order} {page}", (wmi, vmr, vmf, vcc) =>
            {
                wmi.VehicleMark = vmr;
                wmi.VehicleManufacturer = vmf;
                wmi.VehicleCountryCode = vcc;
                return wmi;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Code");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM VehicleWMIs
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int wmiId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(c)
FROM (
SELECT COUNT(*) as c
FROM Cars
WHERE VehicleWMIId = @wmiId
UNION ALL
SELECT COUNT(*) as c
FROM Machineries
WHERE VehicleWMIId = @wmiId
) as t", new { wmiId = wmiId });
        }
    }
}