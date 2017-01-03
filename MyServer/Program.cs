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
using System.Reflection;
using System.Xml;
using RequestProcessor;
using System.Threading;

// MyServer.Program, MyServer
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

    //class OldProgram
    //{
    //    static void Main2(string[] args)
    //    {
    //        TcpListener server = null;
    //        int port = 1700;
    //        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

    //        try
    //        {
    //            server = new TcpListener(localAddr, port);
    //            server.Start();

    //            Byte[] bytes = new Byte[512];
    //            String request;
    //            while (true)
    //            {
    //                Console.Write("Waiting for a connection - ");

    //                TcpClient client = server.AcceptTcpClient();
    //                Console.WriteLine("Connected!");

    //                NetworkStream stream = client.GetStream();
    //                request = "";
    //                int i;
    //                if ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
    //                {
    //                    request = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
    //                    Console.WriteLine("Received: {0}", request);


    //                    string response = @"<html><body><body><h1>Welcome</h1></body>";
    //                    byte[] msg = System.Text.Encoding.UTF8.GetBytes(response);

    //                    stream.Write(msg, 0, msg.Length);
    //                    Console.WriteLine("Sent: {0}", response);
    //                }
    //                client.Close();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //        finally
    //        {
    //            server.Stop();
    //        }
    //        Console.ReadLine();
    //    }
    //}
}
