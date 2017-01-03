# Simple-HTTP-Web-Server-in-C-Sharp
Simple HTTP Web Server in C# .Net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Text.RegularExpressions;


namespace MyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Myserver ser = new Myserver("127.0.0.1", "1700", "");
            ser.Start();
            Console.ReadKey();
        }
    }

    class Myserver
    {
        string rootDirectory;
        string host;
        string port;
        HttpListener httpListener;

        public Myserver(string host, string port, string rootDirectory)
        {
            this.host = host;
            this.port = port;
            this.rootDirectory = rootDirectory;
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://" + host + ":" + port + "/");
            //httpListener.Prefixes.Add("http://192.168.94.20:1700/");
        }

        public void Start()
        {
            try
            {
                httpListener.Start();
                while (true)
                {
                    HttpListenerContext context = httpListener.GetContext();
                    processRequest(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (httpListener.IsListening)
                    httpListener.Stop();
                httpListener.Close();
            }
        }


        void processRequest(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            filename = filename.Substring(1);
            filename = Path.Combine(rootDirectory, filename);
            Console.WriteLine(filename);

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
                    Stream input = new FileStream(filename, FileMode.Open);

                    Stream output = replaceDelims(input, _REQUEST);


                    string mime;
                    context.Response.ContentType = mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = output.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = output.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    output.Close();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
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

        }

        private Stream replaceDelims(Stream input, NameValueCollection _REQUEST)
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
            return new MemoryStream(Encoding.UTF8.GetBytes(line));
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
                    nameval.Add(key, val);
                }
            }
            return nameval;
        }

        private static IDictionary<string, string> mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "video/mpeg"},
            {".pdf", "application/pdf"},
            {".png", "image/png"},
            {".xml", "text/xml"},
            {".txt", "text/plain"},
            {".cs", "text/plain"}
        };

    }

    class OldProgram
    {
        static void Main2(string[] args)
        {
            TcpListener server = null;
            int port = 1700;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            try
            {
                server = new TcpListener(localAddr, port);
                server.Start();

                Byte[] bytes = new Byte[512];
                String request;
                while (true)
                {
                    Console.Write("Waiting for a connection - ");

                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    NetworkStream stream = client.GetStream();
                    request = "";
                    int i;
                    if ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        request = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", request);


                        string response = @"<html><body><body><h1>Welcome</h1></body>";
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(response);

                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", response);
                    }
                    client.Close();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                server.Stop();
            }
            Console.ReadLine();
        }
    }
}
