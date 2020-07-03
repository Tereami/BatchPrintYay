using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BatchPrintYay
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class CommandRefreshSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<ViewSheet> sheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(i => !i.IsPlaceholder)
                .ToList();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Обновление спецификаций");
                foreach (ViewSheet sheet in sheets)
                {
                    SchedulesRefresh.Start(doc, sheet);
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
}
