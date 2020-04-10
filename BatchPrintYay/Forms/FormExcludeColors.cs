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
