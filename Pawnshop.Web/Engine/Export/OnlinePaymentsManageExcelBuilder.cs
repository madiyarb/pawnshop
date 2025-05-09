using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises;

namespace Pawnshop.Web.Engine.Export
{
    public class OnlinePaymentsManageExcelBuilder : IExcelBuilder<OnlinePaymentRevise>
    {
        public Stream Build(OnlinePaymentRevise model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Договоры"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, OnlinePaymentRevise model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "Дата";
            resultSheet.Cells["B1"].Value = "Транзакция";
            resultSheet.Cells["C1"].Value = "Платежная система";
            resultSheet.Cells["D1"].Value = "БИН получателя";
            resultSheet.Cells["E1"].Value = "ИИН/БИН заемщика";
            resultSheet.Cells["F1"].Value = "ФИО заемщика";
            resultSheet.Cells["G1"].Value = "Номер договора";
            resultSheet.Cells["H1"].Value = "Сумма";
            resultSheet.Cells["I1"].Value = "Статус";
            resultSheet.Cells["J1"].Value = "Примечание";

            var row = 2;
            foreach (var onlinePayment in model.Rows)
            {
                resultSheet.Cells[row, 1].Value = onlinePayment.Action.Date.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 2].Value = onlinePayment.ProcessingId;
                resultSheet.Cells[row, 3].Value = model.ProcessingType.GetDisplayName();
                resultSheet.Cells[row, 4].Value = onlinePayment.CompanyBin;
                resultSheet.Cells[row, 5].Value = onlinePayment.Contract.ContractData.Client.IdentityNumber;
                resultSheet.Cells[row, 6].Value = onlinePayment.Contract.ContractData.Client.FullName;
                resultSheet.Cells[row, 7].Value = onlinePayment.Contract.ContractNumber;
                resultSheet.Cells[row, 8].Value = onlinePayment.Amount;
                resultSheet.Cells[row, 9].Value = onlinePayment.Status.GetDisplayName();
                resultSheet.Cells[row, 10].Value = onlinePayment.Message;

                row++;
            }
        }
    }
}
