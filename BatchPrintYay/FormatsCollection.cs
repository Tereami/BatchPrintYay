//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Autodesk.Revit.DB;
//using System.IO;

//namespace BatchPrintYay
//{
//    public static class FormatsCollection
//    {

//        /// <summary>
//        /// Проверяет, добавлены ли в Сервер печати основные используемые форматы, и при необходимости добавляет их.
//        /// </summary>
//        //public static void CheckAndInitialize(string printerName)
//        //{
//        //    string collectionFilename = Path.Combine(Path.GetDirectoryName(App.assemblyPath), "formats.txt");
//        //    string[] lines = File.ReadAllLines(collectionFilename, Encoding.UTF8);

//        //    foreach (string line in lines)
//        //    {
//        //        if (line.StartsWith("#")) continue;
//        //        string[] data = line.Split(';')[0].Split(',');

//        //        //все действия с форматами производим в книжной ориентации
//        //        double heigthMm = double.Parse(data[0]);
//        //        double widthMm = double.Parse(data[1]);
//        //        string formatName = data[2];

//        //        System.Drawing.Printing.PaperSize winPaperSize = PrinterUtility.GetPaperSize(printerName, widthMm, heigthMm);
//        //        if (winPaperSize == null)
//        //        {
//        //            PrinterUtility.AddFormatToAnyPdfPrinter(formatName, widthMm / 10, heigthMm / 10);
//        //            //PrinterUtility.AddFormat(formatName, widthMm / 10, heigthMm / 10);
//        //            System.Threading.Thread.Sleep(100);
//        //        }
//        //    }
//        //}
//    }
//}
