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

        private static string schemaGuid = "414447EA-4228-4B87-A97C-612462722AD2";
        private static string fieldFolder = "OutputFolder";
        private static string fieldConstructor = "NameConstructor";



        public string OutputFolder
        {
            get
            {
                string val = this.readParamFromStorage(fieldFolder);
                return val;
            }
        }
        public string NameConstructor
        {
            get
            {
                string val = this.readParamFromStorage(fieldConstructor);
                return val;
            }
        }



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
        public void SaveNewSettings(string outputFolder, string nameConstructor)
        {
            using (Transaction t = new Transaction(_doc))
            {
                t.Start("Сохранение настроек печати");

                Schema sch = Schema.Lookup(new Guid(schemaGuid));

                Field f1 = sch.GetField(fieldFolder);
                Field f2 = sch.GetField(fieldConstructor);

                Entity ent = _storageElem.GetEntity(sch);

                ent.Set<string>(f1, outputFolder);
                ent.Set<string>(f2, nameConstructor);

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

            FieldBuilder fb1 = sb.AddSimpleField(fieldFolder, typeof(string));
            FieldBuilder fb2 = sb.AddSimpleField(fieldConstructor, typeof(string));

            sb.SetSchemaName("YayBatchPrintSettings");

            sch = sb.Finish();

            using (Transaction t = new Transaction(_doc))
            {
                t.Start("Создание настроек печати");


                Field f1 = sch.GetField(fieldFolder);
                Field f2 = sch.GetField(fieldConstructor);

                Entity ent2 = new Entity(sch);
                ent2.Set<string>(f1, @"C:\PDF Print");
                ent2.Set<string>(f2, "<Номер листа>_<Имя листа>.pdf");

                _storageElem.SetEntity(ent2);

                t.Commit();
            }
        }

        /// <summary>
        /// Считываю значение параметра из Storage
        /// </summary>
        /// <param name="paramName">Имя параметра</param>
        /// <returns>Значение параметра</returns>
        private string readParamFromStorage(string paramName)
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
