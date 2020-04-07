using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB; //для работы с элементами модели Revit
using Autodesk.Revit.UI;

namespace BatchPrintYay
{
    public static class SchedulesRefresh
    {
        public static List<int> groupIds = new List<int>();
        public static void Start(Document doc, View sheet)
        {
            List<ScheduleSheetInstance> ssis = new FilteredElementCollector(doc)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>()
                .Where(i => i.OwnerViewId.IntegerValue == sheet.Id.IntegerValue)
                .Where(i => !i.IsTitleblockRevisionSchedule)
                .ToList();

            List<ScheduleSheetInstance> pinnedSchedules = new List<ScheduleSheetInstance>();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Обновление спецификаций 1");

                foreach (ScheduleSheetInstance ssi in ssis)
                {
                    if(ssi.Pinned && (ssi.GroupId == null || ssi.GroupId == ElementId.InvalidElementId))
                    {
                        ssi.Pinned = false;
                        pinnedSchedules.Add(ssi);
                    }
                    MoveScheduleOrGroup(doc, ssi, 0.1);
                }

                t.Commit();
            }

            groupIds.Clear();

            using (Transaction t2 = new Transaction(doc))
            {
                t2.Start("Обновление спецификаций 2");

                foreach (ScheduleSheetInstance ssi in ssis)
                {
                    MoveScheduleOrGroup(doc, ssi, -0.1);
                }

                foreach(ScheduleSheetInstance ssi in pinnedSchedules)
                {
                    ssi.Pinned = true;
                }

                t2.Commit();
            }
        }

        private static void MoveScheduleOrGroup(Document doc, ScheduleSheetInstance ssi, double distance)
        {
            if (ssi.GroupId == null || ssi.GroupId == ElementId.InvalidElementId)
            {
                ElementTransformUtils.MoveElement(doc, ssi.Id, new XYZ(distance, 0, 0));
            }
            else
            {
                Element group = doc.GetElement(ssi.GroupId);
                if(groupIds.Contains(ssi.GroupId.IntegerValue)) return;

                if(group.Pinned)
                {
                    group.Pinned = false;
                }
                ElementTransformUtils.MoveElement(doc, ssi.GroupId, new XYZ(distance, 0, 0));
                groupIds.Add(ssi.GroupId.IntegerValue);
            }
        }
    }
}
