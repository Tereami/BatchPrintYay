#region License
/*Данный код опубликован под лицензией Creative Commons Attribution-ShareAlike.
Разрешено использовать, распространять, изменять и брать данный код за основу для производных в коммерческих и
некоммерческих целях, при условии указания авторства и если производные лицензируются на тех же условиях.
Код поставляется "как есть". Автор не несет ответственности за возможные последствия использования.
Зуев Александр, 2020, все права защищены.
This code is listed under the Creative Commons Attribution-ShareAlike license.
You may use, redistribute, remix, tweak, and build upon this work non-commercially and commercially,
as long as you credit the author by linking back and license your new creations under the same terms.
This code is provided 'as is'. Author disclaims any implied warranty.
Zuev Aleksandr, 2020, all rigths reserved.*/
#endregion
#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit.DB;
#endregion

namespace BatchPrintYay
{
    public static class PrintSupport
    {
        public static string CreateFolderToPrint(Document doc, string printerName, string outputFolder)
        {
            string docTitle = SheetSupport.ClearIllegalCharacters(doc.Title);
            string docTitle2 = docTitle.Replace(".rvt", "");
            string folder2 = docTitle2 + "_" + DateTime.Now.ToString("yyyy MM dd H mm ss");

            string newFolder = System.IO.Path.Combine(outputFolder, folder2);
            try
            {
                System.IO.Directory.CreateDirectory(newFolder);
            }
            catch
            {
                string msg = MyStrings.MessageUnableToSaveFiles + newFolder;
                Debug.WriteLine(msg);
                return msg;
            }

            //пробуем настроить PDFCreator через реестр Windows, для автоматической печати в папку
            if (printerName == "PDFCreator")
            {
                SupportRegistry.ActivateSettingsForPDFCreator(newFolder);
            }
            return newFolder;
        }


        /// <summary>
        /// Ищет и назначает форматы для листов, при необходимости создает форматы в Сервере печати
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="titleBlocks"></param>
        /// <param name="mSheets"></param>
        /// <returns></returns>
        public static string PrintFormatsCheckIn(Document doc, string printerName, List<FamilyInstance> titleBlocks, ref List<MySheet> mSheets)
        {
            PrintManager pManager = doc.PrintManager;
            foreach (MySheet msheet in mSheets)
            {
                Debug.WriteLine(" ");
                Debug.WriteLine(" Sheet is checking " + msheet.sheet.Name);
                double widthMm = 0;
                double heigthMm = 0;

                List<FamilyInstance> tempTitleBlocks = titleBlocks
                    .Where(i => i.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString() == msheet.sheet.SheetNumber)
                    .ToList();

                Debug.WriteLine(" Titleblocks found: " + tempTitleBlocks.Count.ToString());
                if (tempTitleBlocks.Count == 0)
                {
                    return MyStrings.MessageNoTitleblockOnSheet + msheet.sheet.Name;
                }



                msheet.titleBlocks = tempTitleBlocks;
                FamilyInstance titleBlock = tempTitleBlocks.First();
                Debug.WriteLine(" Id of used titleblock " + titleBlock.Id.IntegerValue.ToString());

                widthMm = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble() * 304.8;
                Debug.WriteLine(" BuiltInParameter.SHEET_WIDTH = " + widthMm.ToString("F3"));

                heigthMm = titleBlock.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble() * 304.8;
                Debug.WriteLine(" BuiltInParameter.SHEET_HEIGHT = " + heigthMm.ToString("F3"));

                Debug.WriteLine(" Check titleblock is correct ");
                string sizeCheckMessage = SheetSupport.CheckTitleblocSizeCorrects(msheet.sheet, titleBlock);
                if (sizeCheckMessage != "")
                {
                    return sizeCheckMessage;
                }


                widthMm = Math.Round(widthMm);
                msheet.widthMm = widthMm;
                heigthMm = Math.Round(heigthMm);
                msheet.heigthMm = heigthMm;



                //определяю ориентацию листа
                if (widthMm > heigthMm)
                {
                    Debug.WriteLine(" Sheet is vertical oriented");
                    msheet.IsVertical = false;
                }
                else
                {
                    Debug.WriteLine(" Sheet is horizontal oriented");
                    msheet.IsVertical = true;
                }

                System.Drawing.Printing.PaperSize winPaperSize = PrinterUtility.GetPaperSize(printerName, widthMm, heigthMm);
                

                if (winPaperSize != null) //есть подходящий формат
                {
                    Debug.WriteLine(" Found sheet with Windows format: " + winPaperSize.PaperName);
                    pManager = doc.PrintManager;
                    string paperSizeName = winPaperSize.PaperName;
                    PaperSize revitPaperSize = PrintSupport.SearchRevitPaperSizeByName(pManager, paperSizeName);
                    

                    if (revitPaperSize == null)
                    {
                        string message = MyStrings.MessageUnableToApplyRevitSheetFormat;
                        message += msheet.sheet.SheetNumber + " : " + msheet.sheet.Name + ". Format: " + paperSizeName;
                        Debug.WriteLine("  " + message);
                        return message;
                    }

                    Debug.WriteLine(" Found Revit sheet format: " + revitPaperSize.Name);
                    msheet.revitPaperSize = revitPaperSize;
                }
                else //нет такого формата, нужно добавить в Сервер печати
                {
                    string paperSizeName = widthMm.ToString("F0") + "x" + heigthMm.ToString("F0");
                    Debug.WriteLine("Windows sheet format isnt found! " + paperSizeName);
                    FormCreateCustomFormat formccf = new FormCreateCustomFormat(msheet.sheet.Title, paperSizeName);
                    formccf.ShowDialog();
                    if (formccf.DialogResult != System.Windows.Forms.DialogResult.OK) return "cancel";

                    paperSizeName = "UnknownFormat_" + paperSizeName;
                    Debug.WriteLine(" Try to add sheet format to local print server " + paperSizeName);

                    try
                    {
                        PrinterUtility.AddFormatToAnyPdfPrinter(paperSizeName, widthMm / 10, heigthMm / 10);
                        Debug.WriteLine(" Sheet format has been added succesfully!");
                    }
                    catch (Exception ex)
                    {
                        string msg = MyStrings.MessageUnableToCreateSheetFormat + paperSizeName + MyStrings.MessageNoAdmin + ex.Message;
                        Debug.WriteLine("  " + msg);
                        return msg;
                    }


                    System.Threading.Thread.Sleep(100);


                    pManager = doc.PrintManager;

                    Debug.WriteLine(" Find sheet format again " + paperSizeName);
                    PaperSize revitPaperSize = PrintSupport.SearchRevitPaperSizeByName(pManager, paperSizeName);
                    if (revitPaperSize == null)
                    {
                        string message = MyStrings.MessageNonStandardList;
                        message += msheet.sheet.SheetNumber + " : " + msheet.sheet.Name + ". Format " + paperSizeName;
                        Debug.WriteLine("  " + message);
                        return message;
                    }

                    Debug.WriteLine(" Format is applied successfully: " + revitPaperSize.Name);
                    msheet.revitPaperSize = revitPaperSize;
                }
            }

            return string.Empty;
        }





