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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace BatchPrintYay
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class CommandBatchPrint : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            App.assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            Selection sel = commandData.Application.ActiveUIDocument.Selection;
            Document mainDoc = commandData.Application.ActiveUIDocument.Document;
            
            string mainDocTitle = SheetSupport.GetDocTitleWithoutRvt(mainDoc.Title);

            //листы из всех открытых файлов, ключ - имя файла, значение - список листов
            Dictionary<string, List<MySheet>> allSheets = SheetSupport.GetAllSheets(commandData);

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

                MySheet sheetInBase = allSheets[mainDocTitle].Where(i => i.sheet.Id.IntegerValue == sheet.Id.IntegerValue).First();
                sheetInBase.IsPrintable = true;

                //mSheets0.Add(new MySheet(sheet));
            }
            if (!sheetsIsChecked)
            {
                message = "Не выбраны листы. Выберите листы в Диспетчере проекта через Shift.";
                return Result.Failed;
            }


            //запись статистики по файлу
            //ProjectRating.Worker.Execute(commandData);

            //очистка старых Schema при необходимости
            try
            {
                Autodesk.Revit.DB.ExtensibleStorage.Schema sch =
                     Autodesk.Revit.DB.ExtensibleStorage.Schema.Lookup(new Guid("414447EA-4228-4B87-A97C-612462722AD4"));
                Autodesk.Revit.DB.ExtensibleStorage.Schema.EraseSchemaAndAllEntities(sch, true);

                Autodesk.Revit.DB.ExtensibleStorage.Schema sch2 =
                     Autodesk.Revit.DB.ExtensibleStorage.Schema.Lookup(new Guid("414447EA-4228-4B87-A97C-612462722AD5"));
                Autodesk.Revit.DB.ExtensibleStorage.Schema.EraseSchemaAndAllEntities(sch2, true);
            }
            catch { }


            YayPrintSettings printSettings = YayPrintSettings.GetSavedPrintSettings();
            FormPrint form = new FormPrint(allSheets, printSettings);
            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK) return Result.Cancelled;
            printSettings = form.printSettings;

            string printerName = printSettings.printerName;
            allSheets = form.sheetsSelected;

            string outputFolder = printSettings.outputFolder;

            YayPrintSettings.SaveSettings(printSettings);

            //Дополнительные возможности работают только с PDFCreator
            if (printerName != "PDFCreator")
            {
                if (printSettings.colorsType == ColorType.MonochromeWithExcludes || printSettings.mergePdfs || printSettings.useOrientation)
                {
                    string errmsg = "Объединение PDF и печать \"Штампа\" в цвете поддерживаются только  для PDFCreator.";
                    errmsg += "\nВо избежание ошибок эти настройки будут отключены.";
                    TaskDialog.Show("Предупреждение", errmsg);
                    printSettings.mergePdfs = false;
                    printSettings.excludeColors = new List<PdfColor>();
                    printSettings.useOrientation = false;
                }
            }
            else
            {
                if (!printSettings.useOrientation)
                {
                    SupportRegistry.SetOrientationForPdfCreator(OrientationType.Automatic);
                }
            }
            bool printToFile = form.printToFile;
            if (printToFile)
            {
                PrintSupport.CreateFolderToPrint(mainDoc, printerName, ref outputFolder);
            }
            //List<string> pfdFileNames = new List<string>();
            int printedSheetCount = 0;

            //печатаю листы из каждого выбранного revit-файла
            foreach (string docTitle in allSheets.Keys)
            {
                Document openedDoc = null;

                RevitLinkType rlt = null;

                //проверяю, текущий это документ или полученный через ссылку
                if (docTitle == mainDocTitle)
                {
                    openedDoc = mainDoc;
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
                        }
                    }

                    //иначе придется открывать документ через ссылку
                    if (openedDoc == null)
                    {
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
                            rlt.Unload(new SaveCoordinates());
                            string docPath = linkDoc.PathName;
                            openedDoc = commandData.Application.Application.OpenDocumentFile(docPath);
                        }
                    }
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
                            if (ms.SheetId == vs.Id.IntegerValue)
                            {
                                MySheet newMs = new MySheet(vs);
                                tempSheets.Add(newMs);
                            }
                        }
                    }
                    mSheets = tempSheets;
                }

                PrintManager pManager = openedDoc.PrintManager;
                pManager.SelectNewPrintDriver(printerName);
                pManager = openedDoc.PrintManager;
                pManager.PrintRange = PrintRange.Current;
                pManager.Apply();


                //список основных надписей нужен потому, что размеры листа хранятся в них
                //могут быть примечания, сделанные Основной надписью, надо их отфильровать, поэтому >0.6
                List<FamilyInstance> titleBlocks = new FilteredElementCollector(openedDoc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .Cast<FamilyInstance>()
                    .Where(t => t.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble() > 0.6)
                    .ToList();

                //получаю имя формата и проверяю, настроены ли размеры бумаги в Сервере печати
                string formatsCheckinMessage = PrintSupport.PrintFormatsCheckIn(openedDoc, printerName, titleBlocks, ref mSheets);
                if (formatsCheckinMessage != "")
                {
                    message = formatsCheckinMessage;
                    return Result.Failed;
                }


                //печатаю каждый лист
                foreach (MySheet msheet in mSheets)
                {
                    if (printSettings.refreshSchedules)
                    {
                        SchedulesRefresh.Start(openedDoc, msheet.sheet);
                    }
                

                    using (Transaction t = new Transaction(openedDoc))
                    {
                        t.Start("Профили печати");

                        string fileName = msheet.NameByConstructor(printSettings.nameConstructor);

                        if (printerName == "PDFCreator" && printSettings.useOrientation)
                        {
                            if (msheet.IsVertical)
                                SupportRegistry.SetOrientationForPdfCreator(OrientationType.Portrait);
                            if (!msheet.IsVertical)
                                SupportRegistry.SetOrientationForPdfCreator(OrientationType.Landscape);
                        }



                        for (int i = 0; i < msheet.titleBlocks.Count; i++)
                        {
                            string tempFilename = "";
                            if(msheet.titleBlocks.Count > 1)
                            {
                                tempFilename = fileName.Replace(".pdf", "_" +  i.ToString() + ".pdf");
                            }
                            else
                            {
                                tempFilename = fileName;
                            }

                            string fullFilename = System.IO.Path.Combine(outputFolder, tempFilename);

                            //смещаю область для печати многолистовых спецификаций
                            double offsetX = -i * msheet.widthMm / 25.4; //смещение задается в дюймах!

                            PrintSetting ps = PrintSupport.CreatePrintSetting(openedDoc, pManager, msheet, printSettings, offsetX, 0);

                            pManager.PrintSetup.CurrentPrintSetting = ps;

                            

                            PrintSupport.PrintView(msheet.sheet, pManager, ps, tempFilename);
                            msheet.PdfFileName = fullFilename;
                            printedSheetCount++;
                        }

                        if (printerName == "PDFCreator" && printSettings.useOrientation)
                        {
                            System.Threading.Thread.Sleep(5000);
                        }

                        t.RollBack();

                        
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
                }
            }
            //если требуется постобработка файлов - ждем, пока они напечатаются
            if (printSettings.colorsType == ColorType.MonochromeWithExcludes || printSettings.mergePdfs)
            {
                int watchTimer = 0;
                while (printToFile)
                {
                    int filescount = System.IO.Directory.GetFiles(outputFolder).Length;
                    if (filescount == printedSheetCount)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(500);
                    watchTimer++;

                    if (watchTimer > 100)
                    {
                        BalloonTip.Show("Обнаружены неполадки", "Печать PDF заняла продолжительное время или произошел сбой. Дождитесь окончания печати.");
                        return Result.Failed;
                    }
                }
            }

            List<MySheet> printedSheets = new List<MySheet>();
            foreach(List<MySheet> mss in allSheets.Values)
            {
                printedSheets.AddRange(mss);
            }
            List<string> pfdFileNames = printedSheets.Select(i => i.PdfFileName).ToList();

            //преобразую файл в черно-белый при необходимости
            if (printSettings.colorsType == ColorType.MonochromeWithExcludes)
            {
                foreach (MySheet msheet in printedSheets)
                {
                    if (msheet.ForceColored) continue;
                    string file = msheet.PdfFileName;
                    string outFile = file.Replace(".pdf", "_OUT.pdf");

                    pdf.PdfWorker.SetExcludeColors(printSettings.excludeColors);
                    pdf.PdfWorker.ConvertToGrayScale(file, outFile);

                    //GrayscaleConvertTools.ConvertPdf(file, outFile, ColorType.Grayscale, new List<ExcludeRectangle> { rect, rect2 });

                    System.IO.File.Delete(file);
                    System.IO.File.Move(outFile, file);
                }
            }



            //объединяю файлы при необходимости
            if (printSettings.mergePdfs)
            {
                System.Threading.Thread.Sleep(500);
                string combinedFile = System.IO.Path.Combine(outputFolder, mainDoc.Title + ".pdf");

                BatchPrintYay.pdf.PdfWorker.CombineMultiplyPDFs(pfdFileNames, combinedFile);

                foreach (string file in pfdFileNames)
                {
                    System.IO.File.Delete(file);
                }
            }

            if (printToFile)
            {
                System.Diagnostics.Process.Start(outputFolder);
            }

            //восстанавливаю настройки PDFCreator
            //if(printerName == "PDFCreator")
            //{
            //    SupportRegistry.RestoreSettingsForPDFCreator();
            //}


            string msg = "Напечатано листов: " + printedSheetCount;
            BalloonTip.Show("Печать завершена!", msg);

            return Result.Succeeded;
        }
    }
}
