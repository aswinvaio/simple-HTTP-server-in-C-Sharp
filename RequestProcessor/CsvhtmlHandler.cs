using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtensionHandler;
using System.IO;
using System.Net;


namespace RequestProcessor
{
    public class CsvhtmlHandler : IExtensionHandler
    {
        private const string htmlTableBegin = "<html><head><title>Csvhtml Rendering</title></head><body><table>";
        private const string htmlTableEnd = "</table></body></html>";
        private const string htmlFileNotFound = "<html><head><title>404</title></head>" +
            "<body><h1>404<br/>File Not Found!</h1></body></html>";

        public void ProcessRequest(HttpListenerContext context)
        {
            
            string htmlContent = "";
            string pathToFile = Directory.GetCurrentDirectory() + context.Request.Url.LocalPath;

            //replace extension with .csv
            Helper.FieldReplacer(ref pathToFile, @"\.csvhtml$", ".csv");
            try
            {
                //debug
                Console.WriteLine("pathToCsvFile: {0}", pathToFile);
                if (context.Request.Headers["If-Modified-Since"] != null && !checkIfModified(pathToFile, Convert.ToDateTime(context.Request.Headers["If-Modified-Since"])))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
                else
                {
                    TextReader tr = new StreamReader(pathToFile);
                    string csvContent = tr.ReadToEnd();  //getting the page's content

                    //context.Response.ContentType = Helper.MimeTypes["html"];
                    htmlContent = convertCsvToHtml(csvContent);
                    context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(pathToFile).ToString("r"));
                }

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File Not Found!");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                htmlContent = htmlFileNotFound;
            }
            byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);
            context.Response.OutputStream.Write( buffer, 0, buffer.Length);
        }

        private string convertCsvToHtml(string csvContent)
        {
            string htmlContent = htmlTableBegin;
            const string tableRowBegin = "<tr>";
            const string tableRowEnd = "</tr>";
            const string tableColumnBegin = "<th>";
            const string tableColumnEnd = "</th>";

            HtmlEncode(ref csvContent);
            string[] itemList = csvContent.Split('\r');
            foreach (string item in itemList)
            {
                htmlContent += tableRowBegin;

                string[] entryList = item.Split(',');
                foreach (string entry in entryList)
                {
                    htmlContent += tableColumnBegin;
                    htmlContent += entry;
                    htmlContent += tableColumnEnd;
                }

                htmlContent += tableRowEnd;
            }

            return htmlContent + htmlTableEnd;
        }

        private void HtmlEncode(ref string csvContent)
        {
            Helper.FieldReplacer(ref csvContent, @"&", "&amp");
            Helper.FieldReplacer(ref csvContent, @"\<", "&lt");
            Helper.FieldReplacer(ref csvContent, @"\>", "&gt");
        }
        private bool checkIfModified(String fileName, DateTime since)
        {
            FileInfo info = new FileInfo(fileName);
            DateTime lastmodified = info.LastWriteTime;
            return lastmodified > since;
        }

    }
}
