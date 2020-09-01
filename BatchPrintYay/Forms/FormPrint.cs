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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BatchPrintYay
{
    public partial class FormPrint : Form
    {
        private YayPrintSettings _printSettings;
        public YayPrintSettings printSettings
        {
            get { return _printSettings; }
        }

        //public List<MySheet> sheetsToPrint = new List<MySheet>();
        private Dictionary<string, List<MySheet>> sheetsBaseToPrint =
            new Dictionary<string, List<MySheet>>();

        public Dictionary<string, List<MySheet>> sheetsSelected =
            new Dictionary<string, List<MySheet>>();


        public bool printToFile = false;


        public FormPrint(Dictionary<string, List<MySheet>> SheetsBase, YayPrintSettings printSettings)
        {
            InitializeComponent();
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            sheetsBaseToPrint = SheetsBase;

            //заполняю treeView
            foreach (var docWithSheets in sheetsBaseToPrint)
            {
                TreeNode docNode = new TreeNode(docWithSheets.Key);
                bool haveChecked = false;
                foreach (MySheet sheet in docWithSheets.Value)
                {
                    string sheetTitle = sheet.ToString();
                    TreeNode sheetNode = new TreeNode(sheetTitle);
                    sheetNode.Checked = sheet.IsPrintable;
                    if (sheet.IsPrintable) haveChecked = true;
                    docNode.Nodes.Add(sheetNode);
                }
                if (haveChecked) docNode.Expand();
                treeView1.Nodes.Add(docNode);
            }

            //заполняю параметры печати
            _printSettings = printSettings;
            textBoxNameConstructor.Text = printSettings.nameConstructor;
            txtBoxOutputFolder.Text = printSettings.outputFolder;
            checkBoxMergePdfs.Checked = printSettings.mergePdfs;


            List<string> printers = new List<string>();
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
            if (printers.Count == 0) throw new Exception("Cant find any installed printers");
            comboBoxPrinters.DataSource = printers;

            if (printers.Contains(_printSettings.printerName))
            {
                comboBoxPrinters.SelectedItem = printers.Where(i => i.Equals(_printSettings.printerName)).First();
            }
            else
            {
                string selectedPrinterName = PrinterUtility.GetDefaultPrinter();
                if (!printers.Contains(selectedPrinterName)) throw new Exception("Cant find printer " + selectedPrinterName);
                comboBoxPrinters.SelectedItem = printers.Where(i => i.Equals(selectedPrinterName)).First();
            }



            radioButtonPDF.Checked = comboBoxPrinters.SelectedItem.ToString().Contains("PDF");
            radioButtonPaper.Checked = !radioButtonPDF.Checked;


            if (_printSettings.hiddenLineProcessing == Autodesk.Revit.DB.HiddenLineViewsType.VectorProcessing)                
            {
                radioButtonVector.Checked = true;
                radioButtonRastr.Checked = false;
            }
            else
            {
                radioButtonVector.Checked = false;
                radioButtonRastr.Checked = true;
            }

            List<Autodesk.Revit.DB.RasterQualityType> rasterTypes =
                Enum.GetValues(typeof(Autodesk.Revit.DB.RasterQualityType))
                .Cast<Autodesk.Revit.DB.RasterQualityType>()
                .ToList();
            comboBoxRasterQuality.DataSource = rasterTypes;
            try
            {
                comboBoxRasterQuality.SelectedItem = _printSettings.rasterQuality; 
                //rasterQualityTypes.Where(i => Enum.GetName(typeof(Autodesk.Revit.DB.RasterQualityType), i).Equals()).First();
            }
            catch { }

            List<ColorType> colorTypes = Enum.GetValues(typeof(ColorType))
                .Cast<ColorType>()
                .ToList();
            comboBoxColors.DataSource = colorTypes;
            comboBoxColors.SelectedItem = _printSettings.colorsType;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

            foreach (TreeNode docNode in treeView1.Nodes)
            {
                string docNodeTitle = docNode.Text;
                //string revitDocTitle = sheetsBaseToPrint.Keys.Where(d => d == docNodeTitle).First();
                List<MySheet> selectedSheetsInDoc = new List<MySheet>();
                foreach (TreeNode sheetNode in docNode.Nodes)
                {
                    if (!sheetNode.Checked) continue;
                    string sheetTitle = sheetNode.Text;

                    var tempSheets = sheetsBaseToPrint[docNodeTitle].Where(s => sheetTitle == s.ToString()).ToList();
                    if (tempSheets.Count == 0) throw new Exception("Cant get sheets from TreeNode");
                    MySheet msheet = tempSheets.First();
                    selectedSheetsInDoc.Add(msheet);
                }
                if (selectedSheetsInDoc.Count == 0) continue;

                sheetsSelected.Add(docNodeTitle, selectedSheetsInDoc);
            }


            if (radioButtonVector.Checked)
                _printSettings.hiddenLineProcessing = Autodesk.Revit.DB.HiddenLineViewsType.VectorProcessing;
            //Enum.GetName(typeof(Autodesk.Revit.DB.HiddenLineViewsType), Autodesk.Revit.DB.HiddenLineViewsType.VectorProcessing);
            else
                _printSettings.hiddenLineProcessing = Autodesk.Revit.DB.HiddenLineViewsType.RasterProcessing;
            //Enum.GetName(typeof(Autodesk.Revit.DB.HiddenLineViewsType), Autodesk.Revit.DB.HiddenLineViewsType.RasterProcessing);

            _printSettings.rasterQuality = (Autodesk.Revit.DB.RasterQualityType)comboBoxRasterQuality.SelectedValue;
                //Enum.GetName(typeof(Autodesk.Revit.DB.RasterQualityType), comboBoxRasterQuality.SelectedValue);
            _printSettings.outputFolder = txtBoxOutputFolder.Text;
            _printSettings.printerName = comboBoxPrinters.SelectedItem.ToString();


            bool checkConstructor = false;
            string tempConstr = textBoxNameConstructor.Text;
            if (tempConstr.Split('<').Length > 1)
            {
                if (tempConstr.Split('<')[1].Contains(">"))
                    checkConstructor = true;
            }
            if (checkConstructor)
            {
                _printSettings.nameConstructor = textBoxNameConstructor.Text;
            }
            else
            {
                _printSettings.nameConstructor = "<Номер листа>_<Имя листа>.pdf";
            }
            _printSettings.mergePdfs = checkBoxMergePdfs.Checked;
            _printSettings.printToPaper = radioButtonPaper.Checked;
            this.printToFile = radioButtonPDF.Checked;

            //_printSettings.colorStamp = checkBoxColorStamp.Checked;
            //_printSettings.excludeColors = textBoxExcludeColors.Text;

            _printSettings.colorsType = (ColorType)comboBoxColors.SelectedItem;

            _printSettings.useOrientation = checkBoxOrientation.Checked;

            _printSettings.refreshSchedules = checkBoxRefresh.Checked;

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbDialog = new FolderBrowserDialog();
            fbDialog.ShowNewFolderButton = true;
            if (fbDialog.ShowDialog() == DialogResult.OK)
            {
                string path = fbDialog.SelectedPath;
                this.txtBoxOutputFolder.Text = path;
            }
        }

        private void btnOpenNameConstructor_Click(object sender, EventArgs e)
        {
            formNameConstructor formName = new BatchPrintYay.formNameConstructor(textBoxNameConstructor.Text);

            if (formName.ShowDialog(this) == DialogResult.OK)
            {
                textBoxNameConstructor.Text = formName.nameConstructor;
            }
            formName.Dispose();
        }

        private void buttonFormatsSetup_Click(object sender, EventArgs e)
        {

        }

        private void radioButtonPDF_CheckedChanged(object sender, EventArgs e)
        {
            txtBoxOutputFolder.Enabled = true;
            buttonBrowse.Enabled = true;
            textBoxNameConstructor.Enabled = true;
            btnOpenNameConstructor.Enabled = true;
            label5.Enabled = true;
            label6.Enabled = true;
            checkBoxMergePdfs.Enabled = true;
            checkBoxOrientation.Enabled = true;

            List<ColorType> colorTypes = Enum.GetValues(typeof(ColorType))
                .Cast<ColorType>()
                .ToList();
            comboBoxColors.DataSource = colorTypes;
            comboBoxColors.SelectedItem = _printSettings.colorsType;
        }

        private void radioButtonPaper_CheckedChanged(object sender, EventArgs e)
        {
            txtBoxOutputFolder.Enabled = false;
            buttonBrowse.Enabled = false;
            textBoxNameConstructor.Enabled = false;
            btnOpenNameConstructor.Enabled = false;
            label5.Enabled = false;
            label6.Enabled = false;
            checkBoxMergePdfs.Enabled = false;
            checkBoxOrientation.Enabled = false;

            List<ColorType> colorTypes = new List<ColorType> { ColorType.Color, ColorType.GrayScale, ColorType.Monochrome };
            comboBoxColors.DataSource = colorTypes;
            comboBoxColors.SelectedItem = ColorType.Monochrome;
        }

        private void checkBoxMergePdfs_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxMergePdfs.Checked == true)
            {
                textBoxNameConstructor.Enabled = false;
                btnOpenNameConstructor.Enabled = false;
                label6.Enabled = false;
            }
            else
            {
                textBoxNameConstructor.Enabled = true;
                btnOpenNameConstructor.Enabled = true;
                label6.Enabled = true;
            }
        }

        private void comboBoxColors_SelectedIndexChanged(object sender, EventArgs e)
        {
            ColorType curColorType = (ColorType)comboBoxColors.SelectedItem;
            if (curColorType == ColorType.MonochromeWithExcludes)
            {
                buttonExcludesColor.Enabled = true;
            }
            else
            {
                buttonExcludesColor.Enabled = false;
            }
        }

        private void comboBoxPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {
            radioButtonPDF.Checked = comboBoxPrinters.SelectedItem.ToString().Contains("PDF");
            radioButtonPaper.Checked = !radioButtonPDF.Checked;
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://bim-starter.com/plugins/batchprint");
        }

        private void radioButtonRastr_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxRasterQuality.Enabled = true;
        }

        private void radioButtonVector_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxRasterQuality.Enabled = false;
        }

        private void buttonExcludesColor_Click(object sender, EventArgs e)
        {
            FormExcludeColors form = new FormExcludeColors(_printSettings.excludeColors);
            if (form.ShowDialog() != DialogResult.OK) return;

            printSettings.excludeColors = form.Colors;
        }
    }
}
