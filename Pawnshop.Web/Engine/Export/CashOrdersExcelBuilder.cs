using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Web.Engine.Export
{
    public class CashOrdersExcelBuilder : IExcelBuilder<List<CashOrder>>
    {
        public Stream Build(List<CashOrder> model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Кассовые ордера"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, List<CashOrder> model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "Вид";
            resultSheet.Cells["B1"].Value = "№";
            resultSheet.Cells["C1"].Value = "Дата";
            resultSheet.Cells["D1"].Value = "Основание";
            resultSheet.Cells["E1"].Value = "Примечание";
            resultSheet.Cells["F1"].Value = "Сумма";
            resultSheet.Cells["G1"].Value = "Дебет";
            resultSheet.Cells["H1"].Value = "Кредит";
            resultSheet.Cells["I1"].Value = "Клиент";
            resultSheet.Cells["J1"].Value = "Филиал";
            resultSheet.Cells["K1"].Value = "Автор";

            var row = 2;
            foreach (var cashOrder in model)
            {
                resultSheet.Cells[row, 1].Value = cashOrder.OrderType.GetDisplayName();
                resultSheet.Cells[row, 2].Value = cashOrder.OrderNumber.ToString();
                resultSheet.Cells[row, 3].Value = cashOrder.OrderDate.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 4].Value = cashOrder.Reason;
                resultSheet.Cells[row, 5].Value = cashOrder.Note;
                resultSheet.Cells[row, 6].Value = cashOrder.OrderCost.ToString();
                resultSheet.Cells[row, 7].Value = cashOrder.DebitAccount.Code;
                resultSheet.Cells[row, 8].Value = cashOrder.CreditAccount.Code;
                resultSheet.Cells[row, 9].Value = cashOrder.ClientName;
                resultSheet.Cells[row, 10].Value = cashOrder.Branch.DisplayName;
                resultSheet.Cells[row, 11].Value = cashOrder.Author.Fullname;
                row++;
            }
        }
    }
}