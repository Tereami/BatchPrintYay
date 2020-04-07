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

        private static string schemaGuid = "414447EA-4228-4B87-A97C-612462722AD4";


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
                Entity ent = this.WriteEntity(sch, printSettings);
                _storageElem.SetEntity(ent);

                t.Commit();
            }
        }


        private Entity WriteEntity(Schema sch, YayPrintSettings printSettings)
        {
            Entity ent = new Entity(sch);

            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(YayPrintSettings));
            string xml = "";

            using (System.IO.StringWriter writer = new System.IO.StringWriter())
            {
                serializer.Serialize(writer, printSettings);
                xml = writer.ToString();
            }

            ent.Set<string>(sch.GetField("xmlsettings"), xml);

            return ent;
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

            //foreach (PrintSettingFields field in Enum.GetValues(typeof(PrintSettingFields)))
            //{
            //    string fieldName = Enum.GetName(typeof(PrintSettingFields), field);
            //    FieldBuilder fb = sb.AddSimpleField(fieldName, typeof(string));
            //}
            FieldBuilder fb = sb.AddSimpleField("xmlsettings", typeof(string));

            sb.SetSchemaName("YayBatchPrintSettings");

            sch = sb.Finish();

            YayPrintSettings startsettings = new YayPrintSettings(
                "PDFCreator",
                @"C:\PDF Print",
                "<Номер листа>_<Имя листа>.pdf",
                Enum.GetName(typeof(HiddenLineViewsType), HiddenLineViewsType.VectorProcessing),
                Enum.GetName(typeof(ColorDepthType), ColorDepthType.GrayScale),
                Enum.GetName(typeof(RasterQualityType), RasterQualityType.High),
                false,
                false,
                "0 0 255,242 242 242,192 192 192");

            using (Transaction t = new Transaction(_doc))
            {
                t.Start("Создание настроек печати");

                Entity ent3 = this.WriteEntity(sch, startsettings);

                _storageElem.SetEntity(ent3);

                t.Commit();
            }
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
