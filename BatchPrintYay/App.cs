using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;


namespace BatchPrintYay
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class App : IExternalApplication
    {
        public static string assemblyPath = "";
        //public static Dictionary<Document, List<ViewSheet>> allSheets
        //    = new Dictionary<Document, List<ViewSheet>>();
       

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Weandrevit";
            try { application.CreateRibbonTab(tabName); } catch { }

            assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            RibbonPanel panel1 = application.CreateRibbonPanel(tabName, "Печать2");

            PushButton btnCreate = panel1.AddItem(new PushButtonData(
                "CreateHoleTask",
                "Печать",
                assemblyPath,
                "BatchPrintYay.CommandBatchPrint")
                ) as PushButton;
            btnCreate.LargeImage = PngImageSource("BatchPrintYay.Resources.PrintBig.png");
            btnCreate.Image = PngImageSource("BatchPrintYay.Resources.PrintSmall.png");
            btnCreate.ToolTip = "Пакетная печать выбранных листов в формат PDF";

            PushButton btnRefresh = panel1.AddItem(new PushButtonData(
                "RefreshSchedules",
                "Обновить спец-и",
                assemblyPath,
                "BatchPrintYay.CommandRefreshSchedules")
                ) as PushButton;
            btnRefresh.LargeImage = PngImageSource("BatchPrintYay.Resources.UpdateBig.png");
            btnRefresh.Image = PngImageSource("BatchPrintYay.Resources.UpdateSmall.png");


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private System.Windows.Media.ImageSource PngImageSource(string embeddedPathname)
        {
            System.IO.Stream st = this.GetType().Assembly.GetManifestResourceStream(embeddedPathname);
            var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(st, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            return decoder.Frames[0];
        }

    }
}
