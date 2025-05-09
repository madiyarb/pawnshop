using System;
using System.Linq;
using System.Text;

namespace Pawnshop.Services.ClientRequisites
{
    public static class ClientRequisitesValidator
    {
        public static bool ValidateIban(string bankAccount)
        {
            bankAccount = bankAccount.ToUpper(); 
            if (String.IsNullOrEmpty(bankAccount))
                return false;
            else if (System.Text.RegularExpressions.Regex.IsMatch(bankAccount, "^[A-Z0-9]"))
            {
                bankAccount = bankAccount.Replace(" ", String.Empty);
                string bank =
                    bankAccount.Substring(4, bankAccount.Length - 4) + bankAccount.Substring(0, 4);
                int asciiShift = 55;
                StringBuilder sb = new StringBuilder();
                foreach (char c in bank)
                {
                    int v;
                    if (Char.IsLetter(c)) v = c - asciiShift;
                    else v = int.Parse(c.ToString());
                    sb.Append(v);
                }
                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }
            else
                return false;
        }

        public static bool ValidateCard(string digits)
        {
            return digits.All(char.IsDigit) && digits.Reverse()
                .Select(c => c - 48)
                .Select((thisNum, i) => i % 2 == 0
                    ? thisNum
                    : ((thisNum *= 2) > 9 ? thisNum - 9 : thisNum)
                ).Sum() % 10 == 0;
        }
    }
}
