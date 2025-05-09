using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.Data.Models.Transfers.TransferContracts;

namespace Pawnshop.Web.Engine.Export
{
    public class TransferContractsExcelBuilder : IExcelBuilder<List<TransferContract>>
    {
        public Stream Build(List<TransferContract> model)
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

        private void BuildSheet(ExcelWorksheet resultSheet, List<TransferContract> model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "№";
            resultSheet.Cells["B1"].Value = "Статус договора";
            resultSheet.Cells["C1"].Value = "№ Договор";
            resultSheet.Cells["D1"].Value = "ИИН/БИН клиента";
            resultSheet.Cells["E1"].Value = "Сумма";
            resultSheet.Cells["F1"].Value = "Статус перевода";
            resultSheet.Cells["G1"].Value = "Примечания";

            var row = 2;
            foreach (var transferContract in model)
            {
                resultSheet.Cells[row, 1].Value = transferContract.EntryPosition;
                resultSheet.Cells[row, 2].Value = transferContract.ContractId != null ? "Договор найден" : "Договор не найден";
                resultSheet.Cells[row, 3].Value = transferContract.EntryСontractNumber;
                resultSheet.Cells[row, 4].Value = transferContract.EntryСlientIdentityNumber;
                resultSheet.Cells[row, 5].Value = transferContract.Contract != null ? transferContract.Amount : 0;
                resultSheet.Cells[row, 6].Value = transferContract.Status.GetDisplayName();
                resultSheet.Cells[row, 7].Value = transferContract.ErrorMessages;

                row++;
            }
        }
    }
}
