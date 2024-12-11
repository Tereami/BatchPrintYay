using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BatchPrintYay
{
    public abstract class MyRevitDocument : IComparable<MyRevitDocument>, IEquatable<MyRevitDocument>
    {

        public Document Doc;
        public bool SaveAfterClosing = false;
        public List<MySheet> Sheets;
        public string Name;
        public int SelectedSheetsCount
        {
            get
            {
                if (Sheets == null) return 0;
                return Sheets.Count(i => i.IsPrintable);
            }
        }

        public MyRevitDocument(Document doc, Selection sel = null)
        {
            Doc = doc;
            Name = MyRevitDocument.GetDocNameWithoutRvt(doc);
            Sheets = GetSheetsFromDocument(doc, sel);
        }

        public override string ToString()
        {
            return Name;
        }

        public static string GetDocNameWithoutRvt(Document doc)
        {
            string title = doc.Title;
            if (title.EndsWith(".rvt")) title = title.Substring(0, title.Length - 4);
            return title;
        }


        internal List<MySheet> GetSheetsFromDocument(Document doc, Selection sel = null)
        {
            List<MySheet> sheets = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .Select(i => new MySheet(i))
                    .ToList();
            sheets.Sort();

            if (sel == null) return sheets;
            List<ElementId> selIds = sel.GetElementIds().ToList();
            if (selIds.Count == 0) return sheets;

            foreach (MySheet sheet in sheets)
            {
                if (selIds.Contains(sheet.sheet.Id))
                {
                    sheet.IsPrintable = true;
                }
            }
            return sheets;
        }

        public int CompareTo(MyRevitDocument other)
        {
            return this.Name.CompareTo(other.Name);
        }

        public bool Equals(MyRevitDocument other)
        {
            return this.Name.Equals(other.Name);
        }
    }
}
