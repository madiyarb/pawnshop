using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Pawnshop.Data.Models.TasOnline;

namespace Pawnshop.Web.Engine.Export
{
    public class TasOnlinePaymentExcelBuilder : IExcelBuilder<List<TasOnlinePayment>>
    {
        public TasOnlinePaymentExcelBuilder()
        {
            
        }

        public Stream Build(List<TasOnlinePayment> model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Платежи ТасОнлайн"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, List<TasOnlinePayment> model)
        {

            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "ID платежа";
            resultSheet.Cells["B1"].Value = "Дата";
            resultSheet.Cells["C1"].Value = "ИИН";
            resultSheet.Cells["D1"].Value = "Сумма платежа";

            var row = 2;
            foreach (var payment in model)
            {
                resultSheet.Cells[row, 1].Value = payment.TasOnlineDocumentId;
                resultSheet.Cells[row, 2].Style.Numberformat.Format = "dd.mm.yyyy";
                resultSheet.Cells[row, 2].Value = payment.Order.OrderDate;
                resultSheet.Cells[row, 3].Value = payment.Order.Client.IdentityNumber;
                resultSheet.Cells[row, 4].Value = payment.Order.OrderCost;

                row++;
            }

            resultSheet.Cells.AutoFitColumns();
            resultSheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
    }
}