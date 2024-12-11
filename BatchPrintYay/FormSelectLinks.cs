using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BatchPrintYay
{
    public partial class FormSelectLinks : System.Windows.Forms.Form
    {
        public List<RevitLinkType> selectedLinks;
        public FormSelectLinks(List<RevitLinkType> links)
        {
            InitializeComponent();

            checkedListBox1.Items.Clear();
            checkedListBox1.Items.AddRange(links.ToArray());
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
