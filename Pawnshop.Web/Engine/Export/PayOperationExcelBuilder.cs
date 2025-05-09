using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi.Extensions;
using OfficeOpenXml;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.PayOperations;

namespace Pawnshop.Web.Engine.Export
{
    public class PayOperationExcelBuilder : IExcelBuilder<List<PayOperation>>
    {
        private readonly ContractRepository _contractRepository;
        private readonly PayOperationRepository _payOperationRepository;

        public PayOperationExcelBuilder(ContractRepository contractRepository,
            PayOperationRepository payOperationRepository)
        {
            _contractRepository = contractRepository;
            _payOperationRepository = payOperationRepository;
        }

        public Stream Build(List<PayOperation> model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                BuildSheet(package.Workbook.Worksheets.Add("Операции"), model);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private void BuildSheet(ExcelWorksheet resultSheet, List<PayOperation> model)
        {
            resultSheet.Row(1).Style.Font.Bold = true;
            resultSheet.Cells["A1"].Value = "Вид операции";
            resultSheet.Cells["B1"].Value = "№ задачи на перечисление";
            resultSheet.Cells["C1"].Value = "Дата и время формирования задачи";
            resultSheet.Cells["D1"].Value = "Дата и время выполнения бухгалтером действия \"Проверить\"";
            resultSheet.Cells["E1"].Value = "Дата и время выполнения бухгалтером действия \"Вернуть на доработку\"";
            resultSheet.Cells["F1"].Value = "Дата и время исправления данных филиалом и повторной отправки на обработку Бухгалтеру";
            resultSheet.Cells["G1"].Value = "Дата и время проставления статуса \"Исполнен\"";
            resultSheet.Cells["H1"].Value = "Дата и время выполнения операции \"Сторнировать\"";
            resultSheet.Cells["I1"].Value = "Номер договора займа";
            resultSheet.Cells["J1"].Value = "Дата договора займа";
            resultSheet.Cells["K1"].Value = "Текущий статус договора во Фронт-Базе";
            resultSheet.Cells["L1"].Value = "Сумма договора займа";
            resultSheet.Cells["M1"].Value = "Сумма перечисленная с расчетного счета";
            resultSheet.Cells["N1"].Value = "Номер расчетного счета клиента";
            resultSheet.Cells["O1"].Value = "БИК Банка получателя клиента";
            resultSheet.Cells["P1"].Value = "Дт";
            resultSheet.Cells["Q1"].Value = "Кт";
            resultSheet.Cells["R1"].Value = "ФИО клиента";
            resultSheet.Cells["S1"].Value = "Филиал";
            resultSheet.Cells["T1"].Value = "Компания";
            resultSheet.Cells["U1"].Value = "Автор задачи (Менеджер филиала, который нажал на кнопку Подписать)";
            resultSheet.Cells["V1"].Value = "ФИО Бухгалтера, который перевел задачу в статус \"Проверен\"";
            resultSheet.Cells["W1"].Value = "ФИО Бухгалтера, который перевел задачу в статус \"Отказан\"";
        
            var row = 2;
            foreach (var operation in model)
            {
                var contract = _contractRepository.Get(operation.ContractId.Value);
                var savedOperation = _payOperationRepository.Get(operation.Id);
                resultSheet.Cells[row, 1].Value = savedOperation.PayType.Name;//Вид операции
                resultSheet.Cells[row, 2].Value = savedOperation.Number;//№ задачи на перечисление
                resultSheet.Cells[row, 3].Value = savedOperation.CreateDate.ToString("dd.MM.yyyy HH:mm");//Дата и время формирования задачи
                resultSheet.Cells[row, 4].Value = operation.Actions.Where(x=> x.ActionType == PayOperationActionType.Check).FirstOrDefault()?.CreateDate.ToString("dd.MM.yyyy HH:mm") ?? "Не проверен";//Дата и время выполнения бухгалтером действия "Проверить"
                resultSheet.Cells[row, 5].Value = savedOperation.Actions.Where(x => x.ActionType == PayOperationActionType.ReturnIfWrong).FirstOrDefault()?.CreateDate.ToString("dd.MM.yyyy HH:mm") ?? "Не возвращался";//Дата и время выполнения бухгалтером действия "Вернуть на доработку"
                resultSheet.Cells[row, 6].Value = savedOperation.Actions.Where(x => x.ActionType == PayOperationActionType.ChangeOrRepair).FirstOrDefault()?.CreateDate.ToString("dd.MM.yyyy HH:mm") ?? "Не возвращался";//Дата и время исправления данных филиалом и повторной отправки на обработку Бухгалтеру
                resultSheet.Cells[row, 7].Value = savedOperation.ExecuteDate.HasValue ? savedOperation.ExecuteDate.Value.ToString("dd.MM.yyyy HH:mm") : string.Empty;//Дата и время проставления статуса "Исполнен"
                resultSheet.Cells[row, 8].Value = savedOperation.DeleteDate.HasValue ? savedOperation.DeleteDate.Value.ToString("dd.MM.yyyy HH:mm") : string.Empty;//Дата и время выполнения операции "Сторнировать"
                resultSheet.Cells[row, 9].Value = contract.ContractNumber;//Номер договора займа
                resultSheet.Cells[row, 10].Value = contract.ContractDate.ToString("dd.MM.yyyy");//Дата договора займа
                resultSheet.Cells[row, 11].Value = contract.DisplayStatus.GetDisplayName();//Текущий статус договора во Фронт-Базе
                resultSheet.Cells[row, 12].Value = contract.LoanCost;//Сумма договора займа
                resultSheet.Cells[row, 13].Value = 0;//Сумма перечисленная с расчетного счета TODO: непонятно откуда брать
                resultSheet.Cells[row, 14].Value = savedOperation.Requisite.Number;//Номер расчетного счета клиента
                resultSheet.Cells[row, 15].Value = savedOperation.Requisite?.Bank?.BankIdentifierCode ?? "Не указан";//БИК Банка получателя клиента
                resultSheet.Cells[row, 16].Value = 0;//Дт TODO: непонятно откуда брать
                resultSheet.Cells[row, 17].Value = 0;//Кт TODO: непонятно откуда брать
                resultSheet.Cells[row, 18].Value = contract.ContractData.Client.FullName;//ФИО клиента
                resultSheet.Cells[row, 19].Value = contract.Branch.DisplayName;//Филиал
                resultSheet.Cells[row, 20].Value = contract.Branch.Configuration.LegalSettings.LegalName;//Компания
                resultSheet.Cells[row, 21].Value = savedOperation.Author.Fullname;//Автор задачи (Менеджер филиала, который нажал на кнопку Подписать)
                resultSheet.Cells[row, 22].Value = savedOperation.Actions.Where(x => x.ActionType == PayOperationActionType.Check).FirstOrDefault()?.Author?.Fullname ?? "Не найден";//ФИО Бухгалтера, который перевел задачу в статус "Проверен"
                resultSheet.Cells[row, 23].Value = savedOperation.Actions.Where(x => x.ActionType == PayOperationActionType.Cancel).FirstOrDefault()?.Author?.Fullname ?? "Не найден";//ФИО Бухгалтера, который перевел задачу в статус "Отказан"

                row++;
            }
        }
    }
}