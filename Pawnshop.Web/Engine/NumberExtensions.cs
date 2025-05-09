using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Web
{
    public static class NumberExtensions
    {
        private static readonly List<List<string>> _numbers = new List<List<string>>();
        private const string ZERO = "ноль";
        static NumberExtensions()
        {
            _numbers.Add(new string[] { "", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять", "десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" }.ToList());
            _numbers.Add(new string[] { "", "", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто" }.ToList());
            _numbers.Add(new string[] { "", "сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот" }.ToList());
        }

        public static string ToWords(this int intValue)
        {
            if (intValue < 0)
                throw new ArgumentOutOfRangeException(nameof(intValue), "значение аргумента не должно быть меньше нуля");

            if (intValue == 0) 
                return string.Empty;

            var stringValue = intValue.ToString();
            var length = stringValue.Length;
            var result = string.Empty;
            var numberParser = string.Empty;
            var count = 0;
            for (var p = (length - 1); p >= 0; p--)
            {
                var numberDigit = stringValue.Substring(p, 1);
                numberParser = numberDigit + numberParser;
                int outParser = 0;
                if ((numberParser.Length == 3 || p == 0) && int.TryParse(numberParser, out outParser))
                {
                    result = Parse(int.Parse(numberParser), count, stringValue) + result;
                    numberParser = string.Empty;
                    count++;
                }
            }

            return result.Trim();
        }

        /// <summary>
        /// Разделение дробного числа на целую часть и дробную часть(2 числа после запятых)
        /// Пример 10.1 должен вернуть - "десять", "десять", что является репрезентацией десять тенге и десять тиын
        /// </summary>
        /// <param name="decimalValue">Входное число</param>
        /// <returns></returns>
        public static (string, string) ToWords(this decimal decimalValue)
        {
            if (decimalValue < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalValue), "Значение аргумента не должно быть меньше нуля");

            if (decimalValue == 0) 
                return (string.Empty, string.Empty);

            decimalValue = Math.Round(decimalValue, 2);
            int intVal = (int)Math.Floor(decimalValue);
            string intValString = ToWords(intVal);
            decimal fractionalPart = decimalValue - intVal;
            string fractionalPartString = fractionalPart.ToString("#.00");
            // число которое надо умножить на число после запятой чтобы получить его целое значение, пример 0.02 превратится в 2
            int floatMultiplier = Convert.ToInt32(Math.Pow(10, fractionalPartString.Length - 1));
            int intValAfterComma = Convert.ToInt32(floatMultiplier * fractionalPart);
            string fractonalValPart = ToWords(intValAfterComma);
            return (intValString, fractonalValPart);
        }

        private static string Parse(int intValue, int desc, string sourceValue)
        {
            if (sourceValue == null)
                throw new ArgumentNullException(nameof(sourceValue));

            if(string.IsNullOrWhiteSpace(sourceValue))
                throw new ArgumentException(nameof(sourceValue));

            var stringValue = intValue.ToString();
            var result = string.Empty;
            var hundred = string.Empty;

            if (stringValue.Length == 3)
            {
                hundred = stringValue.Substring(0, 1);
                stringValue = stringValue.Substring(1, 2);
                intValue = int.Parse(stringValue);
                result = $"{_numbers[2][int.Parse(hundred)]} ";
            }

            if (intValue < 20)
            {
                result += $"{_numbers[0][intValue]} ";
            }
            else
            {
                var firstNumber = int.Parse(stringValue.Substring(0, 1));
                var secondNumber = int.Parse(stringValue.Substring(1, 1));
                result += $"{_numbers[1][firstNumber]} {_numbers[0][secondNumber]} ";
            }

            var lastNumber = 0;
            switch (desc)
            {
                case 0:
                    break;
                case 1:
                    if (sourceValue.Length > 6 && int.Parse(sourceValue.Substring(sourceValue.Length - 6, 6)) == 0) break;
                    lastNumber = int.Parse(stringValue.Substring(stringValue.Length - 1));
                    if (lastNumber == 1) result += "тысяча ";
                    else if (lastNumber > 1 && lastNumber < 5) result += "тысячи ";
                    else result += "тысяч ";
                    result = result.Replace("один ", "одна ");
                    result = result.Replace("два ", "две ");                    
                    break;
                case 2:
                    lastNumber = int.Parse(stringValue.Substring(stringValue.Length - 1));
                    if (lastNumber == 1) result += "миллион ";
                    else if (lastNumber > 1 && lastNumber < 5) result += "миллиона ";
                    else result += "миллионов ";
                    break;
                case 3:
                    lastNumber = int.Parse(stringValue.Substring(stringValue.Length - 1));
                    if (lastNumber == 1) result += "миллиард ";
                    else if (lastNumber > 1 && lastNumber < 5) result += "миллиарда ";
                    else result += "миллиардов ";
                    break;
                default:
                    break;
            }

            result = result.Replace("  ", " ");
            return result;
        }
    }
}