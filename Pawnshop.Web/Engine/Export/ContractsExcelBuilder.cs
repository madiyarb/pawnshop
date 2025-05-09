using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Engine.Export
{
    public class ContractsExcelBuilder : IExcelBuilder<List<Contract>>
    {
        public Stream Build(List<Contract> model)
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

        private void BuildSheet(ExcelWorksheet resultSheet, List<Contract> model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "№";
            resultSheet.Cells["B1"].Value = "Дата";
            resultSheet.Cells["C1"].Value = "Дата возврата";
            resultSheet.Cells["D1"].Value = "Статус";
            resultSheet.Cells["E1"].Value = "Сумма";
            resultSheet.Cells["F1"].Value = "Тип залога";
            resultSheet.Cells["G1"].Value = "Тип карты";
            resultSheet.Cells["H1"].Value = "Клиент";
            resultSheet.Cells["I1"].Value = "Филиал";
            resultSheet.Cells["J1"].Value = "Автор";

            var row = 2;
            foreach (var contract in model)
            {
                resultSheet.Cells[row, 1].Value = contract.ContractNumber;
                resultSheet.Cells[row, 2].Value = contract.ContractDate.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 3].Value = contract.MaturityDate.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 4].Value = contract.Status.GetDisplayName();
                resultSheet.Cells[row, 5].Value = contract.LoanCost;
                resultSheet.Cells[row, 6].Value = contract.CollateralType.GetDisplayName();
                resultSheet.Cells[row, 7].Value = contract.ContractData.Client.CardType.ToString();
                resultSheet.Cells[row, 8].Value = contract.ContractData.Client.FullName;
                resultSheet.Cells[row, 9].Value = contract.Branch.DisplayName;
                resultSheet.Cells[row, 10].Value = contract.Author.Fullname;
                row++;
            }
        }
    }
}