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
    public partial class FormExcludeColors : Form
    {
        public List<Color> Colors;
        public FormExcludeColors(List<Color> colors)
        {
            InitializeComponent();

            Colors = colors;
            foreach (Color c in colors)
            {
                Button btn = new Button();
                btn.Size = new Size(80, 25);
                btn.Text = c.R.ToString() + " " + c.G.ToString() + " " + c.B.ToString();
                btn.BackColor = c;
                btn.Click += buttonColor_Click;
                flowLayoutPanel1.Controls.Add(btn);
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Colors = new List<Color>();
            foreach (var btn in flowLayoutPanel1.Controls)
            {
                Button btn2 = btn as Button;
                Color color = btn2.BackColor;
                Colors.Add(color);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private Random rnd = new Random();
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Button btn = new Button();
            btn.Size = new Size(80, 25);
            Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            btn.Text = randomColor.R.ToString() + " " + randomColor.G.ToString() + " " + randomColor.B.ToString();
            btn.BackColor = randomColor;
            btn.Click += buttonColor_Click;
            flowLayoutPanel1.Controls.Add(btn);
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ColorDialog dialog = new ColorDialog();
            dialog.Color = btn.BackColor;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            btn.BackColor = dialog.Color;
            btn.Text = dialog.Color.R.ToString() + " " + dialog.Color.G.ToString() + " " + dialog.Color.B.ToString();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.RemoveAt(flowLayoutPanel1.Controls.Count - 1);
        }
    }
}
