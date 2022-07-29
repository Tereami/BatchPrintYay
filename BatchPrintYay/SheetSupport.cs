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
#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace BatchPrintYay
{
    public static class SheetSupport
    {
        /// <summary>
        /// Получает листы из всех открытых документов
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns></returns>
        public static Dictionary<string, List<MySheet>> GetAllSheets(ExternalCommandData commandData, YayPrintSettings printSets)
        {
            Dictionary<string, List<MySheet>> data = new Dictionary<string, List<MySheet>>();
            Document mainDoc = commandData.Application.ActiveUIDocument.Document;
            string mainDocTitle = GetDocTitleWithoutRvt(mainDoc.Title);

            List<RevitLinkInstance> links = new FilteredElementCollector(mainDoc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();

            List<MySheet> mainSheets = GetSheetsFromDocument(mainDoc, printSets);
            data.Add(mainDocTitle, mainSheets);

            foreach (RevitLinkInstance rli in links)
            {
                Document linkDoc = rli.GetLinkDocument();
                if (linkDoc == null) continue;
                string linkDocTitle = GetDocTitleWithoutRvt(linkDoc.Title);
                if (data.ContainsKey(linkDocTitle)) continue;

                RevitLinkType rlt = mainDoc.GetElement(rli.GetTypeId()) as RevitLinkType;
                List<MySheet> curSheets = GetSheetsFromDocument(linkDoc, printSets);
                
                data.Add(linkDocTitle, curSheets);
            }

            return data;
        }


        public static string GetDocTitleWithoutRvt(string docTitle)
        {
            string result = docTitle;
            if (docTitle.EndsWith(".rvt")) result = docTitle.Substring(0, docTitle.Length - 4);
            return result;
        }

        public static string ClearIllegalCharacters(string line)
        {
            if (string.IsNullOrEmpty(line)) return string.Empty;
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) 
                + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            string line2 = r.Replace(line, "");
            return line2;
        }

        private static List<MySheet> GetSheetsFromDocument(Document doc, YayPrintSettings printSets)
        {
            List<MySheet> sheets = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .Select(i => new MySheet(i, printSets.alwaysColorParamName))
                    .ToList();

            sheets.Sort();
            return sheets;
        }

        public static string CheckTitleblocSizeCorrects(ViewSheet sheet, FamilyInstance titleBlock)
        {
            string message = "";

            Debug.WriteLine("   Titleblock ID " + titleBlock.Id.IntegerValue.ToString());

            double widthMm = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble() * 304.8;
            widthMm = Math.Round(widthMm);
            double heigthMm = titleBlock.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble() * 304.8;
            heigthMm = Math.Round(heigthMm);


            double widthMmCheck = -1, heigthMmCheck = -1;

            Parameter checkWidthParam = titleBlock.LookupParameter("Ширина");
            if (checkWidthParam != null)
            {
                
                widthMmCheck = checkWidthParam.AsDouble() * 304.8;
                widthMmCheck = Math.Round(widthMmCheck);
                Debug.WriteLine(MyStrings.MessageParameterExists + " Ширина = " + widthMmCheck.ToString("F3"));
            }

            Parameter checkHeigthParam = titleBlock.LookupParameter("Высота");
            if (checkHeigthParam != null)
            {
                heigthMmCheck = checkHeigthParam.AsDouble() * 304.8;
                heigthMmCheck = Math.Round(heigthMmCheck);
                Debug.WriteLine(MyStrings.MessageParameterExists + " Высота = " + heigthMmCheck.ToString("F3"));
            }

            if (widthMmCheck == -1 || heigthMmCheck == -1)
            {
                Debug.WriteLine("    Titleblock is not from Weandrevit template, unable to check size");
                return string.Empty;
            }

            double epsilon = 2.0;
            bool widthEquals = DoubleEquals(widthMm, widthMmCheck, epsilon);
            bool heightEquals = DoubleEquals(heigthMm, heigthMmCheck, epsilon);

            if (!widthEquals || !heightEquals)
            {
                Debug.WriteLine(MyStrings.MessageSheetSizeProblem + epsilon.ToString("F3") + " mm");
                message += "Sheet '" + sheet.SheetNumber + " : " + sheet.Name;
                message += "'. " + MyStrings.MessageUnableToGetTitleblockSize;
                if (widthMm != widthMmCheck)
                {
                    message += "'Ширина': " + widthMmCheck.ToString("F0") + "мм, '" + 
                        MyStrings.ParameterBultinSheetWidth + "': " + widthMm.ToString("F0") + "мм.\n";
                }
                if (heigthMm != heigthMmCheck)
                {
                    message += "'Высота': " + heigthMmCheck.ToString("F0") + "мм, '" + 
                        MyStrings.ParameterBultinSheetHeight + "': " + heigthMm.ToString("F0") + "мм.\n";
                }

                message += MyStrings.MessageCheckTitleblockSize;
            }
            else
            {
                Debug.WriteLine("    Incorrect titleblock size");
            }

            return message;
        }

        private static bool DoubleEquals(double d1, double d2, double epsilon)
        {
            double c = Math.Abs(d1 - d2);
            if (c <= epsilon) return true;
            return false;
        }
    }
}