        /// <summary>
        /// Печатает вид с заданными настройками печати.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="pManager"></param>
        /// <param name="ps"></param>
        /// <param name="fileName"></param>
        public static void PrintView(View view, PrintManager pManager, PrintSetting ps, string fileName)
        {
            pManager.PrintSetup.CurrentPrintSetting = ps;

            fileName = @"C:\" + fileName;

            pManager.PrintToFileName = fileName;
            pManager.Apply();
            pManager.SubmitPrint(view);
            pManager.Apply();
        }



        /// <summary>
        /// Проверяет, были ли внесены изменения в параметры печати.
        /// Ревит возвращает ошибку, если попытаться сохранить параметры печати, не изменив их.
        /// </summary>
        /// <param name="pset"></param>
        /// <param name="printSettings"></param>
        /// <returns></returns>
        public static bool PrintSettingsEquals(PrintSetting pset, YayPrintSettings printSettings)
        {
            PrintParameters pParams = pset.PrintParameters;
            bool c1 = printSettings.colorsType.Equals(pParams.ColorDepth);
            bool c2 = printSettings.hiddenLineProcessing.Equals(pParams.HiddenLineViews);
            bool c3 = printSettings.rasterQuality.Equals(pParams.RasterQuality);

            bool check = c1 && c2 && c3;
            return check;
        }


