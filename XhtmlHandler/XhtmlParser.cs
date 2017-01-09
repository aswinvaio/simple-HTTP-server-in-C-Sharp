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
        public XhtmlParser(String filename)
        {
            throw new NotImplementedException();
        }
        public XhtmlParser(XDocument doc)
        {
            this.xhtmldoc = doc;
            if (validateXml(doc)) { throw new Exception("failed to parse xhtml file"); }
            
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
