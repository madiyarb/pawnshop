using System;
using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Models.Reports.ContractMonitoring;

namespace Pawnshop.Web.Engine.Export.Reports
{
    public class ContractMonitoringExcelBuilder : IExcelBuilder<ContractMonitoringModel>
    {
        public Stream Build(ContractMonitoringModel model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Мониторинг билетов"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, ContractMonitoringModel model)
        {
            resultSheet.Column(2).Width = 20;
            resultSheet.Column(3).Width = 20;
            resultSheet.Column(4).Width = 20;
            resultSheet.Column(5).Width = 20;
            resultSheet.Column(6).Width = 20;
            resultSheet.Column(7).Width = 20;
            resultSheet.Column(8).Width = 70;
            resultSheet.Column(9).Width = 70;

            var right = "I";
            var column = 9;
            if (model.CollateralType == CollateralType.Gold)
            {
                resultSheet.Column(++column).Width = 20;
                resultSheet.Column(++column).Width = 20;
                resultSheet.Column(++column).Width = 20;
            }
            else if (model.CollateralType == CollateralType.Car || model.CollateralType == CollateralType.Machinery)
            {
                resultSheet.Column(++column).Width = 20;
            }
            
            if (model.DisplayStatus.HasValue && model.DisplayStatus.Value == ContractDisplayStatus.Prolong)
            {
                resultSheet.Column(++column).Width = 20;
                resultSheet.Column(++column).Width = 20;
                resultSheet.Column(++column).Width = 70;
            }
            
            resultSheet.Column(++column).Width = 20;
            resultSheet.Column(++column).Width = 70;

            switch (column)
            {
                case 17:
                    right = "Q";
                    break;
                case 15:
                    right = "O";
                    break;
                case 14:
                    right = "N";
                    break;
                case 13:
                    right = "M";
                    break;
                case 12:
                    right = "L";
                    break;
                default:
                    right = "K";
                    break;
            }

            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells[$"A1:{right}1"].Merge = true;
            resultSheet.Cells["A1"].Value = "МОНИТОРИНГ ПО БИЛЕТАМ";

            resultSheet.Row(2).Style.Font.Bold = true;
            resultSheet.Cells[$"A2:C2"].Merge = true;
            resultSheet.Cells["A2"].Value = $"Дата с {model.BeginDate:dd.MM.yyyy}";
            resultSheet.Cells[$"D2:G2"].Merge = true;
            resultSheet.Cells["D2"].Value = $"Дата по {model.EndDate:dd.MM.yyyy}";

            resultSheet.Row(3).Style.Font.Bold = true;
            resultSheet.Cells[$"A3:C3"].Merge = true;
            resultSheet.Cells["A3"].Value = $"Филиал {model.BranchName}";
            resultSheet.Cells[$"D3:G3"].Merge = true;
            resultSheet.Cells["D3"].Value = $"Вид залога {model.CollateralType.GetDisplayName()}";

            var rows = 4;
            if (model.DisplayStatus.HasValue)
            {
                resultSheet.Row(rows).Style.Font.Bold = true;
                resultSheet.Cells[$"A{rows}:G{rows}"].Merge = true;
                resultSheet.Cells[$"A{rows}"].Value = $"Статус {model.DisplayStatus.Value.GetDisplayName()}";
                rows++;
            }

            if (model.ProlongDayCount != null)
            {
                resultSheet.Row(rows).Style.Font.Bold = true;
                resultSheet.Cells[$"A{rows}:G{rows}"].Merge = true;
                resultSheet.Cells[$"A{rows}"].Value = $"Дней просрочки {model.ProlongDayCount.DisplayOperator} {model.ProlongDayCount.Value}";
                rows++;
            }

            if (model.LoanCost != null)
            {
                resultSheet.Row(rows).Style.Font.Bold = true;
                resultSheet.Cells[$"A{rows}:G{rows}"].Merge = true;
                resultSheet.Cells[$"A{rows}"].Value = $"Ссуда {model.LoanCost.DisplayOperator} {model.LoanCost.Value}";
                rows++;
            }

            resultSheet.Cells[$"A{rows}:{right}{rows + model.List.Count}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            resultSheet.Cells[$"A{rows}:{right}{rows + model.List.Count}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            resultSheet.Cells[$"A{rows}:{right}{rows + model.List.Count}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            resultSheet.Cells[$"A{rows}:{right}{rows + model.List.Count}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            resultSheet.Cells[rows, 1].Value = "№";
            resultSheet.Cells[rows, 2].Value = "№ билета";
            resultSheet.Cells[rows, 3].Value = "Вид залога";
            resultSheet.Cells[rows, 4].Value = "Дата";
            resultSheet.Cells[rows, 5].Value = "Сумма";
            resultSheet.Cells[rows, 6].Value = "Статус";
            resultSheet.Cells[rows, 7].Value = "Дата возврата";
            resultSheet.Cells[rows, 8].Value = "Клиент";
            resultSheet.Cells[rows, 9].Value = "Категория";

            column = 9;
            if (model.CollateralType == CollateralType.Gold)
            {
                resultSheet.Cells[rows, ++column].Value = "Общий вес";
                resultSheet.Cells[rows, ++column].Value = "Чистый вес";
                resultSheet.Cells[rows, ++column].Value = "Проба";
            }
            else if (model.CollateralType == CollateralType.Car || model.CollateralType == CollateralType.Machinery)
            {
                resultSheet.Cells[rows, ++column].Value = "Каско";                
            }
            
            if (model.DisplayStatus.HasValue && model.DisplayStatus.Value == ContractDisplayStatus.Prolong)
            {
                resultSheet.Cells[rows, ++column].Value = "Дата продления";
                resultSheet.Cells[rows, ++column].Value = "Сумма продления";
                resultSheet.Cells[rows, ++column].Value = "Автор продления";
            }

            resultSheet.Cells[rows, ++column].Value = "Дата выкупа";
            resultSheet.Cells[rows, ++column].Value = "Автор";

            rows++;

            decimal sum = 0;
            for (var i = 0; i < model.List.Count; i++)
            {
                var item = model.List[i];
                resultSheet.Cells[rows, 1].Value = (i + 1);
                resultSheet.Cells[rows, 2].Value = item.ContractNumber;
                resultSheet.Cells[rows, 3].Value = ((CollateralType)item.CollateralType).GetDisplayName();
                resultSheet.Cells[rows, 4].Value = item.ContractDate.ToString("dd.MM.yyyy");
                resultSheet.Cells[rows, 5].Value = item.LoanCost;
                resultSheet.Cells[rows, 6].Value = ((ContractDisplayStatus)item.DisplayStatus).GetDisplayName();
                resultSheet.Cells[rows, 7].Value = item.MaturityDate.ToString("dd.MM.yyyy");
                resultSheet.Cells[rows, 8].Value = item.ClientName;
                resultSheet.Cells[rows, 9].Value = item.CategoryName;

                column = 9;
                if (model.CollateralType == CollateralType.Gold)
                {
                    resultSheet.Cells[rows, ++column].Value = item.TotalWeight;
                    resultSheet.Cells[rows, ++column].Value = item.SpecificWeight;
                    resultSheet.Cells[rows, ++column].Value = item.Purity;
                }
                else if (model.CollateralType == CollateralType.Car || model.CollateralType == CollateralType.Machinery)
                {
                    resultSheet.Cells[rows, ++column].Value = item.HasCasco != null && item.HasCasco == 1 ? "Да" : "Нет";                    
                }
                
                if (model.DisplayStatus.HasValue && model.DisplayStatus.Value == ContractDisplayStatus.Prolong)
                {
                    resultSheet.Cells[rows, ++column].Value = ((DateTime?)item.ProlongDate).HasValue ? ((DateTime?)item.ProlongDate).Value.ToString("dd.MM.yyyy") : null;
                    resultSheet.Cells[rows, ++column].Value = item.ProlongCost;
                    resultSheet.Cells[rows, ++column].Value = item.ActionAuthor;
                }

                resultSheet.Cells[rows, ++column].Value = ((DateTime?)item.BuyoutDate).HasValue ? ((DateTime?)item.BuyoutDate).Value.ToString("dd.MM.yyyy") : null;
                resultSheet.Cells[rows, ++column].Value = item.ContractAuthor;

                rows++;
                sum += item.LoanCost;
            }

            resultSheet.Cells[$"E{rows}"].Value = sum;
        }
    }
}