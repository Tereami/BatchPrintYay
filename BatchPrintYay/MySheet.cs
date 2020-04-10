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

namespace BatchPrintYay
{
    /// <summary>
    /// Класс-"оболочка", хранящий лист Revit, сведения формате листа и параметры печати листа
    /// </summary>
    public class MySheet : IComparable
    {
        public ViewSheet sheet;
        public PaperSize revitPaperSize;
        public System.Drawing.Printing.PaperSize windowsPaperSize;
        public IPrintSetting pSetting;
        public bool IsVertical;
        public int SheetId;
        public bool IsPrintable;

        //параметры для печати нескольких листов на одном
        public List<FamilyInstance> titleBlocks;
        public double widthMm;
        public double heigthMm;
        

        /// <summary>
        /// Инициализация класса, без объявления формата листа и параметров печати
        /// </summary>
        /// <param name="Sheet"></param>
        public MySheet(ViewSheet Sheet)
        {
            sheet = Sheet;
            SheetId = Sheet.Id.IntegerValue;
        }

        public override string ToString()
        {
            string name = sheet.SheetNumber + " - " + sheet.Name;
            return name;
        }

        /// <summary>
        /// Формирует имя листа на базе строки-"конструктора", содержащего имена параметров,
        /// которые будут заменены на значения параметров из данного листа
        /// </summary>
        /// <param name="Constructor">Строка конструктора. Имена параметров должны быть включены в треугольные скобки.</param>
        /// <returns>Сформированное имя листа</returns>
        public string NameByConstructor(string Constructor)
        {
            string name = "";

            string prefix = Constructor.Split('<').First();
            name = name + prefix;

            string[] sa = Constructor.Split('<');
            for(int i = 0; i < sa.Length; i++)
            {
                string s = sa[i];
                if (!s.Contains(">")) continue;

                string paramName = s.Split('>').First();
                string separator = s.Split('>').Last();

                string val = this.GetParameterValueBySheetOrProject(sheet, paramName);

                name = name + val;
                name = name + separator;
            }


            char[] arr = name.Where(c => (char.IsLetterOrDigit(c) ||
                             char.IsWhiteSpace(c) ||
                             c == '-' ||
                             c == '_' ||
                             c == '.' )).ToArray();

            name = new string(arr);

            return name;
        }

        /// <summary>
        /// Получает значение параметра из листа и из "информации о проекте", по аналогии с "меткой" в семействе основной надписи.
        /// </summary>
        /// <param name="sheet">Элемент модели</param>
        /// <param name="paramName">Имя параметра</param>
        /// <returns></returns>
        private string GetParameterValueBySheetOrProject(Element sheet, string paramName)
        {
            string value = "";

            Parameter param = sheet.LookupParameter(paramName);
            if(param == null)
            {
                param = sheet.Document.ProjectInformation.LookupParameter(paramName);
            }
            if(param != null)
            {
                value = this.GetParameterValueAsString(param);
            }
            return value;
        }

        /// <summary>
        /// Получает значение параметра с любым типом данных, преобразованное в тип string
        /// </summary>
        /// <param name="param">Имя параметра</param>
        /// <returns>Значение параметра как string</returns>
        private string GetParameterValueAsString(Parameter param)
        {
            string val = "";
            switch (param.StorageType)
            {
                case StorageType.None:
                    break;
                case StorageType.Integer:
                    val = param.AsInteger().ToString();
                    break;
                case StorageType.Double:
                    double d = param.AsDouble();
                    val = Math.Round(d * 304.8, 3).ToString();
                    break;
                case StorageType.String:
                    val = param.AsString();
                    break;
                case StorageType.ElementId:
                    val = param.AsElementId().IntegerValue.ToString();
                    break;
                default:
                    break;
            }
            return val;
        }


        /// <summary>
        /// Попытаться преобразовать текстовый номер листа в число для правильной сортировки
        /// </summary>
        /// <returns></returns>
        public int GetSheetNumberAsInt()
        {
            string sheetNumberString = this.sheet.SheetNumber;

            if (sheetNumberString.Contains("-"))
            {
                sheetNumberString = sheetNumberString.Split('-').Last();
            }
            if(sheetNumberString.Contains("_"))
            {
                sheetNumberString = sheetNumberString.Split('_').Last();
            }
            int sheetNumber = 0;
            try
            {
                sheetNumber = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(sheetNumberString, @"[^\d]+", ""));
            }
            catch
            {
            }
            return sheetNumber;
        }

        //для сортировки по номеру листа
        public int CompareTo(object obj)
        {
            MySheet ms = obj as MySheet;
            if(ms != null)
            {
                int thisSheetNumber = this.GetSheetNumberAsInt();
                int compareSheetNumber = ms.GetSheetNumberAsInt();
                int resuls = thisSheetNumber.CompareTo(compareSheetNumber);
                return resuls;
            }
            else
            {
                throw new Exception("Невозможно сравнить два объекта");
            }
        }
    }
}
