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
                message = "Please select a titleblock to copy poperties";
                return Result.Failed;
            }

            List<MyParameterValue> instanceParameters = GetParameterValues(titleBlock);

            ElementType titleblockType = mainDoc.GetElement(titleBlock.GetTypeId()) as ElementType;
            List<MyParameterValue> typeParameters = GetParameterValues(titleblockType);

            ProjectInfo pi = mainDoc.ProjectInformation;
            List<MyParameterValue> projectParameters = GetParameterValues(pi);




            return Result.Succeeded;
        }

        private List<MyParameterValue> GetParameterValues(Element elem)
        {
            List<MyParameterValue> values = new List<MyParameterValue>();
            foreach (Parameter p in elem.Parameters)
            {
                MyParameterValue mpv = new MyParameterValue(p);
                values.Add(mpv);
            }
            return values;
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
