using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExtensionHandler;

namespace RequestProcessor
{
    public class ftmlProcessor : IExtensionHandler
    {

        public void ProcessRequest(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            filename = filename.Substring(1);
            filename = Path.Combine("", filename); // rootdirectory
            Console.WriteLine("ftml handler :"+filename);

            string requestData = null;
            if (context.Request.HttpMethod == "GET")
            {
                if (context.Request.Url.Query.Length > 0)
                    requestData = context.Request.Url.Query.Substring(1);
            }
            else if (context.Request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestData = reader.ReadToEnd();
                }
            }


            NameValueCollection _REQUEST = new NameValueCollection();
            if (!string.IsNullOrEmpty(requestData))
            {
                _REQUEST = BuildNameValueCollection(requestData);
            }


            if (File.Exists(filename))
            {
                try
                {
                    if (context.Request.Headers["If-Modified-Since"] != null && !checkIfModified(filename, Convert.ToDateTime(context.Request.Headers["If-Modified-Since"])))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    }
                    else
                    {
                        Stream input = new FileStream(filename, FileMode.Open);
                        context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));
                        byte[] buff = replaceDelims(input, _REQUEST);
                        context.Response.OutputStream.Write(buff, 0, buff.Length);
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            }
        }

        private byte[] replaceDelims(Stream input, NameValueCollection _REQUEST)
        {
            //Stream output = new MemoryStream();
            string line;
            StreamReader rdr = new StreamReader(input);
            line = rdr.ReadToEnd();

            while (true)
            {
                Match match = Regex.Match(line, "<%=[0-9a-zA-Z]*%>");
                if (!match.Success)
                    break;
                string matchedstring = match.Value;
                string key = matchedstring.Substring(3, matchedstring.Length - 5);
                string val = _REQUEST[key];
                if (val != null)
                    line = line.Replace(matchedstring, val);
                else
                    line = line.Replace(matchedstring, "");

            }
            rdr.Close();
            rdr.Dispose();
            return Encoding.UTF8.GetBytes(line);
        }

        private NameValueCollection BuildNameValueCollection(string query)
        {
            NameValueCollection nameval = new NameValueCollection();
            string[] querySegments = query.Split('&');
            foreach (string segment in querySegments)
            {
                string[] parts = segment.Split('=');
                if (parts.Length > 0)
                {
                    string key = parts[0].Trim(new char[] { '?', ' ' });
                    string val = parts[1].Trim();
                    val = val.Replace('+', ' ');
                    val = Uri.UnescapeDataString(val);
                    val = val.Replace("&", "&amp;");
                    val = val.Replace("<", "&lt;");
                    val = val.Replace(">", "&gt;");
                    nameval.Add(key, val);
                }
            }
            return nameval;
        }

        private bool checkIfModified(String fileName, DateTime since)
        {
            FileInfo info = new FileInfo(fileName);
            DateTime lastmodified = info.LastWriteTime;
            return lastmodified > since;
        }

    }
}
