# simple-HTTP-server-in-C-Sharp

using ExtensionHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MyServer
{

    class Myserver
    {
        void write()
        {
        }

        string rootDirectory;
        string host;
        string port;
        string path;
        HttpListener httpListener;
        List<Extension> extensions;

        public Myserver(string host, string port, string rootDirectory)
        {
            this.host = host;
            this.port = port;
            this.rootDirectory = rootDirectory;
            this.path = Directory.GetCurrentDirectory();
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add("http://" + host + ":" + port + "/");
            this.httpListener.Prefixes.Add("http://192.168.94.20:1700/");
            this.extensions = _loadExtensionsFromConifg();
        }

        private void processRequest(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            filename = filename.Substring(1);
            filename = Path.Combine(rootDirectory, filename);
            string _extention = Path.GetExtension(filename);
            Console.WriteLine(context.Request.HttpMethod + " " + filename);
            string mime;


            if (mimeTypeMappings.TryGetValue(_extention, out mime))
            {
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
                            contextWrite(context, filename, mime);
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
            else
            {
                Extension extension = null;
                foreach (Extension ext in this.extensions)
                {
                    if (ext.Name == _extention)
                    {
                        extension = ext;
                        break;
                    }
                }
                if (extension != null)
                {
                    var Dll = Assembly.LoadFile(path + @"\" + extension.Assembly);
                    var theType = Dll.GetType(extension.Class);
                    IExtensionHandler c = Activator.CreateInstance(theType) as IExtensionHandler;
                    if (c == null)
                    {
                        throw new InvalidProgramException("Wrong implementation for extension: " + _extention);
                    }
                    context.Response.ContentType = extension.Mime;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    c.ProcessRequest(context);
                }
                else
                {
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
                                mime = "application/octet-stream";
                                contextWrite(context, filename, mime);
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
            }
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
        }

        private bool checkIfModified(String fileName, DateTime since)
        {
            FileInfo info = new FileInfo(fileName);
            DateTime dt = info.LastWriteTime;
            DateTime lastmodified = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
            return lastmodified > since;
        }

        private void contextWrite(HttpListenerContext context, string filename, string mime)
        {
            Stream output = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToUniversalTime().ToString("r"));
            context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
            context.Response.ContentType = mime;
            context.Response.ContentLength64 = output.Length;
            byte[] buffer = new byte[1024 * 16];
            int nbytes;
            while ((nbytes = output.Read(buffer, 0, buffer.Length)) > 0)
                context.Response.OutputStream.Write(buffer, 0, nbytes);
            output.Close();
        }

        public void Start()
        {

            try
            {
                this.httpListener.Start();
                Console.WriteLine("- - Server started");
                while (true)
                {
                    HttpListenerContext context = this.httpListener.GetContext();
                    Console.Write("- - - - New request : ");
                    Thread t = new Thread(() => processRequest(context));
                    t.Start();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("- - Server restart");
                if (httpListener.IsListening)
                    httpListener.Stop();
                Start();
            }
            finally
            {
                if (httpListener.IsListening)
                    httpListener.Stop();
                httpListener.Close();
                Console.WriteLine("- - Server stoped");
            }
        }

        private List<Extension> _loadExtensionsFromConifg()
        {
            List<Extension> exts = new List<Extension>();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path + @"\Server.config");
            XmlNode xnodes = xdoc.SelectSingleNode("/configuration/ExtensionHandler");

            foreach (XmlNode xnn in xnodes.ChildNodes)
            {
                string[] qclass = xnn.Attributes["class"].Value.Split(',');
                exts.Add(new Extension() { Name = xnn.Attributes["name"].Value, Mime = xnn.Attributes["mime"].Value, Class = qclass[0].Trim(), Assembly = qclass[1].Trim() + ".dll" });
            }
            return exts;
        }



        private static Dictionary<string, string> mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".mp3", "audio/mpeg"},
            {".mp4", "video/mp4"},
            {".mpeg", "video/mpeg"},
            {".pdf", "application/pdf"},
            {".png", "image/png"},
            {".xml", "text/xml"},
            {".txt", "text/plain"},
            {".cs", "text/plain"}
        };
    }
}

