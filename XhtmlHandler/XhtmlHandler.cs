using ExtensionHandler;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System;
using System.Text;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;

namespace XhtmlHandler
{
    public class XhtmlHandler : IExtensionHandler
    {

        private bool validateXml(string fileName)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("","XhtmlSchema.xsd");

            XDocument doc = XDocument.Load(fileName);
            bool isInValid = false;
            doc.Validate(schemas, (s, e) =>
            {
                Console.WriteLine("xhtml validation : " + e.Message);
                isInValid = true;
            });
            return !isInValid;
        }


        public void ProcessRequest(HttpListenerContext context)
        {
            string xhtmlContent = "";
            string htmlContent = "";
            string pathToFile = Directory.GetCurrentDirectory() + context.Request.Url.LocalPath;
            // read the file
            try
            {
                //debug
                Console.WriteLine("pathToXhtmlFile: {0}", pathToFile);
                DateTime fileModifiedAt = File.GetLastWriteTime(pathToFile);
                string ifModifiedSinceHeader = context.Request.Headers["If-Modified-Since"];
                if (ifModifiedSinceHeader != null)
                {
                    bool fileNotModified = !checkIfModified(pathToFile, Convert.ToDateTime(context.Request.Headers["If-Modified-Since"]).ToUniversalTime());
                    if (fileNotModified)
                        context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
                else
                {
                    //Console.WriteLine(fileModifiedAt);
                    // process the xhtml content
                    
                    string classname;
                    xhtmlContent = buildHtml(pathToFile,out classname);
                    try
                    {
                        XhtmlToHtml xhtmlItem = new XhtmlToHtml(xhtmlContent, classname);
                        Dictionary<string, string> formFields = Helper.GetFormFields(context.Request);
                        xhtmlItem.WriteFormData(formFields);
                        htmlContent = xhtmlItem.PlugData();
                        context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(pathToFile).ToUniversalTime().ToString("r"));
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("One of the handle variables is unknown!"); ;
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Problem with the class name!");
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            finally
            {
                //if(tr != null)
                //    tr.Close();
            }


            byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private string buildHtml(string fileName, out string classname)
        {
            string html = "<html>\n";
            string _classname = "";
            if (validateXml(fileName))
            {
                XDocument doc = XDocument.Load(fileName);
                _classname = doc.Element("page").Attribute("class").Value;
                string head = doc.Element("page").Element("head").ToString();
                string body = doc.Element("page").Element("body").ToString();
                html += head;
                html += body;
            }
            else
            {
            }
            html += "\n</html>";
            classname = _classname;
            return html;
        }

        private bool checkIfModified(String fileName, DateTime since)
        {
            FileInfo info = new FileInfo(fileName);
            DateTime lastmodified = info.LastWriteTime.ToUniversalTime();
            //remove extra ticks after the final second
            lastmodified = lastmodified.AddTicks(-(lastmodified.Ticks % TimeSpan.TicksPerSecond));

            //file is modified if `lastmodified` is greater than `since`
            return DateTime.Compare(lastmodified, since) > 0;
        }
    }
}