        public static PrintSetting CreatePrintSetting(Document doc, PrintManager pManager, MySheet mSheet, YayPrintSettings printSettings, double offsetX, double offsetY)
        {
            PrintSetup pSetup = pManager.PrintSetup;

            IPrintSetting ps = pSetup.InSession as IPrintSetting;
            PrintParameters pps = ps.PrintParameters;

            pps.HideCropBoundaries = true;
            pps.HideReforWorkPlanes = true;
            pps.HideScopeBoxes = true;
            pps.HideUnreferencedViewTags = false;
            pps.ZoomType = ZoomType.Zoom;
            pps.Zoom = 100;

            pps.PaperPlacement = PaperPlacementType.Margins;
            pps.MarginType = MarginType.UserDefined;
            pps.UserDefinedMarginX = offsetX;
            pps.UserDefinedMarginY = offsetY;

            //RasterQualityType rqt =(RasterQualityType)Enum.Parse(typeof(RasterQualityType), printSettings.rasterQuality);
            pps.RasterQuality = printSettings.rasterQuality;

            //HiddenLineViewsType hlvt = (HiddenLineViewsType)Enum.Parse(typeof(HiddenLineViewsType), printSettings.hiddenLineProcessing);
            pps.HiddenLineViews = printSettings.hiddenLineProcessing;

            ColorDepthType cdt = ColorDepthType.Color;
            if (printSettings.colorsType == ColorType.Monochrome)
                cdt = ColorDepthType.BlackLine;
            if (printSettings.colorsType == ColorType.GrayScale)
                cdt = ColorDepthType.GrayScale;
            pps.ColorDepth = cdt;



            if (mSheet.revitPaperSize == null)
            {
                string msg = MyStrings.MessageSheetFormatNotFound
                    + mSheet.sheet.SheetNumber + " : " + mSheet.sheet.Name + MyStrings.MessageDefaultSheetFormatSet;
                Autodesk.Revit.UI.TaskDialog.Show("Error", msg);

                foreach (PaperSize curPsize in pManager.PaperSizes)
                {
                    if (curPsize.Name.Equals("A4"))
                    {
                        ps.PrintParameters.PaperSize = curPsize;
                        mSheet.IsVertical = true;
                    }
                }
            }
            else
            {
                try
                {
                    ps.PrintParameters.PaperSize = mSheet.revitPaperSize;
                }
                catch (Exception ex)
                {
                    string msg = MyStrings.MessageUnableToSetSheetSize + mSheet.revitPaperSize.Name
                        + MyStrings.MessageDefaultSizeAndStartAgain + ex.Message;
                    Autodesk.Revit.UI.TaskDialog.Show("Error", msg);
                }
            }

            if (mSheet.IsVertical)
                pps.PageOrientation = PageOrientationType.Portrait;
            else
                pps.PageOrientation = PageOrientationType.Landscape;

            pSetup.CurrentPrintSetting = ps;
            string printSetupName = "YayPrint" + DateTime.Now.ToShortTimeString() + "x" + (offsetX * 25.4).ToString("F0");
            pSetup.SaveAs(printSetupName);
            // pManager.Apply();

            doc.Regenerate();

            PrintSetting yayPs = new FilteredElementCollector(doc)
                .OfClass(typeof(PrintSetting))
                .Where(i => i.Name == printSetupName)
                .Cast<PrintSetting>()
                .First();
            return yayPs;
        }





        /// <summary>
        /// Находит формат листа
        /// </summary>
        /// <param name="pManager"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        //public static PaperSize SearchPaperSize(PrintManager pManager, SheetFormat format)
        //{
        //    PaperSizeSet pss = pManager.PaperSizes;
        //    if (format.PrinterPaperSizes == null) return null;
        //    foreach (PaperSize pSize in pss)
        //    {
        //        for (int i = 0; i < format.PrinterPaperSizes.Count; i++)
        //        {
        //            string paperSizeName = format.PrinterPaperSizes[i];
        //            bool check = StringAwesomeEquals(pSize.Name, paperSizeName);
        //            if (check)
        //            {
        //                return pSize;
        //            }
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// Находит формат листа в Revit по его имени
        /// </summary>
        /// <param name="pManager"></param>
        /// <param name="formatName"></param>
        /// <returns></returns>
        public static PaperSize SearchRevitPaperSizeByName(PrintManager pManager, string formatName)
        {
            PaperSizeSet pss = pManager.PaperSizes;
            foreach (PaperSize pSize in pss)
            {
                if (pSize.Name == formatName)
                {
                    return pSize;
                }
            }
            return null;
        }



        /// <summary>
        /// Сравнивает дви строки, игнорируя регистр и русские буквы, замененные аналогичными английскими
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool StringAwesomeEquals(string s1, string s2)
        {
            s1 = ConvertToUpperEnglishChars(s1);
            s2 = ConvertToUpperEnglishChars(s2);

            bool check = string.Equals(s1, s2);

            return check;
        }

        private static string ConvertToUpperEnglishChars(string s)
        {
            s = s.ToUpper();

            s = s.Replace('А', 'A');
            s = s.Replace('В', 'B');
            s = s.Replace('Е', 'E');
            s = s.Replace('К', 'K');
            s = s.Replace('М', 'M');
            s = s.Replace('Н', 'H');
            s = s.Replace('О', 'O');
            s = s.Replace('Р', 'P');
            s = s.Replace('С', 'C');
            s = s.Replace('Т', 'T');
            s = s.Replace('Х', 'X');

            return s;
        }
    }
}
