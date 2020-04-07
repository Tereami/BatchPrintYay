using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace BatchPrintYay
{
    /// <summary>
    /// Вспомогательный класс, более удобно хранящий информацию о параметрах печати листа
    /// </summary>
    public class YayPrintSettings
    {
        public string printerName;
        public string outputFolder;
        public string nameConstructor;
        //public HiddenLineViewsType hiddenLineProcessing;
        public string hiddenLineProcessing;

        //public ColorDepthType colorsType;
        public string colorsType;

        //public RasterQualityType rasterQuality;
        public string rasterQuality;

        public bool mergePdfs;
        public bool printToPaper;
        //public bool colorStamp;
        public bool useOrientation;
        public bool refreshSchedules;

        public string excludeColors;

        /// <summary>
        /// Получение параметров печати из ExtensibleStorage документа
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <param name="ses">Информация о параметрах печати, полученная из ExtensibleStorage</param>
        public static YayPrintSettings GetPrintSettingsByDocument(Document doc, SupportExtensibleStorage ses)
        {
            

            string xmlsettings = ses.readParamFromStorage("xmlsettings");
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(YayPrintSettings));

            YayPrintSettings ps;
            using (System.IO.StringReader reader = new System.IO.StringReader(xmlsettings))
            {
                ps = (YayPrintSettings)serializer.Deserialize(reader);
            }

            PrintManager pManager = doc.PrintManager;
            ps.printerName = pManager.PrinterName;

            return ps;

            //printerName = ses.readParamFromStorage(PrintSettingFields.PrinterName);

            //outputFolder = ses.readParamFromStorage(PrintSettingFields.OutputFolder);
            //nameConstructor = ses.readParamFromStorage(PrintSettingFields.NameConstructor);

            //string stringRasterQuality = ses.readParamFromStorage(PrintSettingFields.RasterQuality);
            //int intRasterQuality = int.Parse(stringRasterQuality);
            //rasterQuality = (RasterQualityType)intRasterQuality;


            //string joinPdfs = ses.readParamFromStorage(PrintSettingFields.JoinPdf);
            //mergePdfs = bool.Parse(joinPdfs);
            //printToPaper = bool.Parse(ses.readParamFromStorage(PrintSettingFields.PrintToPaper));


            //hiddenLineProcessing = (HiddenLineViewsType)int.Parse(ses.readParamFromStorage(PrintSettingFields.HiddenLineViewsType));
            //colorsType = (ColorDepthType)int.Parse(ses.readParamFromStorage(PrintSettingFields.ColorsType));

            //colorStamp = bool.Parse(ses.readParamFromStorage(PrintSettingFields.ColorStamp));
        }

        /// <summary>
        /// Сохранение параметров печати, заданных пользователем
        /// </summary>
        /// <param name="PrinterName"></param>
        /// <param name="OutputFolder"></param>
        /// <param name="NameConstructor"></param>
        /// <param name="HiddenLineProcessing"></param>
        /// <param name="ColorsType"></param>
        /// <param name="RasterQuality"></param>
        public YayPrintSettings(string PrinterName,
            string OutputFolder,
            string NameConstructor,
            string HiddenLineProcessing,
            string ColorsType,
            string RasterQuality,
            bool MergePdfs,
            bool PrintToPaper,
            string ExcludeColors)
        {
            printerName = PrinterName;
            outputFolder = OutputFolder;
            nameConstructor = NameConstructor;
            hiddenLineProcessing = HiddenLineProcessing;
            colorsType = ColorsType;
            rasterQuality = RasterQuality;
            mergePdfs = MergePdfs;
            printToPaper = PrintToPaper;
            excludeColors = ExcludeColors;
            useOrientation = false;
        }

        /// <summary>
        /// Беспараметрический конструктор для сериализатора
        /// </summary>
        public YayPrintSettings()
        {

        }



    }
}
