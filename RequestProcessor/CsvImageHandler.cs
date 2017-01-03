using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtensionHandler;
using System.IO;
using System.Net;
using System.Drawing;

namespace RequestProcessor
{
    public class CsvImageHandler : IExtensionHandler
    {

        public void ProcessRequest(HttpListenerContext context)
        {
            string pathToFile = Directory.GetCurrentDirectory() + context.Request.Url.LocalPath;

            //replace extension with .csv
            Helper.FieldReplacer(ref pathToFile, @"\.csvimage$", ".csv");
            Stream st = context.Response.OutputStream;
            try
            {
                //debug
                Console.WriteLine("pathToCsvFile: {0}", pathToFile);

                //using (Stream input = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                //{
                TextReader tr = new StreamReader(pathToFile);
                string csvContent = tr.ReadToEnd();  //getting the page's content
                tr.Close();
                // replace windows style \r\n line ending with single \n
                Helper.FieldReplacer(ref csvContent, @"\r\n", "\n");
                // now remove trailing newline added by csv editor
                Helper.FieldReplacer(ref csvContent, @"\n\z", "");
                Bitmap image = ConvertCsvToImage(csvContent);

                image.Save(st, System.Drawing.Imaging.ImageFormat.Jpeg);
                context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(pathToFile).ToString("r"));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine("File Not Found! Exception {0}", e.Message);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
           // st.Position = 0;

        }

        private static Bitmap ConvertCsvToImage(string csvContent)
        {
            string[] itemList = csvContent.Split('\n');

            string[] entryNameList = itemList[0].Split(',');

            int fontSize = 14;
            int cellWidth = 100;
            int paddingHeight = 6;
            int cellHeight = fontSize + 2 * paddingHeight;
            Bitmap image = new Bitmap((int)(entryNameList.Length * cellWidth), (int)(itemList.Length * cellHeight));
            Graphics canvas = Graphics.FromImage(image);

            for (int itemNumber = 0; itemNumber < itemList.Length; itemNumber++)
            {
                string[] entryList = itemList[itemNumber].Split(',');
                for (int entryNumber = 0; entryNumber < entryList.Length; entryNumber++)
                {
                    RectangleF cell = new RectangleF(entryNumber * cellWidth, itemNumber * cellHeight, cellWidth, cellHeight);
                    canvas.DrawString(entryList[entryNumber], new Font("Tahoma", fontSize), Brushes.White, cell);
                }
            }

            return image;
        }
    }
}
