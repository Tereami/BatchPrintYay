using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BatchPrintYay
{
    public class MyRevitLinkDocument : MyRevitDocument
    {
        public RevitLinkType LinkType;
        public RevitLinkInstance LinkInstance;

        private bool AllowSave = false;

        public MyRevitLinkDocument(Document doc, Selection sel = null) : base(doc, sel)
        {
        }

        public void OpenLinkDocument(ExternalCommandData commandData, bool Readonly)
        {
            //проверю, не открыт ли уже документ, который пытаемся печатать
            foreach (Document testOpenedDoc in commandData.Application.Application.Documents)
            {
                if (testOpenedDoc.IsLinked) continue;
                string testDocTitle = MyRevitDocument.GetDocNameWithoutRvt(testOpenedDoc);
                if (testDocTitle == this.Name)
                {
                    Trace.WriteLine("It is an opened link document");
                    this.Doc = testOpenedDoc;
                    return;
                }
            }

            //иначе придется открывать документ через ссылку
            Trace.WriteLine("It is a closed link document. Try to open");
            //List<Document> linkDocs = new FilteredElementCollector(mainDoc)
            //    .OfClass(typeof(RevitLinkInstance))
            //    .Cast<RevitLinkInstance>()
            //    .Select(i => i.GetLinkDocument())
            //    .Where(i => i != null)
            //    .Where(i => SheetSupport.GetDocTitleWithoutRvt(i.Title) == docTitle)
            //    .ToList();
            //if (linkDocs.Count == 0) throw new Exception("Cant find link file " + docTitle);
            //Document linkDoc = linkDocs.First();


            if (Doc.IsWorkshared)
            {
                Trace.WriteLine("It is a workshared file, try to open");
                ModelPath mpath = Doc.GetWorksharingCentralModelPath();
                OpenOptions oo = new OpenOptions();

                if (Readonly)
                    oo.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
                else
                    oo.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;

                WorksetConfiguration wc = new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);
                oo.SetOpenWorksetsConfiguration(wc);
                LinkType.Unload(new SaveCoordinates());
                Doc = commandData.Application.Application.OpenDocumentFile(mpath, oo);
            }
            else
            {
                Trace.WriteLine("It is a single-user file");
                string docPath = Doc.PathName;
                this.LinkType.Unload(new SaveCoordinates());
                Doc = commandData.Application.Application.OpenDocumentFile(docPath);
            }

            //после открытия файла надо переписать листы на него
            List<ViewSheet> linkSheets = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .ToList();
            foreach (MySheet ms in this.Sheets)
            {
                ViewSheet linkSheet = linkSheets.FirstOrDefault(i => i.GetElementId() == ms.SheetId);
                ms.sheet = linkSheet;
            }
        }
    }
}
