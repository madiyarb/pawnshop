
using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Web.Engine.Export
{
    public class InsuranceReviseRowsExcelBuilder : IExcelBuilder<List<InsuranceReviseRow>>
    {
        public Stream Build(List<InsuranceReviseRow> model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Сверка со страховой компанией"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, List<InsuranceReviseRow> model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "Номер полиса";
            resultSheet.Cells["B1"].Value = "Дата начала действия полиса";
            resultSheet.Cells["C1"].Value = "Дата окончания действия полиса";
            resultSheet.Cells["D1"].Value = "Страховая премия";
            resultSheet.Cells["E1"].Value = "Агентское вознаграждение";
            resultSheet.Cells["F1"].Value = "Итоговая страховая сумма";
            resultSheet.Cells["G1"].Value = "ФИО Клиента";
            resultSheet.Cells["H1"].Value = "ИИН Клиента";
            resultSheet.Cells["I1"].Value = "Филиал";
            resultSheet.Cells["J1"].Value = "Статус";
            resultSheet.Cells["K1"].Value = "Примечание";

            var row = 2;
            foreach (var insuranceReviseRow in model)
            {
                resultSheet.Cells[row, 1].Value = insuranceReviseRow.InsurancePolicyNumber;
                resultSheet.Cells[row, 2].Value = insuranceReviseRow.InsuranceStartDate?.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 3].Value = insuranceReviseRow.InsuranceEndDate?.ToString("dd.MM.yyyy");
                resultSheet.Cells[row, 4].Value = insuranceReviseRow.SurchargeAmount.ToString();
                resultSheet.Cells[row, 5].Value = insuranceReviseRow.AgencyFees.ToString();
                resultSheet.Cells[row, 6].Value = insuranceReviseRow.InsuranceAmount.ToString();
                resultSheet.Cells[row, 7].Value = insuranceReviseRow.ClientFullName;
                resultSheet.Cells[row, 8].Value = insuranceReviseRow.ClientIdentityNumber;
                resultSheet.Cells[row, 9].Value = insuranceReviseRow.BranchName;
                resultSheet.Cells[row, 10].Value = insuranceReviseRow.Status.ToString();
                resultSheet.Cells[row, 11].Value = insuranceReviseRow.Message;
                row++;
            }
        }
    }
}