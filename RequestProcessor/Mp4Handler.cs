using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using ExtensionHandler;

namespace RequestProcessor
{
    class Mp4Handler : IExtensionHandler
    {

        public void ProcessRequest(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            filename = filename.Substring(1);
            filename = Path.Combine("", filename); // rootdirectory
            Console.WriteLine("mp4 handler :" + filename);

            if (File.Exists(filename))
            {
                try
                {

                    Stream input = new FileStream(filename, FileMode.Open);
                    //context.Response.AddHeader("Content-Disposition", "inline");
                    //return input;
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
            context.Response.OutputStream.Close();
            //return null;
        }
    }
}
