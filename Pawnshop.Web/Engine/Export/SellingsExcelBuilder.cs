using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.Data.Models.Sellings;

namespace Pawnshop.Web.Engine.Export
{
    public class SellingsExcelBuilder : IExcelBuilder<List<Selling>>
    {
        public Stream Build(List<Selling> model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Реализация"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, List<Selling> model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "Дата";
            resultSheet.Cells["B1"].Value = "Вид залога";
            resultSheet.Cells["C1"].Value = "Изделие";
            resultSheet.Cells["D1"].Value = "Себестоимость";
            resultSheet.Cells["E1"].Value = "Описание";
            resultSheet.Cells["F1"].Value = "Цена продажи";
            resultSheet.Cells["G1"].Value = "Дата продажи";
            resultSheet.Cells["H1"].Value = "Статус";
            resultSheet.Cells["I1"].Value = "Автор";

            var row = 2;
            foreach (var selling in model)
            {
                resultSheet.Cells[row, 1].Value = selling.CreateDate.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 2].Value = selling.CollateralType.GetDisplayName();
                resultSheet.Cells[row, 3].Value = selling.Position.Name;
                resultSheet.Cells[row, 4].Value = selling.PriceCost;
                resultSheet.Cells[row, 5].Value = selling.Note;
                resultSheet.Cells[row, 6].Value = selling.SellingCost;
                resultSheet.Cells[row, 7].Value = selling.SellingDate?.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 8].Value = selling.Status.GetDisplayName();
                resultSheet.Cells[row, 9].Value = selling.Author.Fullname;
                row++;
            }
        }
    }
}