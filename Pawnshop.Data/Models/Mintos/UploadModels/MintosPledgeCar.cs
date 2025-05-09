using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Залог - Авто
    /// </summary>
    public class MintosPledgeCar : MintosPledge
    {
        /// <param name="car">Информация о залоге (машине)</param>
        /// <param name="contract">Договор</param>
        public MintosPledgeCar(Car car, Contract contract, decimal exchangeRate = 1)
        {
            make = car.Mark;
            model = car.Model;
            year = car.ReleaseYear;
            first_registration_date = car.TechPassportDate.Value.ToString("yyyy-MM-dd");
            valuation_date = contract.ContractDate.ToString("yyyy-MM-dd");
            valuation_amount = Math.Round((contract.LoanCost > contract.EstimatedCost ? contract.LoanCost : contract.EstimatedCost) * exchangeRate, 2);
        }

        /// <summary>
        /// Вид залога
        /// Car Loan (pledge - vehicle)
        /// </summary>
        public override string type => "vehicle";

        /// <summary>
        /// Марка
        /// </summary>
        public string make { get; set; }

        /// <summary>
        /// Модель
        /// </summary>
        public string model { get; set; }

        /// <summary>
        /// Год выпуска
        /// </summary>
        public int year { get; set; }

        /// <summary>
        /// Дата техпаспорта
        /// </summary>
        public string first_registration_date { get; set; }

        /// <summary>
        /// Дата оценки
        /// </summary>
        public string valuation_date { get; set; }

        /// <summary>
        /// Сумма оценки
        /// </summary>
        public decimal valuation_amount { get; set; }
    }
}
