using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace TGVL.MyHelper
{
    public static class FormatPhoneNumber
    {
        public static MvcHtmlString FormatPhoneNum(this HtmlHelper helper, string phoneNum)
        {
            //You could strip non-digits here to make it more robust

            if (String.IsNullOrEmpty(phoneNum)) return null;

            return new MvcHtmlString(Regex.Replace(phoneNum, @"(\d{4})(\d{3})(\d{3})", "$1 $2 $3"));  
        }
    }
}