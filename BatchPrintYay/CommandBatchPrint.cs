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
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace BatchPrintYay
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class CommandBatchPrint : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new RbsLogger.Logger("BatchPrint"));
            Trace.WriteLine("Print started");
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Trace.WriteLine($"Assembly version: {version}");

            App.assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            Selection sel = commandData.Application.ActiveUIDocument.Selection;
            Document mainDoc = commandData.Application.ActiveUIDocument.Document;
            
            string mainDocTitle = SheetSupport.GetDocTitleWithoutRvt(mainDoc.Title);

            YayPrintSettings printSettings = YayPrintSettings.GetSavedPrintSettings();
            printSettings.dwgProfiles = DwgSupport.GetAllDwgExportSettingsNames(mainDoc);


            //листы из всех открытых файлов, ключ - имя файла, значение - список листов
            Dictionary<string, List<MySheet>> allSheets = SheetSupport.GetAllSheets(commandData, printSettings);

            //получаю выбранные листы в диспетчере проекта
            List<ElementId> selIds = sel.GetElementIds().ToList();
            //List<MySheet> mSheets0 = new List<MySheet>();
            bool sheetsIsChecked = false;
            foreach (ElementId id in selIds)
            {
                Element elem = mainDoc.GetElement(id);
                ViewSheet sheet = elem as ViewSheet;
                if (sheet == null) continue;
                sheetsIsChecked = true;

                MySheet sheetInBase = allSheets[mainDocTitle].Where(i => i.sheet.GetElementId() == sheet.GetElementId()).First();
                sheetInBase.IsPrintable = true;

                //mSheets0.Add(new MySheet(sheet));
            }
            if (!sheetsIsChecked)
            {
                message = MyStrings.MessageNoSelectedSheets;
                Trace.WriteLine("Print has been stopped, no selected sheets");
                return Result.Failed;
            }


            //запись статистики по файлу
            //ProjectRating.Worker.Execute(commandData);

            //очистка старых Schema при необходимости
            /*try
            {
                Autodesk.Revit.DB.ExtensibleStorage.Schema sch =
                     Autodesk.Revit.DB.ExtensibleStorage.Schema.Lookup(new Guid("414447EA-4228-4B87-A97C-612462722AD4"));
                Autodesk.Revit.DB.ExtensibleStorage.Schema.EraseSchemaAndAllEntities(sch, true);

                Autodesk.Revit.DB.ExtensibleStorage.Schema sch2 =
                     Autodesk.Revit.DB.ExtensibleStorage.Schema.Lookup(new Guid("414447EA-4228-4B87-A97C-612462722AD5"));
                Autodesk.Revit.DB.ExtensibleStorage.Schema.EraseSchemaAndAllEntities(sch2, true);
                Trace.WriteLine("Schema очищены");
            }
            catch
            {
                Trace.WriteLine("Не удалось очистить Schema");
            }
            */

            FormPrint form = new FormPrint(allSheets, printSettings);
            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK) return Result.Cancelled;
            Trace.WriteLine("Click OK, print has started");
            printSettings = form.printSettings;

            string printerName = printSettings.printerName;
            allSheets = form.sheetsSelected;
            Trace.WriteLine("Selected sheets");
            foreach(var kvp in allSheets)
            {
                Trace.WriteLine(" File " + kvp.Key);
                foreach(MySheet ms in kvp.Value)
                {
                    Trace.WriteLine("  Sheet " + ms.sheet.Name);
                }
            }

            string outputFolderCommon = printSettings.outputFolder;

            YayPrintSettings.SaveSettings(printSettings);
            Trace.WriteLine("Print settings is saved");

            //Дополнительные возможности работают только с PDFCreator
            if (printerName != "PDFCreator")
            {
                if (printSettings.colorsType == ColorType.MonochromeWithExcludes || printSettings.mergePdfs || printSettings.useOrientation)
                {
                    string errmsg = MyStrings.MessagePrinterNotPdfCreator;
                    TaskDialog.Show("Warning", errmsg);
                    printSettings.mergePdfs = false;
                    printSettings.excludeColors = new List<PdfColor>();
                    printSettings.useOrientation = false;
                    Trace.WriteLine("Settings is not compatible for " + printerName);
                }
            }
            else
            {
                if (!printSettings.useOrientation)
                {
                    SupportRegistry.SetOrientationForPdfCreator(OrientationType.Automatic);
                    Trace.WriteLine("Sheet orientation is Automatic");
                }
            }
            bool printToFile = form.printToFile;
            string outputFolder = "";
            if (printToFile)
            {
                outputFolder =  PrintSupport.CreateFolderToPrint(mainDoc, printerName, outputFolderCommon);
                Trace.WriteLine("Folder for print is created: " + outputFolder);
            }
            //List<string> pfdFileNames = new List<string>();
            
            //печатаю листы из каждого выбранного revit-файла
            List<MySheet> printedSheets = new List<MySheet>();
            foreach (string docTitle in allSheets.Keys)
            {
                Document openedDoc = null;
                Trace.WriteLine("Print from file: " + docTitle);

                RevitLinkType rlt = null;

                //проверяю, текущий это документ или полученный через ссылку
                if (docTitle == mainDocTitle)
                {
                    openedDoc = mainDoc;
                    Trace.WriteLine("This is not a link document");
                }
                else
                {
                    List<RevitLinkType> linkTypes = new FilteredElementCollector(mainDoc)
                        .OfClass(typeof(RevitLinkType))
                        .Cast<RevitLinkType>()
                        .Where(i => SheetSupport.GetDocTitleWithoutRvt(i.Name) == docTitle)
                        .ToList();
                    if (linkTypes.Count == 0) throw new Exception("Cant find opened link file " + docTitle);
                    rlt = linkTypes.First();

                    //проверю, не открыт ли уже документ, который пытаемся печатать
                    foreach (Document testOpenedDoc in commandData.Application.Application.Documents)
                    {
                        if (testOpenedDoc.IsLinked) continue;
                        if (testOpenedDoc.Title == docTitle || testOpenedDoc.Title.StartsWith(docTitle) || docTitle.StartsWith(testOpenedDoc.Title))
                        {
                            openedDoc = testOpenedDoc;
                            Trace.WriteLine("It is an opened link document");
                        }
                    }

                    //иначе придется открывать документ через ссылку
                    if (openedDoc == null)
                    {
                        Trace.WriteLine("It is a closed link document. Try to open");
                        List<Document> linkDocs = new FilteredElementCollector(mainDoc)
                            .OfClass(typeof(RevitLinkInstance))
                            .Cast<RevitLinkInstance>()
                            .Select(i => i.GetLinkDocument())
                            .Where(i => i != null)
                            .Where(i => SheetSupport.GetDocTitleWithoutRvt(i.Title) == docTitle)
                            .ToList();
                        if(linkDocs.Count == 0) throw new Exception("Cant find link file " + docTitle);
                        Document linkDoc = linkDocs.First();

                        if (linkDoc.IsWorkshared)
                        {
                            Trace.WriteLine("It is a workshared file, try to open with detouch");
                            ModelPath mpath = linkDoc.GetWorksharingCentralModelPath();
                            OpenOptions oo = new OpenOptions();
                            oo.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
                            WorksetConfiguration wc = new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);
                            oo.SetOpenWorksetsConfiguration(wc);
                            rlt.Unload(new SaveCoordinates());
                            openedDoc = commandData.Application.Application.OpenDocumentFile(mpath, oo);
                        }
                        else
                        {
                            Trace.WriteLine("It is a single-user file");
                            string docPath = linkDoc.PathName;
                            rlt.Unload(new SaveCoordinates());
                            openedDoc = commandData.Application.Application.OpenDocumentFile(docPath);
                        }
                    }
                    Trace.WriteLine("Link file is opened succesfully");
                } //
                

                List<MySheet> mSheets = allSheets[docTitle];
                
                if (docTitle != mainDocTitle)
                {
                    List<ViewSheet> linkSheets = new FilteredElementCollector(openedDoc)
                        .OfClass(typeof(ViewSheet))
                        .Cast<ViewSheet>()
                        .ToList();
                    List<MySheet> tempSheets = new List<MySheet>();
                    foreach (MySheet ms in mSheets)
                    {
                        foreach (ViewSheet vs in linkSheets)
                        {
                            if (ms.SheetId == vs.GetElementId())
                            {
                                MySheet newMs = new MySheet(vs, printSettings.alwaysColorParamName);
                                tempSheets.Add(newMs);
                            }
                        }
                    }
                    mSheets = tempSheets;
                }
                Trace.WriteLine("Sheets found in this file: " + mSheets.Count.ToString());

                Trace.WriteLine(": " + mSheets.Count.ToString());
                PrintManager pManager = openedDoc.PrintManager;
                Trace.WriteLine("Current selected printer: " + pManager.PrinterName);
                Trace.WriteLine("Try to set printer: " + printerName);
                pManager.SelectNewPrintDriver(printerName);
                pManager = openedDoc.PrintManager;
                pManager.PrintRange = PrintRange.Current;
                pManager.Apply();
                Trace.WriteLine("Print manager setting applied successfully");


                //список основных надписей нужен потому, что размеры листа хранятся в них
                //могут быть примечания, сделанные Основной надписью, надо их отфильровать, поэтому >0.6
                List<FamilyInstance> titleBlocks = new FilteredElementCollector(openedDoc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .Cast<FamilyInstance>()
                    .Where(t => t.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble() > 0.6)
                    .ToList();
                Trace.WriteLine("Titleblocks found: " + titleBlocks.Count.ToString());


                //получаю имя формата и проверяю, настроены ли размеры бумаги в Сервере печати
                string formatsCheckinMessage = PrintSupport.PrintFormatsCheckIn(openedDoc, printerName, titleBlocks, ref mSheets);
                if (formatsCheckinMessage != "")
                {
                    message = formatsCheckinMessage;
                    Trace.WriteLine("Formats checking failed: " + message);
                    return Result.Failed;
                }
                Trace.WriteLine("Format checking success, go to print");

                //если включен экспорт dwg - нахожу параметры экспорта по имени 
                DWGExportOptions dwgOptions = null;
                if(printSettings.exportToDwg)
                {
                    List<ExportDWGSettings> curDwgSettings = DwgSupport.GetAllDwgExportSettingsNames(openedDoc)
                        .Where(i => i.Name == printSettings.selectedDwgExportProfileName)
                        .ToList();
                    if(curDwgSettings.Count == 0)
                    {
                        TaskDialog.Show("Error", MyStrings.MessageInFile + openedDoc.Title 
                            + MyStrings.MessageDwgNotFound + printSettings.selectedDwgExportProfileName);
                        dwgOptions = DwgSupport.GetAllDwgExportSettingsNames(openedDoc).First().GetDWGExportOptions();
                    }
                    else
                    {
                        dwgOptions = curDwgSettings.First().GetDWGExportOptions();
                    }
                }

                //печатаю каждый лист
                foreach (MySheet msheet in mSheets)
                {
                    Trace.WriteLine(" ");
                    Trace.WriteLine("Sheet is printing: " + msheet.sheet.Name);
                    if (printSettings.refreshSchedules)
                    {
                        SchedulesRefresh.Start(openedDoc, msheet.sheet);
                        Trace.WriteLine("Schedules is refreshed succesfully");
                    }

                    msheet.CheckIsColored(printSettings.alwaysColorParamName);          

                    using (Transaction t = new Transaction(openedDoc))
                    {
                        t.Start(MyStrings.TransactionPrintProfiles);

                        string fileName0 = "";
                        if (printSettings.mergePdfs)
                        {
                            string guid = Guid.NewGuid().ToString();
                            fileName0 = msheet.sheet.SheetNumber + "_" + guid + ".pdf";
                        }
                        else
                        {
                            fileName0 = msheet.NameByConstructor(printSettings.nameConstructor);
                        }
                        string fileName = SheetSupport.ClearIllegalCharacters(fileName0);
                        if(fileName.Length > 128)
                        {
                            Trace.WriteLine("Sheet name length is longer than 128 symbols, will be cut");
                            string cutname = fileName.Substring(0, 63);
                            cutname += fileName.Substring(fileName.Length - 64);
                            fileName = cutname;
                        }

                        if (printerName == "PDFCreator" && printSettings.useOrientation)
                        {
                            if (msheet.IsVertical)
                            {
                                SupportRegistry.SetOrientationForPdfCreator(OrientationType.Portrait);
                                Trace.WriteLine("Portrait orientation is set forcibly");
                            }
                            if (!msheet.IsVertical)
                            {
                                SupportRegistry.SetOrientationForPdfCreator(OrientationType.Landscape);
                                Trace.WriteLine("Landscape orientation is set forcibly");
                            }
                        }

                        for (int i = 0; i < msheet.titleBlocks.Count; i++)
                        {
                            MySheet printedSheet = msheet;
                            string tempFilename = "";
                            if(msheet.titleBlocks.Count > 1)
                            {
                                Trace.WriteLine("More than 1 titleblock on 1 sheet, print number " + i.ToString());
                                tempFilename = fileName.Replace(".pdf", "_" +  i.ToString() + ".pdf");
                                printedSheet = new MySheet(msheet);
                                printedSheet.SheetSubNumber = i;
                            }
                            else
                            {
                                Trace.WriteLine($"1 titleblock on sheet, Id {msheet.titleBlocks.First().GetElementId()}");
                                tempFilename = fileName;
                            }
                            
                            string fullFilename = System.IO.Path.Combine(outputFolder, tempFilename);
                            Trace.WriteLine("File full name: " + fullFilename);

                            if(fullFilename.Length > 256)
                            {
                                throw new Exception(MyStrings.ExceptionFilenameTooLong + fullFilename);
                            }
                            printedSheet.PdfFileName = fullFilename;

                            //смещаю область для печати многолистовых спецификаций
                            double offsetX = -i * printedSheet.widthMm / 25.4; //смещение задается в дюймах!
                            Trace.WriteLine("Offset X: " + offsetX.ToString("F3"));

                            PrintSetting ps = PrintSupport.CreatePrintSetting(openedDoc, pManager, printedSheet, printSettings, offsetX, 0);

                            pManager.PrintSetup.CurrentPrintSetting = ps;
                            Trace.WriteLine("Print settings are applied, " + ps.Name);


                            PrintSupport.PrintView(printedSheet.sheet, pManager, ps, tempFilename);
                            Trace.WriteLine("Sheet is send to the printer");
                            
                            printedSheets.Add(printedSheet);
                        }

                        if (printerName == "PDFCreator" && printSettings.useOrientation)
                        {
                            System.Threading.Thread.Sleep(5000);
                        }

                        t.RollBack();
                    }

                    //если включен dwg - то ещё экспортирую этот лист
                    if(printSettings.exportToDwg)
                    {
                        List<ElementId> sheetsIds = new List<ElementId> { msheet.sheet.Id };
                        string sheetname = msheet.NameByConstructor(printSettings.dwgNameConstructor);
                        openedDoc.Export(outputFolder, sheetname, sheetsIds, dwgOptions);
                    }
                }

                if (rlt != null)
                {
                    
                    openedDoc.Close(false);
#if R2017
                    RevitLinkLoadResult LoadResult = rlt.Reload();
#else
                    LinkLoadResult loadResult = rlt.Reload();
#endif
                    Trace.WriteLine("Link document is closed");
                }
            }
            int printedSheetsCount = printedSheets.Count;
            printedSheets = printedSheets
                .OrderBy(i => i.SheetNumberInt)
                .ThenBy(i => i.SheetSubNumber)
                .ToList();


            //если требуется постобработка файлов - ждем, пока они напечатаются
            if (printSettings.colorsType == ColorType.MonochromeWithExcludes || printSettings.mergePdfs)
            {
                Trace.WriteLine("Postprocessing is switched on; wait all sheets will be printed. Expected files count " + printedSheetsCount.ToString());
                int watchTimer = 0;
                while (printToFile)
                {
                    int filescount = System.IO.Directory.GetFiles(outputFolder, "*.pdf").Length;
                    Trace.WriteLine("Iteracion " + watchTimer + ", files are printed " + filescount);
                    if (filescount >= printedSheetsCount)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(500);
                    watchTimer++;
                    

                    if (watchTimer > 100)
                    {
                        BalloonTip.Show("Warning", MyStrings.MessagePrintTooMuchTime);
                        Trace.WriteLine("Unable to wait for the end of printing");
                        return Result.Failed;
                    }
                }
            }

            
            List<string> pdfFileNames = printedSheets.Select(i => i.PdfFileName).ToList();
            Trace.WriteLine("PDF files should be printed");
            foreach(string pdfname in pdfFileNames)
            {
                Trace.WriteLine("  " + pdfname);
            }
            Trace.WriteLine("PDF files are printed in fact");
            foreach (string pdfnameOut in System.IO.Directory.GetFiles(outputFolder, "*.pdf"))
            {
                Trace.WriteLine("  " + pdfnameOut);
            }

            //преобразую файл в черно-белый при необходимости
            if (printSettings.colorsType == ColorType.MonochromeWithExcludes)
            {
                Trace.WriteLine("Convert pdf to black-white");
                foreach (MySheet msheet in printedSheets)
                {
                    if (msheet.ForceColored)
                    {
                        Trace.WriteLine("Sheet is marked as always colored: " + msheet.sheet.Name);
                        continue;
                    }

                    string file = msheet.PdfFileName;
                    string outFile = file.Replace(".pdf", "_OUT.pdf");
                    Trace.WriteLine("File will be converted from " + file + " to " + outFile);

                    pdf.PdfWorker.SetExcludeColors(printSettings.excludeColors);
                    pdf.PdfWorker.ConvertToGrayScale(file, outFile);

                    //GrayscaleConvertTools.ConvertPdf(file, outFile, ColorType.Grayscale, new List<ExcludeRectangle> { rect, rect2 });

                    System.IO.File.Delete(file);
                    System.IO.File.Move(outFile, file);
                    Trace.WriteLine("Sheet os converted successfully");
                }
            }



            //объединяю файлы при необходимости
            if (printSettings.mergePdfs)
            {
                Trace.WriteLine(" ");
                Trace.WriteLine("\nMerge pdf files");
                System.Threading.Thread.Sleep(500);
                string combinedFile = System.IO.Path.Combine(outputFolder, mainDoc.Title + ".pdf");

                BatchPrintYay.pdf.PdfWorker.CombineMultiplyPDFs(pdfFileNames, combinedFile);

                foreach (string file in pdfFileNames)
                {
                    System.IO.File.Delete(file);
                    Trace.WriteLine("File has been deleted " + file);
                }
                Trace.WriteLine("Merge success");
            }

            if (printToFile)
            {
                System.Diagnostics.Process.Start(outputFolder);
                Trace.WriteLine("Folder is opened " + outputFolder);
            }

            //восстанавливаю настройки PDFCreator
            //if(printerName == "PDFCreator")
            //{
            //    SupportRegistry.RestoreSettingsForPDFCreator();
            //}


            string msg = MyStrings.MessageSheetsPrinted + printedSheetsCount;
            BalloonTip.Show(MyStrings.MessageFinishPrintTItle, msg);
            Trace.WriteLine("Print has been finished successfully, sheets have been printed: " + printedSheetsCount);
            return Result.Succeeded;
        }
    }
}
