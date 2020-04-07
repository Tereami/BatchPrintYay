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
    public partial class FormCreateCustomFormat : Form
    {
        string msgSheetName;
        string msgSheetSize;


        public FormCreateCustomFormat(string SheetTitle, string PaperSize)
        {
            InitializeComponent();
            msgSheetName = SheetTitle;
            msgSheetSize = PaperSize;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormCreateCustomFormat_Load(object sender, EventArgs e)
        {
            labelMsg.Text = "На листе '" + msgSheetName + "' применен лист с размерами " + msgSheetSize + ".";
        }
    }
}
