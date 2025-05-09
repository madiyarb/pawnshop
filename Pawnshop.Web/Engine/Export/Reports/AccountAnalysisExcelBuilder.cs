using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Pawnshop.Web.Models.Reports.AccountAnalysis;

namespace Pawnshop.Web.Engine.Export.Reports
{
    public class AccountAnalysisExcelBuilder : IExcelBuilder<AccountAnalysisModel>
    {
        public Stream Build(AccountAnalysisModel model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Анализ счета"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, AccountAnalysisModel model)
        {
            resultSheet.Column(1).Width = 70;
            resultSheet.Column(2).Width = 30;
            resultSheet.Column(3).Width = 30;
            resultSheet.Column(4).Width = 30;

            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1:D1"].Merge = true;
            resultSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            resultSheet.Cells["A1"].Value = $"АНАЛИЗ СЧЕТА {model.AccountCode}";

            resultSheet.Row(2).Style.Font.Bold = true;
            resultSheet.Cells["A2:B2"].Merge = true;
            resultSheet.Cells["A2"].Value = $"с {model.BeginDate:dd.MM.yyyy}";
            resultSheet.Cells["C2:D2"].Merge = true;
            resultSheet.Cells["C2"].Value = $"по {model.EndDate:dd.MM.yyyy}";
            
            resultSheet.Row(3).Style.Font.Bold = true;
            resultSheet.Cells["A3:D3"].Merge = true;
            resultSheet.Cells["A3"].Value = $"Филиал {model.BranchName}";

            resultSheet.Cells[$"A5:D{5 + model.List.Count + 3}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            resultSheet.Cells[$"A5:D{5 + model.List.Count + 3}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            resultSheet.Cells[$"A5:D{5 + model.List.Count + 3}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            resultSheet.Cells[$"A5:D{5 + model.List.Count + 3}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            resultSheet.Cells["A5"].Value = "Наименование счета";
            resultSheet.Cells["B5"].Value = "Счет";
            resultSheet.Cells["C5"].Value = "С кредита счетов";
            resultSheet.Cells["D5"].Value = "В дебет счетов";

            resultSheet.Cells["A6:B6"].Merge = true;
            resultSheet.Cells["A6"].Value = "Остаток на начало периода";            
            resultSheet.Cells["C6"].Value = model.Group.CashBeginPeriod;

            var row = 7;
            for (var i = 0; i < model.List.Count; i++)
            {
                resultSheet.Cells[row, 1].Value = model.List[i].AccountName;
                resultSheet.Cells[row, 2].Value = model.List[i].AccountCode;
                resultSheet.Cells[row, 3].Value = model.List[i].FromCredit;
                resultSheet.Cells[row, 4].Value = model.List[i].ToDebit;
                row++;
            }

            resultSheet.Cells[$"A{row}:B{row}"].Merge = true;
            resultSheet.Cells[$"A{row}"].Value = "Обороты за период";            
            resultSheet.Cells[$"C{row}"].Value = model.Total.FromCredit;
            resultSheet.Cells[$"D{row}"].Value = model.Total.ToDebit;
            row++;

            resultSheet.Cells[$"A{row}:B{row}"].Merge = true;
            resultSheet.Cells[$"A{row}"].Value = "Остаток на конец периода";            
            resultSheet.Cells[$"C{row}"].Value = model.Group.CashEndPeriod;
        }
    }
}