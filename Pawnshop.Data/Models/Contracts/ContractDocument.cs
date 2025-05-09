using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries.PrintTemplates;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractDocument : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Договор
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// Шаблон распечатки
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// Номер договора
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public int GetLinkedEntityId()
        {
            return ContractId;
        }

        public void GenerateNumber(PrintTemplateCounterConfig config, Contract contract, int counter)
        {
            Number = config.NumberFormat;
            var counterReversed = ReverseString(counter.ToString()).ToCharArray();
            var x = 0;
            var letters = ReverseString(Number).ToCharArray();
            StringBuilder result = new StringBuilder();

            foreach (var letter in letters)
            {
                if (letter == '#')
                {
                    result.Append(x >= counterReversed.Length ? '0' : counterReversed[x]);
                    x++;
                }else result.Append(letter);
            }

            Number = ReverseString(result.ToString());

            Number = ReplacePart(Number, "%branch_code%", contract.Branch.Configuration.ContractSettings.NumberCode);
            Number = ReplacePart(Number, "%year%", DateTime.Now.Year.ToString());
            Number = ReplacePart(Number, "%year_short%", DateTime.Now.Year.ToString().Substring(2, 2));
            Number = ReplacePart(Number, "%month%", DateTime.Now.Month.ToString("D2"));
            Number = ReplacePart(Number, "%product_type%", contract.ProductTypeId.HasValue ? contract.ProductTypeId.ToString() : string.Empty);
            Number = ReplacePart(Number, "%collateral_type%", contract.CollateralType.ToString());
            Number = ReplacePart(Number, "%schedule_type%", contract.Setting?.ScheduleType?.ToString());
            Number = ReplacePart(Number, "%day%", DateTime.Now.Day.ToString("D2"));
        }


        private string ReplacePart(string text, string part, string replace)
        {
            var regex = new Regex(part);
            return regex.Replace(text, replace ?? string.Empty);
        }

        public string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
    }
}
