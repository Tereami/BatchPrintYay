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
using Autodesk.Revit.DB.ExtensibleStorage;
using System.IO;
using System.Xml.Serialization;

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
        public HiddenLineViewsType hiddenLineProcessing;
        public ColorType colorsType;
        public RasterQualityType rasterQuality;

        public bool mergePdfs;
        public bool printToPaper;
        //public bool colorStamp;
        public bool useOrientation;
        public bool refreshSchedules;

        public List<PdfColor> excludeColors;

        /// <summary>
        /// Получение параметров печати
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <param name="ses">Информация о параметрах печати, полученная из ExtensibleStorage</param>
        public static YayPrintSettings GetPrintSettings(Document doc)
        {
            string programdataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string rbspath = Path.Combine(programdataPath, "RibbonBimStarter");
            if (!Directory.Exists(rbspath)) Directory.CreateDirectory(rbspath);
            string localFolder = Path.Combine(rbspath, "BatchPrintYay");
            if (!Directory.Exists(localFolder)) Directory.CreateDirectory(localFolder);
            string xmlpath = Path.Combine(localFolder, "settings.xml");

            YayPrintSettings ps;
            XmlSerializer serializer = new XmlSerializer(typeof(YayPrintSettings));

            if (File.Exists(xmlpath))
            {
                using (StreamReader reader = new StreamReader(xmlpath))
                {
                    ps = (YayPrintSettings)serializer.Deserialize(reader);
                    if (ps == null)
                    {
                        throw new Exception("Не удалось сериализовать: " + xmlpath);
                    }
                }
            }
            else
            {
                ps = YayPrintSettings.GetDefault(doc);
            }

            PrintManager pManager = doc.PrintManager;
            ps.printerName = pManager.PrinterName;

            return ps;
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
            HiddenLineViewsType HiddenLineProcessing,
            ColorType ColorsType,
            RasterQualityType RasterQuality,
            bool MergePdfs,
            bool PrintToPaper,
            List<PdfColor> ExcludeColors)
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



        private static YayPrintSettings GetDefault(Document doc)
        {
            YayPrintSettings ps = new YayPrintSettings
            {
                colorsType = ColorType.GrayScale,
                excludeColors = new List<PdfColor>
                {
                    new PdfColor(System.Drawing.Color.FromArgb(0,0,255)),
                    new PdfColor(System.Drawing.Color.FromArgb(192,192,192)),
                    new PdfColor(System.Drawing.Color.FromArgb(242,242,242))
                },
                hiddenLineProcessing = HiddenLineViewsType.RasterProcessing,
                mergePdfs = false,
                nameConstructor = "<Номер листа>_<Имя листа>.pdf",
                outputFolder = @"C:\PDF_Print",
                printerName = doc.PrintManager.PrinterName,
                printToPaper = false,
                rasterQuality = RasterQualityType.High,
                refreshSchedules = true,
                useOrientation = false
            };

            return ps;
        }

    }
}
