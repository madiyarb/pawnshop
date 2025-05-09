using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Tests.Web.StringFormatTest
{
    [TestClass()]
    public class StringFormatTesting
    {
        [TestMethod()]
        public void SumStringFormat()
        {
            string str1 = string.Format("{0:N}", 1000000000000);
            string str2 = string.Format("{0:#,###0.#}", 1000000000000);
            string str3 = string.Format("{0:0,0.0}", 1000000000000);

            
        }
    }
}
