using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RequestProcessor
{
    class Helper
    {
        public static Dictionary<string, string> MimeTypes = new Dictionary<string, string>()
        {
            {"html", "text/html"},
            {"htm", "text/html"},
            {"png", "image/png"},
            {"jpg", "image/jpeg"},
            {"ico", "image/x-icon"}
        };

        public static void FieldReplacer(ref string message, string regex, string replacewith)
        {
            Regex rgx = new Regex(regex);
            string replaced = rgx.Replace(message, replacewith);
            message = replaced;
        }
    }
}
