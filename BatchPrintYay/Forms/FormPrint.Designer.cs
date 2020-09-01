namespace BatchPrintYay
{
    partial class FormPrint
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxRefresh = new System.Windows.Forms.CheckBox();
            this.checkBoxOrientation = new System.Windows.Forms.CheckBox();
            this.checkBoxMergePdfs = new System.Windows.Forms.CheckBox();
            this.radioButtonPDF = new System.Windows.Forms.RadioButton();
            this.radioButtonPaper = new System.Windows.Forms.RadioButton();
            this.buttonFormatsSetup = new System.Windows.Forms.Button();
            this.btnOpenNameConstructor = new System.Windows.Forms.Button();
            this.textBoxNameConstructor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.txtBoxOutputFolder = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxPrinters = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonRastr = new System.Windows.Forms.RadioButton();
            this.radioButtonVector = new System.Windows.Forms.RadioButton();
            this.comboBoxRasterQuality = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonExcludesColor = new System.Windows.Forms.Button();
            this.comboBoxColors = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxRefresh);
            this.groupBox1.Controls.Add(this.checkBoxOrientation);
            this.groupBox1.Controls.Add(this.checkBoxMergePdfs);
            this.groupBox1.Controls.Add(this.radioButtonPDF);
            this.groupBox1.Controls.Add(this.radioButtonPaper);
            this.groupBox1.Controls.Add(this.buttonFormatsSetup);
            this.groupBox1.Controls.Add(this.btnOpenNameConstructor);
            this.groupBox1.Controls.Add(this.textBoxNameConstructor);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.txtBoxOutputFolder);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboBoxPrinters);
            this.groupBox1.Location = new System.Drawing.Point(250, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(277, 257);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Принтер";
            // 
            // checkBoxRefresh
            // 
            this.checkBoxRefresh.AutoSize = true;
            this.checkBoxRefresh.Checked = true;
            this.checkBoxRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRefresh.Location = new System.Drawing.Point(9, 232);
            this.checkBoxRefresh.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxRefresh.Name = "checkBoxRefresh";
            this.checkBoxRefresh.Size = new System.Drawing.Size(152, 17);
            this.checkBoxRefresh.TabIndex = 13;
            this.checkBoxRefresh.Text = "Обновить спецификации";
            this.checkBoxRefresh.UseVisualStyleBackColor = true;
            this.checkBoxRefresh.CheckedChanged += new System.EventHandler(this.checkBoxMergePdfs_CheckedChanged);
            // 
            // checkBoxOrientation
            // 
            this.checkBoxOrientation.AutoSize = true;
            this.checkBoxOrientation.Location = new System.Drawing.Point(9, 211);
            this.checkBoxOrientation.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxOrientation.Name = "checkBoxOrientation";
            this.checkBoxOrientation.Size = new System.Drawing.Size(219, 17);
            this.checkBoxOrientation.TabIndex = 13;
            this.checkBoxOrientation.Text = "Улучшенное определение ориентации";
            this.checkBoxOrientation.UseVisualStyleBackColor = true;
            this.checkBoxOrientation.CheckedChanged += new System.EventHandler(this.checkBoxMergePdfs_CheckedChanged);
            // 
            // checkBoxMergePdfs
            // 
            this.checkBoxMergePdfs.AutoSize = true;
            this.checkBoxMergePdfs.Location = new System.Drawing.Point(9, 190);
            this.checkBoxMergePdfs.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxMergePdfs.Name = "checkBoxMergePdfs";
            this.checkBoxMergePdfs.Size = new System.Drawing.Size(153, 17);
            this.checkBoxMergePdfs.TabIndex = 13;
            this.checkBoxMergePdfs.Text = "Объединить в один файл";
            this.checkBoxMergePdfs.UseVisualStyleBackColor = true;
            this.checkBoxMergePdfs.CheckedChanged += new System.EventHandler(this.checkBoxMergePdfs_CheckedChanged);
            // 
            // radioButtonPDF
            // 
            this.radioButtonPDF.AutoSize = true;
            this.radioButtonPDF.Checked = true;
            this.radioButtonPDF.Location = new System.Drawing.Point(122, 70);
            this.radioButtonPDF.Name = "radioButtonPDF";
            this.radioButtonPDF.Size = new System.Drawing.Size(56, 17);
            this.radioButtonPDF.TabIndex = 12;
            this.radioButtonPDF.TabStop = true;
            this.radioButtonPDF.Text = "В PDF";
            this.radioButtonPDF.UseVisualStyleBackColor = true;
            this.radioButtonPDF.CheckedChanged += new System.EventHandler(this.radioButtonPDF_CheckedChanged);
            // 
            // radioButtonPaper
            // 
            this.radioButtonPaper.AutoSize = true;
            this.radioButtonPaper.Location = new System.Drawing.Point(9, 70);
            this.radioButtonPaper.Name = "radioButtonPaper";
            this.radioButtonPaper.Size = new System.Drawing.Size(77, 17);
            this.radioButtonPaper.TabIndex = 11;
            this.radioButtonPaper.Text = "На бумагу";
            this.radioButtonPaper.UseVisualStyleBackColor = true;
            this.radioButtonPaper.CheckedChanged += new System.EventHandler(this.radioButtonPaper_CheckedChanged);
            // 
            // buttonFormatsSetup
            // 
            this.buttonFormatsSetup.Enabled = false;
            this.buttonFormatsSetup.Location = new System.Drawing.Point(238, 41);
            this.buttonFormatsSetup.Name = "buttonFormatsSetup";
            this.buttonFormatsSetup.Size = new System.Drawing.Size(27, 20);
            this.buttonFormatsSetup.TabIndex = 10;
            this.buttonFormatsSetup.Text = "...";
            this.buttonFormatsSetup.UseVisualStyleBackColor = true;
            this.buttonFormatsSetup.Click += new System.EventHandler(this.buttonFormatsSetup_Click);
            // 
            // btnOpenNameConstructor
            // 
            this.btnOpenNameConstructor.Enabled = false;
            this.btnOpenNameConstructor.Location = new System.Drawing.Point(238, 160);
            this.btnOpenNameConstructor.Name = "btnOpenNameConstructor";
            this.btnOpenNameConstructor.Size = new System.Drawing.Size(27, 21);
            this.btnOpenNameConstructor.TabIndex = 9;
            this.btnOpenNameConstructor.Text = "...";
            this.btnOpenNameConstructor.UseVisualStyleBackColor = true;
            this.btnOpenNameConstructor.Click += new System.EventHandler(this.btnOpenNameConstructor_Click);
            // 
            // textBoxNameConstructor
            // 
            this.textBoxNameConstructor.Enabled = false;
            this.textBoxNameConstructor.Location = new System.Drawing.Point(9, 161);
            this.textBoxNameConstructor.Name = "textBoxNameConstructor";
            this.textBoxNameConstructor.Size = new System.Drawing.Size(223, 20);
            this.textBoxNameConstructor.TabIndex = 8;
            this.textBoxNameConstructor.Text = "<Номер листа>_<Имя листа>.pdf";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Enabled = false;
            this.label6.Location = new System.Drawing.Point(6, 144);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(168, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Конструктор имени файла PDF:";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Enabled = false;
            this.buttonBrowse.Location = new System.Drawing.Point(238, 113);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(27, 20);
            this.buttonBrowse.TabIndex = 6;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // txtBoxOutputFolder
            // 
            this.txtBoxOutputFolder.Location = new System.Drawing.Point(9, 113);
            this.txtBoxOutputFolder.Name = "txtBoxOutputFolder";
            this.txtBoxOutputFolder.Size = new System.Drawing.Size(223, 20);
            this.txtBoxOutputFolder.TabIndex = 5;
            this.txtBoxOutputFolder.Text = "C:\\PDF_Print";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Enabled = false;
            this.label5.Location = new System.Drawing.Point(6, 96);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Путь для сохранения:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(177, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Имя (рекомендуется PDFCreator):";
            // 
            // comboBoxPrinters
            // 
            this.comboBoxPrinters.FormattingEnabled = true;
            this.comboBoxPrinters.Location = new System.Drawing.Point(9, 40);
            this.comboBoxPrinters.Margin = new System.Windows.Forms.Padding(8, 3, 8, 3);
            this.comboBoxPrinters.Name = "comboBoxPrinters";
            this.comboBoxPrinters.Size = new System.Drawing.Size(223, 21);
            this.comboBoxPrinters.TabIndex = 2;
            this.comboBoxPrinters.Text = "PDFCreator";
            this.comboBoxPrinters.SelectedIndexChanged += new System.EventHandler(this.comboBoxPrinters_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.radioButtonRastr);
            this.groupBox2.Controls.Add(this.radioButtonVector);
            this.groupBox2.Controls.Add(this.comboBoxRasterQuality);
            this.groupBox2.Location = new System.Drawing.Point(250, 275);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(278, 50);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Обработка";
            // 
            // radioButtonRastr
            // 
            this.radioButtonRastr.AutoSize = true;
            this.radioButtonRastr.Location = new System.Drawing.Point(91, 19);
            this.radioButtonRastr.Name = "radioButtonRastr";
            this.radioButtonRastr.Size = new System.Drawing.Size(79, 17);
            this.radioButtonRastr.TabIndex = 2;
            this.radioButtonRastr.Text = "Растровая";
            this.radioButtonRastr.UseVisualStyleBackColor = true;
            this.radioButtonRastr.CheckedChanged += new System.EventHandler(this.radioButtonRastr_CheckedChanged);
            // 
            // radioButtonVector
            // 
            this.radioButtonVector.AutoSize = true;
            this.radioButtonVector.Checked = true;
            this.radioButtonVector.Location = new System.Drawing.Point(9, 19);
            this.radioButtonVector.Name = "radioButtonVector";
            this.radioButtonVector.Size = new System.Drawing.Size(79, 17);
            this.radioButtonVector.TabIndex = 1;
            this.radioButtonVector.TabStop = true;
            this.radioButtonVector.Text = "Векторная";
            this.radioButtonVector.UseVisualStyleBackColor = true;
            this.radioButtonVector.CheckedChanged += new System.EventHandler(this.radioButtonVector_CheckedChanged);
            // 
            // comboBoxRasterQuality
            // 
            this.comboBoxRasterQuality.Enabled = false;
            this.comboBoxRasterQuality.FormattingEnabled = true;
            this.comboBoxRasterQuality.Items.AddRange(new object[] {
            "Низкое",
            "Среднее",
            "Высокое",
            "Презентационное"});
            this.comboBoxRasterQuality.Location = new System.Drawing.Point(177, 18);
            this.comboBoxRasterQuality.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
            this.comboBoxRasterQuality.Name = "comboBoxRasterQuality";
            this.comboBoxRasterQuality.Size = new System.Drawing.Size(88, 21);
            this.comboBoxRasterQuality.TabIndex = 1;
            this.comboBoxRasterQuality.Text = "Презентационное";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.buttonExcludesColor);
            this.groupBox3.Controls.Add(this.comboBoxColors);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(250, 331);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(278, 72);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Вывод на печать";
            // 
            // buttonExcludesColor
            // 
            this.buttonExcludesColor.Location = new System.Drawing.Point(80, 41);
            this.buttonExcludesColor.Name = "buttonExcludesColor";
            this.buttonExcludesColor.Size = new System.Drawing.Size(192, 23);
            this.buttonExcludesColor.TabIndex = 3;
            this.buttonExcludesColor.Text = "Исключения цветов";
            this.buttonExcludesColor.UseVisualStyleBackColor = true;
            this.buttonExcludesColor.Click += new System.EventHandler(this.buttonExcludesColor_Click);
            // 
            // comboBoxColors
            // 
            this.comboBoxColors.FormattingEnabled = true;
            this.comboBoxColors.Items.AddRange(new object[] {
            "Черные линии",
            "Оттенки серого",
            "Цвет"});
            this.comboBoxColors.Location = new System.Drawing.Point(80, 15);
            this.comboBoxColors.Name = "comboBoxColors";
            this.comboBoxColors.Size = new System.Drawing.Size(192, 21);
            this.comboBoxColors.TabIndex = 2;
            this.comboBoxColors.Text = "Оттенки серого";
            this.comboBoxColors.SelectedIndexChanged += new System.EventHandler(this.comboBoxColors_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Цвета:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(290, 416);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "ОК";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(371, 416);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Листы для печати:";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 426);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "v2020.09.01";
            // 
            // treeView1
            // 
            this.treeView1.CheckBoxes = true;
            this.treeView1.FullRowSelect = true;
            this.treeView1.Location = new System.Drawing.Point(12, 28);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(232, 375);
            this.treeView1.TabIndex = 9;
            // 
            // buttonHelp
            // 
            this.buttonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHelp.Location = new System.Drawing.Point(452, 416);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(75, 23);
            this.buttonHelp.TabIndex = 5;
            this.buttonHelp.Text = "Справка";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // FormPrint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 447);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormPrint";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Пакетная печать";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxPrinters;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonRastr;
        private System.Windows.Forms.RadioButton radioButtonVector;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxColors;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxRasterQuality;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox txtBoxOutputFolder;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOpenNameConstructor;
        private System.Windows.Forms.TextBox textBoxNameConstructor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonFormatsSetup;
        private System.Windows.Forms.RadioButton radioButtonPDF;
        private System.Windows.Forms.RadioButton radioButtonPaper;
        private System.Windows.Forms.CheckBox checkBoxMergePdfs;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.CheckBox checkBoxOrientation;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.CheckBox checkBoxRefresh;
        private System.Windows.Forms.Button buttonExcludesColor;
    }
}