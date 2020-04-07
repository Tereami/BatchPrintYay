using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace BatchPrintYay
{

    /// <summary>
    /// Вспомогательный класс для записи и получения информации непосредственно в документе Revit
    /// </summary>
    public class SupportExtensibleStorage
    {
        private Document _doc;
        private Element _storageElem;

        private static string schemaGuid = "414447EA-4228-4B87-A97C-612462722AD3";
        //private static string fieldFolder = "OutputFolder";
        //private static string fieldConstructor = "NameConstructor";
        //private static string fieldPrinterName = "PrinterName";
        //private static string fieldPrintToPaper = "PrintToPaper";
        //private static string fieldJoinPdfs = "JoinPdf";
        //private static string fieldUseRasterProcessing = "UseRasterProcessing";
        //private static string fieldRasterQuality = "RasterQuality";
        //private static string fieldColorsType = "ColorsType";



        //public string OutputFolder
        //{
        //    get
        //    {
        //       return this.readParamFromStorage(fieldFolder);
        //    }
        //}
        //public string NameConstructor
        //{
        //    get
        //    {
        //        return this.readParamFromStorage(fieldConstructor);
        //    }
        //}



        /// <summary>
        /// Инициализация хранилища данных в документе. При первом запуске выполняется создание хранилища.
        /// </summary>
        /// <param name="Doc"></param>
        public SupportExtensibleStorage(Document Doc)
        {
            _doc = Doc;
            _storageElem = this.getStorageElement();
            StorageCheckIn();
        }



        /// <summary>
        /// Сохраняю новые значения папки для сохранения и конструктора имени в Storage.
        /// Другие настройки сохраняются в стандартных параметрах печати.
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <param name="nameConstructor"></param>
        public void SaveNewSettings(YayPrintSettings printSettings) //string outputFolder, string nameConstructor)
        {
            using (Transaction t = new Transaction(_doc))
            {
                t.Start("Сохранение настроек печати");

                Schema sch = Schema.Lookup(new Guid(schemaGuid));
                Entity ent = _storageElem.GetEntity(sch);


                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.OutputFolder)), printSettings.outputFolder);
                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.NameConstructor)), printSettings.nameConstructor);
                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.PrinterName)), printSettings.printerName);

                int colorTypeInt = (int)printSettings.colorsType;
                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.ColorsType)), colorTypeInt.ToString());

                string joinPdfValue = printSettings.mergePdfs.ToString();
                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.JoinPdf)), joinPdfValue);
                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.PrintToPaper)), printSettings.printToPaper.ToString());

                int rasterQualityValue = (int)printSettings.rasterQuality;
                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.RasterQuality)), rasterQualityValue.ToString());

                int hiddenLinesValue = (int)printSettings.hiddenLineProcessing;
                ent.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.HiddenLineViewsType)), hiddenLinesValue.ToString());
                

                //foreach (string fieldName in Enum.GetValues(typeof(PrintSettingFields)))
                //{
                //    Field f = sch.GetField(fieldName);
                //    ent.Set<string>(f, value);
                //}
                //Field f1 = sch.GetField(fieldFolder);
                //Field f2 = sch.GetField(fieldConstructor);



                //ent.Set<string>(f1, outputFolder);
                //ent.Set<string>(f2, nameConstructor);

                _storageElem.SetEntity(ent);

                t.Commit();
            }
        }

        /// <summary>
        /// Проверяю наличие в документа хранилища настроек, если нет - создаю
        /// </summary>
        private void StorageCheckIn()
        {
            Entity ent = null;
            Schema sch = null;
            try
            {
                sch = Schema.Lookup(new Guid(schemaGuid));
                ent = _storageElem.GetEntity(sch);
                if (ent.Schema != null) return;
            }
            catch { }



            SchemaBuilder sb = new SchemaBuilder(new Guid(schemaGuid));
            sb.SetReadAccessLevel(AccessLevel.Public);
            sb.SetWriteAccessLevel(AccessLevel.Public);

            foreach (string fieldName in Enum.GetValues(typeof(PrintSettingFields)))
            {
                FieldBuilder fb = sb.AddSimpleField(fieldName, typeof(string));
            }
            //FieldBuilder fb1 = sb.AddSimpleField(fieldFolder, typeof(string));
            //FieldBuilder fb2 = sb.AddSimpleField(fieldConstructor, typeof(string));

            sb.SetSchemaName("YayBatchPrintSettings");

            sch = sb.Finish();

            using (Transaction t = new Transaction(_doc))
            {
                t.Start("Создание настроек печати");

                Entity ent2 = new Entity(sch);

                //инициализирую стартовыми значениями для первого запуска
                ent2.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.OutputFolder)), @"C:\PDF Print");
                ent2.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.NameConstructor)), "<Номер листа>_<Имя листа>.pdf");
                ent2.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.ColorsType)), "1");
                ent2.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.JoinPdf)), "false");
                ent2.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.PrintToPaper)), "false");
                ent2.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.RasterQuality)), "3");
                ent2.Set<string>(sch.GetField(PrintSettingField.GetFieldName(PrintSettingFields.HiddenLineViewsType)), "0");

                _storageElem.SetEntity(ent2);

                t.Commit();
            }
        }

        public string readParamFromStorage(PrintSettingFields field)
        {
            string paramName = Enum.GetName(typeof(PrintSettingFields), field);
            string val = this.readParamFromStorage(paramName);
            return val;
        }


        /// <summary>
        /// Считываю значение параметра из Storage
        /// </summary>
        /// <param name="paramName">Имя параметра</param>
        /// <returns>Значение параметра</returns>
        public string readParamFromStorage(string paramName)
        {
            Schema sch = Schema.Lookup(new Guid(schemaGuid));
            Entity ent = _storageElem.GetEntity(sch);

            string val = ent.Get<string>(sch.GetField(paramName));
            return val;
        }

        /// <summary>
        /// Получение элемента модели, содержащего ExtensibleStorage
        /// </summary>
        /// <returns></returns>
        private Element getStorageElement()
        {
            Autodesk.Revit.DB.Structure.StructuralSettings ss =
                new FilteredElementCollector(_doc)
                .OfClass(typeof(Autodesk.Revit.DB.Structure.StructuralSettings))
                .First() as Autodesk.Revit.DB.Structure.StructuralSettings;
            return ss;
        }

    }
}
