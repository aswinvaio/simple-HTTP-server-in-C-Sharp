using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace XhtmlHandler
{
    class XhtmlParser
    {
        XDocument xhtmldoc;
        string xhtmlContent = "";
        string htmlContent = "";
        string method = "";
        public String AssemblyInfo { get; protected set; }

        public XhtmlParser(String filename, string method) : this(XDocument.Load(filename), method) { }
        public XhtmlParser(XDocument doc,string method)
        {
            this.xhtmldoc = doc;
            if (!validateXml(doc)) { throw new Exception("failed to parse xhtml file"); }
            this.AssemblyInfo = xhtmldoc.Element("page").Attribute("class").Value;
            this.method = method;
        }

        public string Parse()
        {
            if (method == "GET") {
                generateHtmlForGET();
                return htmlContent;
            }
            return null;
        }

        private void generateHtmlForGET()
        {
            htmlContent = "";
            XElement head = xhtmldoc.Element("page");
            
        }







        private bool validateXml(XDocument doc)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", "XhtmlSchema.xsd");

            bool isInValid = false;
            doc.Validate(schemas, (s, e) =>
            {
                Console.WriteLine("xhtml validation : " + e.Message);
                isInValid = true;
            });
            return !isInValid;
        }

    }
}
