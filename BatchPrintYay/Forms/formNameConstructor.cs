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
    public partial class formNameConstructor : Form
    {
        public string nameConstructor;

        public formNameConstructor(string NameConstructor)
        {
            InitializeComponent();
            this.AcceptButton = buttonOk;
            this.CancelButton = buttonCancel;
            textBoxNameConstructor.Text = NameConstructor;
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            nameConstructor = textBoxNameConstructor.Text;
            this.Close();
        }
    }
}
