#region License
/*Данный код опубликован под лицензией Creative Commons Attribution-ShareAlike.
Разрешено использовать, распространять, изменять и брать данный код за основу для производных в коммерческих и
некоммерческих целях, при условии указания авторства и если производные лицензируются на тех же условиях.
Код поставляется "как есть". Автор не несет ответственности за возможные последствия использования.
Зуев Александр, 2024, все права защищены.
This code is listed under the Creative Commons Attribution-ShareAlike license.
You may use, redistribute, remix, tweak, and build upon this work non-commercially and commercially,
as long as you credit the author by linking back and license your new creations under the same terms.
This code is provided 'as is'. Author disclaims any implied warranty.
Zuev Aleksandr, 2024, all rigths reserved.*/
#endregion
#region usings
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion


namespace BatchPrintYay
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CommandWriteLinkTitleblock : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new RbsLogger.Logger("BatchPrint"));
            Trace.WriteLine("Start CommandWriteLinkTitleblock");
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Trace.WriteLine($"Assembly version: {version}");
            App.assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            Document mainDoc = commandData.Application.ActiveUIDocument.Document;

            FamilyInstance titleBlock = getTitleblockIsSelected(commandData.Application.ActiveUIDocument);
            if (titleBlock == null)
            {
                message = "Please select a titleblock to copy properties";
                return Result.Failed;
            }

            ViewSheet openedSheet = mainDoc.ActiveView as ViewSheet;
            if (openedSheet == null)
            {
                message = "Please open a sheet to copy parameters";
                return Result.Failed;
            }
            List<MyParameterValue> sheetParameters = GetParameterValues(openedSheet);

            List<MyParameterValue> instanceParameters = GetParameterValues(titleBlock);

            ElementType titleblockType = mainDoc.GetElement(titleBlock.GetTypeId()) as ElementType;
            List<MyParameterValue> typeParameters = GetParameterValues(titleblockType);

            ProjectInfo pi = mainDoc.ProjectInformation;
            List<MyParameterValue> projectParameters = GetParameterValues(pi);

            MyRevitMainDocument myMainDoc = new MyRevitMainDocument(mainDoc);
            List<MyRevitLinkDocument> linkDocs = myMainDoc.GetLinkDocuments();

            FormSelectLinks formSelectLinks = new FormSelectLinks(linkDocs);
            if (formSelectLinks.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                Trace.WriteLine("Cancelled");
                return Result.Cancelled;
            }

            foreach (MyRevitLinkDocument myLinkDoc in formSelectLinks.selectedLinks)
            {
                myLinkDoc.OpenLinkDocument(commandData, false);

                Document linkDoc = myLinkDoc.Doc;

                using (Transaction t = new Transaction(linkDoc))
                {
                    t.Start("Write parameters");

                    foreach (MySheet mySheet in myLinkDoc.Sheets)
                    {
                        string oldSheetName = mySheet.sheet.Name;
                        string oldSheetNumber = mySheet.sheet.SheetNumber;
                        if (mySheet.titleBlocks.Count != 1)
                        {
                            message = $"More that 1 titleblock on the sheet in the file {myLinkDoc.Name}.";
                            linkDoc.Close(false);
                            return Result.Failed;
                        }
                        FamilyInstance linkTitleblockInstance = mySheet.titleBlocks[0];
                        ElementType linkTitleBlockType = linkDoc.GetElement(linkTitleblockInstance.GetTypeId()) as ElementType;

                        WriteParameterValues(linkDoc.ProjectInformation, projectParameters);

                        WriteParameterValues(mySheet.sheet, sheetParameters);

                        WriteParameterValues(linkTitleblockInstance, instanceParameters);

                        WriteParameterValues(linkTitleBlockType, typeParameters);

                        mySheet.sheet.Name = oldSheetName;
                        mySheet.sheet.SheetNumber = oldSheetNumber;
                    }
                    t.Commit();
                }

                if (linkDoc.IsWorkshared)
                {
                    TransactWithCentralOptions transOptions = new TransactWithCentralOptions();
                    SynchronizeWithCentralOptions syncOptions = new SynchronizeWithCentralOptions() { SaveLocalBefore = true };
                    RelinquishOptions relinqOptions = new RelinquishOptions(true);
                    syncOptions.SetRelinquishOptions(relinqOptions);
                    linkDoc.SynchronizeWithCentral(transOptions, syncOptions);
                }
                else
                {
                    linkDoc.Save();
                }
                linkDoc.Close(false);
            }

            return Result.Succeeded;
        }

        private List<MyParameterValue> GetParameterValues(Element elem)
        {
            List<MyParameterValue> values = new List<MyParameterValue>();
            foreach (Parameter p in elem.Parameters)
            {
                MyParameterValue mpv = new MyParameterValue(p);
                if (mpv.IsNull) continue;
                values.Add(mpv);
            }
            return values;
        }

        private void WriteParameterValues(Element elem, List<MyParameterValue> values)
        {
            foreach (Parameter p in elem.ParametersMap)
            {
                if (p.IsReadOnly) continue;
                string paramName = p.Definition.Name;
                MyParameterValue sourceValue = values.FirstOrDefault(i => i.ParameterName == paramName);
                if (sourceValue == null) continue;
                sourceValue.SetValue(p);
            }
        }

        private FamilyInstance getTitleblockIsSelected(UIDocument uidoc)
        {
            Selection sel = uidoc.Selection;
            if (sel == null) return null;
            if (sel.GetElementIds().Count != 1) return null;
            List<ElementId> selIds = sel.GetElementIds().ToList();
            Element selElem = uidoc.Document.GetElement(selIds[0]);
            if (!(selElem is FamilyInstance)) return null;

            bool isCategoryTitleblock = selElem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_TitleBlocks;
            if (!isCategoryTitleblock) return null;

            FamilyInstance fi = selElem as FamilyInstance;
            return fi;
        }
    }
}
