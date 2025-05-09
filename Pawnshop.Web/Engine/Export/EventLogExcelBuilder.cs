using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Web.Engine.Export
{
    public class EventLogExcelBuilder : IExcelBuilder<List<EventLogItem>>
    {
        public Stream Build(List<EventLogItem> model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Журнал событий"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, List<EventLogItem> model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "Дата";
            resultSheet.Cells["B1"].Value = "Событие";
            resultSheet.Cells["C1"].Value = "Филиал";
            resultSheet.Cells["D1"].Value = "Пользователь";
            resultSheet.Cells["E1"].Value = "IP адрес";
            resultSheet.Cells["F1"].Value = "Статус";

            var row = 2;
            foreach (var item in model)
            {
                resultSheet.Cells[row, 1].Value = item.CreateDate.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 2].Value = item.EventDescription;
                resultSheet.Cells[row, 3].Value = item.BranchName;
                resultSheet.Cells[row, 4].Value = item.UserName;
                resultSheet.Cells[row, 5].Value = item.Address;
                resultSheet.Cells[row, 6].Value = item.EventStatus.GetDisplayName();
                row++;
            }
        }
    }
}