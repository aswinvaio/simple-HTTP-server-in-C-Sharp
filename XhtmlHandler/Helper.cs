using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XhtmlHandler
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

        public static Dictionary<string, string> GetFormFields(HttpListenerRequest Request)
        {
            Console.WriteLine("Http method is: {0}", Request.HttpMethod);
            Dictionary<string, string> fields = new Dictionary<string, string> { };
            if (Request.HttpMethod == "GET")
            {
                string queryString = Request.Url.Query;
                FieldReplacer(ref queryString, @"^\?", "");
                if (queryString != "")
                {
                    ConvertStringToFieldsDictionary(fields, ref queryString);
                }
            } // end get method replacer
            else if (Request.HttpMethod == "POST" && Request.ContentType == "application/x-www-form-urlencoded")
            {
                Stream body = Request.InputStream;
                Encoding encoding = Request.ContentEncoding;
                StreamReader reader = new System.IO.StreamReader(body, encoding);

                string contentString = reader.ReadToEnd();
                Console.WriteLine(contentString);

                ConvertStringToFieldsDictionary(fields, ref contentString);
            } // end post method replacer

            return fields;
        }

        public static void ConvertStringToFieldsDictionary(Dictionary<string, string> fields, ref string str)
        {
            //replace + with space and unescape string
            FieldReplacer(ref str, @"\+", " ");

            // now separate fields
            string[] fieldsarray = str.Split('&');

            // store key value pair in dictionary
            for (int itemno = 0; itemno < fieldsarray.Length; itemno++)
            {
                string[] nameValuePair = fieldsarray[itemno].Split('=');
                string name = nameValuePair[0];

                // now escape url encoded chars
                string value = Uri.UnescapeDataString(nameValuePair[1]);
                Console.WriteLine("After Unescape: {0}", value);

                //// now do html encoding
                //FieldReplacer(ref value, @"&", "&amp");
                //FieldReplacer(ref value, @"\<", "&lt");
                //FieldReplacer(ref value, @"\>", "&gt");

                fields.Add(name, value);
            }
        }
    }
}
