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
            Debug.Listeners.Clear();
            Debug.Listeners.Add(new RbsLogger.Logger("BatchPrint"));
            Debug.WriteLine("Print started");

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
                Debug.WriteLine("Печать остановлена, не выбраны листы");
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
                Debug.WriteLine("Schema очищены");
            }
            catch
            {
                Debug.WriteLine("Не удалось очистить Schema");
            }
            */

            YayPrintSettings printSettings = YayPrintSettings.GetSavedPrintSettings();
            printSettings.dwgProfiles = DwgSupport.GetAllDwgExportSettingsNames(mainDoc);
            FormPrint form = new FormPrint(allSheets, printSettings);
            form.ShowDialog();

            if (form.DialogResult != System.Windows.Forms.DialogResult.OK) return Result.Cancelled;
            Debug.WriteLine("В окне печати нажат ОК, переход к печати");
            printSettings = form.printSettings;

            string printerName = printSettings.printerName;
            allSheets = form.sheetsSelected;
            Debug.WriteLine("Выбранные для печати листы");
            foreach(var kvp in allSheets)
            {
                Debug.WriteLine(" Файл " + kvp.Key);
                foreach(MySheet ms in kvp.Value)
                {
                    Debug.WriteLine("  Лист " + ms.sheet.Name);
                }
            }

            string outputFolder = printSettings.outputFolder;

            YayPrintSettings.SaveSettings(printSettings);
            Debug.WriteLine("Настройки печати сохранены");

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
                    Debug.WriteLine("Выбранные настройки несовместимы с принтером " + printerName);
                }
            }
            else
            {
                if (!printSettings.useOrientation)
                {
                    SupportRegistry.SetOrientationForPdfCreator(OrientationType.Automatic);
                    Debug.WriteLine("Установлена ориентация листа Automatic");
                }
            }
            bool printToFile = form.printToFile;
            if (printToFile)
            {
                PrintSupport.CreateFolderToPrint(mainDoc, printerName, ref outputFolder);
                Debug.WriteLine("Создана папка для печати: " + outputFolder);
            }
            //List<string> pfdFileNames = new List<string>();
            
            //печатаю листы из каждого выбранного revit-файла
            List<MySheet> printedSheets = new List<MySheet>();
            foreach (string docTitle in allSheets.Keys)
            {
                Document openedDoc = null;
                Debug.WriteLine("Печать листов из файла " + docTitle);

                RevitLinkType rlt = null;

                //проверяю, текущий это документ или полученный через ссылку
                if (docTitle == mainDocTitle)
                {
                    openedDoc = mainDoc;
                    Debug.WriteLine("Это не ссылочный документ");
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
                            Debug.WriteLine("Это открытый ссылочный документ");
                        }
                    }

                    //иначе придется открывать документ через ссылку
                    if (openedDoc == null)
                    {
                        Debug.WriteLine("Это закрытый ссылочный документ, пытаюсь его открыть");
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
                            Debug.WriteLine("Это файл совместной работы, открываю с отсоединением");
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
                            Debug.WriteLine("Это однопользательский файл");
                            string docPath = linkDoc.PathName;
                            rlt.Unload(new SaveCoordinates());
                            openedDoc = commandData.Application.Application.OpenDocumentFile(docPath);
                        }
                    }
                    Debug.WriteLine("Файл-ссылка успешно открыт");
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
                Debug.WriteLine("Листов для печати найдено в данном файле: " + mSheets.Count.ToString());

                Debug.WriteLine(": " + mSheets.Count.ToString());
                PrintManager pManager = openedDoc.PrintManager;
                Debug.WriteLine("Текущий выбранный принтер: " + pManager.PrinterName);
                Debug.WriteLine("Попытка назначить принтер: " + printerName);
                pManager.SelectNewPrintDriver(printerName);
                pManager = openedDoc.PrintManager;
                pManager.PrintRange = PrintRange.Current;
                pManager.Apply();
                Debug.WriteLine("Настройки менеджера печати успешно применены");


                //список основных надписей нужен потому, что размеры листа хранятся в них
                //могут быть примечания, сделанные Основной надписью, надо их отфильровать, поэтому >0.6
                List<FamilyInstance> titleBlocks = new FilteredElementCollector(openedDoc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .Cast<FamilyInstance>()
                    .Where(t => t.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble() > 0.6)
                    .ToList();
                Debug.WriteLine("Найдено основных надписей: " + titleBlocks.Count.ToString());


                //получаю имя формата и проверяю, настроены ли размеры бумаги в Сервере печати
                string formatsCheckinMessage = PrintSupport.PrintFormatsCheckIn(openedDoc, printerName, titleBlocks, ref mSheets);
                if (formatsCheckinMessage != "")
                {
                    message = formatsCheckinMessage;
                    Debug.WriteLine("Проверка форматов листов неудачна: " + message);
                    return Result.Failed;
                }
                Debug.WriteLine("Проверка форматов листов выполнена успешно, переход к печати");

                //если включен экспорт dwg - нахожу параметры экспорта по имени 
                DWGExportOptions dwgOptions = null;
                if(printSettings.exportToDwg)
                {
                    List<ExportDWGSettings> curDwgSettings = DwgSupport.GetAllDwgExportSettingsNames(openedDoc)
                        .Where(i => i.Name == printSettings.selectedDwgExportProfileName)
                        .ToList();
                    if(curDwgSettings.Count == 0)
                    {
                        TaskDialog.Show("Ошибка", "В файле " + openedDoc.Title + " не найден dwg профиль " + printSettings.selectedDwgExportProfileName);
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
                    Debug.WriteLine(" ");
                    Debug.WriteLine("Печатается лист: " + msheet.sheet.Name);
                    if (printSettings.refreshSchedules)
                    {
                        SchedulesRefresh.Start(openedDoc, msheet.sheet);
                        Debug.WriteLine("Спецификации обновлены успешно");
                    }
                

                    using (Transaction t = new Transaction(openedDoc))
                    {
                        t.Start("Профили печати");

                        string fileName0 = "";
                        if (printSettings.mergePdfs)
                        {
                            fileName0 = msheet.sheet.SheetNumber + "_" + msheet.sheet.Name + ".pdf";
                        }
                        else
                        {
                            fileName0 = msheet.NameByConstructor(printSettings.nameConstructor);
                        }
                        string fileName = SheetSupport.ClearIllegalCharacters(fileName0);

                        if (printerName == "PDFCreator" && printSettings.useOrientation)
                        {
                            if (msheet.IsVertical)
                            {
                                SupportRegistry.SetOrientationForPdfCreator(OrientationType.Portrait);
                                Debug.WriteLine("Принудительно установлена Portrait ориентация");
                            }
                            if (!msheet.IsVertical)
                            {
                                SupportRegistry.SetOrientationForPdfCreator(OrientationType.Landscape);
                                Debug.WriteLine("Принудительно установлена Landscape ориентация");
                            }
                        }

                        for (int i = 0; i < msheet.titleBlocks.Count; i++)
                        {
                            string tempFilename = "";
                            if(msheet.titleBlocks.Count > 1)
                            {
                                Debug.WriteLine("На листе более 1 основной надписи! Печать части №" + i.ToString());
                                tempFilename = fileName.Replace(".pdf", "_" +  i.ToString() + ".pdf");
                            }
                            else
                            {
                                Debug.WriteLine("На листе 1 основная надпись Id " + msheet.titleBlocks.First().Id.IntegerValue.ToString());
                                tempFilename = fileName;
                            }

                            string fullFilename = System.IO.Path.Combine(outputFolder, tempFilename);
                            Debug.WriteLine("Печать в файл " + fullFilename);

                            //смещаю область для печати многолистовых спецификаций
                            double offsetX = -i * msheet.widthMm / 25.4; //смещение задается в дюймах!
                            Debug.WriteLine("Смещение печати по X: " + offsetX.ToString("F3"));

                            PrintSetting ps = PrintSupport.CreatePrintSetting(openedDoc, pManager, msheet, printSettings, offsetX, 0);

                            pManager.PrintSetup.CurrentPrintSetting = ps;
                            Debug.WriteLine("Настройки печати применены, " + ps.Name);


                            PrintSupport.PrintView(msheet.sheet, pManager, ps, tempFilename);
                            Debug.WriteLine("Лист успешно отправлен на принтер");
                            msheet.PdfFileName = fullFilename;
                            printedSheets.Add(new MySheet(msheet));
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
                        openedDoc.Export(outputFolder, msheet.NameByConstructor(printSettings.dwgNameConstructor), sheetsIds, dwgOptions);
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
                    Debug.WriteLine("Ссылочный документ закрыт");
                }
            }
            int printedSheetsCount = printedSheets.Count;
            printedSheets.Sort();

            //если требуется постобработка файлов - ждем, пока они напечатаются
            if (printSettings.colorsType == ColorType.MonochromeWithExcludes || printSettings.mergePdfs)
            {
                Debug.WriteLine("Включена постобработка файлов; ожидание окончания печати. Требуемое число файлов " + printedSheetsCount.ToString());
                int watchTimer = 0;
                while (printToFile)
                {
                    int filescount = System.IO.Directory.GetFiles(outputFolder, "*.pdf").Length;
                    Debug.WriteLine("Итерация №" + watchTimer + ", файлов напечатано " + filescount);
                    if (filescount >= printedSheetsCount)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(500);
                    watchTimer++;
                    

                    if (watchTimer > 100)
                    {
                        BalloonTip.Show("Обнаружены неполадки", "Печать PDF заняла продолжительное время или произошел сбой. Дождитесь окончания печати.");
                        Debug.WriteLine("Не удалось дождаться окончания печати");
                        return Result.Failed;
                    }
                }
            }

            
            List<string> pdfFileNames = printedSheets.Select(i => i.PdfFileName).ToList();
            Debug.WriteLine("PDF файлы которые должны быть напечатаны:");
            foreach(string pdfname in pdfFileNames)
            {
                Debug.WriteLine("  " + pdfname);
            }
            Debug.WriteLine("PDF файлы напечатанные по факту:");
            foreach (string pdfnameOut in System.IO.Directory.GetFiles(outputFolder, "*.pdf"))
            {
                Debug.WriteLine("  " + pdfnameOut);
            }

            //преобразую файл в черно-белый при необходимости
            if (printSettings.colorsType == ColorType.MonochromeWithExcludes)
            {
                Debug.WriteLine("Преобразование PDF файла в черно-белый");
                foreach (MySheet msheet in printedSheets)
                {
                    if (msheet.ForceColored)
                    {
                        Debug.WriteLine("Лист не преобразовывается в черно-белый: " + msheet.sheet.Name);
                        continue;
                    }

                    string file = msheet.PdfFileName;
                    string outFile = file.Replace(".pdf", "_OUT.pdf");
                    Debug.WriteLine("Файл будет преобразован из " + file + " в " + outFile);

                    pdf.PdfWorker.SetExcludeColors(printSettings.excludeColors);
                    pdf.PdfWorker.ConvertToGrayScale(file, outFile);

                    //GrayscaleConvertTools.ConvertPdf(file, outFile, ColorType.Grayscale, new List<ExcludeRectangle> { rect, rect2 });

                    System.IO.File.Delete(file);
                    System.IO.File.Move(outFile, file);
                    Debug.WriteLine("Лист успешно преобразован");
                }
            }



            //объединяю файлы при необходимости
            if (printSettings.mergePdfs)
            {
                Debug.WriteLine(" ");
                Debug.WriteLine("\nОбъединение PDF файлов");
                System.Threading.Thread.Sleep(500);
                string combinedFile = System.IO.Path.Combine(outputFolder, mainDoc.Title + ".pdf");

                BatchPrintYay.pdf.PdfWorker.CombineMultiplyPDFs(pdfFileNames, combinedFile);

                foreach (string file in pdfFileNames)
                {
                    System.IO.File.Delete(file);
                    Debug.WriteLine("Удален файл " + file);
                }
                Debug.WriteLine("Объединено успешно");
            }

            if (printToFile)
            {
                System.Diagnostics.Process.Start(outputFolder);
                Debug.WriteLine("Открыта папка " + outputFolder);
            }

            //восстанавливаю настройки PDFCreator
            //if(printerName == "PDFCreator")
            //{
            //    SupportRegistry.RestoreSettingsForPDFCreator();
            //}


            string msg = "Напечатано листов: " + printedSheetsCount;
            BalloonTip.Show("Печать завершена!", msg);
            Debug.WriteLine("Печать успешно завершена, напечатано листов " + printedSheetsCount);
            return Result.Succeeded;
        }
    }
}
