namespace CoalcavaDropzone
{
    partial class Form1
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
            this.btnSort = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOpenDoc = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnQuickGrab = new System.Windows.Forms.Button();
            this.lblFileName = new System.Windows.Forms.Label();
            this.pnlDropzone = new System.Windows.Forms.Panel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.pbDownloadIcon = new System.Windows.Forms.PictureBox();
            this.lblDropPrompt = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.txtClientSearch = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbDepartment = new System.Windows.Forms.ComboBox();
            this.cmbDocType = new System.Windows.Forms.ComboBox();
            this.dtpDocDate = new System.Windows.Forms.DateTimePicker();
            this.lblPreviewName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageDropzone = new System.Windows.Forms.TabPage();
            this.chkIncludeDay = new System.Windows.Forms.CheckBox();
            this.tabPageOptions = new System.Windows.Forms.TabPage();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.btnBrowseRoot = new System.Windows.Forms.Button();
            this.txtRootPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lstClientSuggestions = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.pnlDropzone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDownloadIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            this.tabControlMain.SuspendLayout();
            this.tabPageDropzone.SuspendLayout();
            this.tabPageOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSort
            // 
            this.btnSort.Location = new System.Drawing.Point(387, 451);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(144, 62);
            this.btnSort.TabIndex = 1;
            this.btnSort.Text = "Guardar Documento";
            this.btnSort.UseVisualStyleBackColor = true;
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(245)))), ((int)(((byte)(242)))));
            this.panel1.Controls.Add(this.btnOpenDoc);
            this.panel1.Controls.Add(this.btnClear);
            this.panel1.Controls.Add(this.btnQuickGrab);
            this.panel1.Controls.Add(this.lblFileName);
            this.panel1.Controls.Add(this.pnlDropzone);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(357, 518);
            this.panel1.TabIndex = 2;
            // 
            // btnOpenDoc
            // 
            this.btnOpenDoc.Location = new System.Drawing.Point(211, 350);
            this.btnOpenDoc.Name = "btnOpenDoc";
            this.btnOpenDoc.Size = new System.Drawing.Size(126, 27);
            this.btnOpenDoc.TabIndex = 5;
            this.btnOpenDoc.Text = "Abrir Documento";
            this.btnOpenDoc.UseVisualStyleBackColor = true;
            this.btnOpenDoc.Click += new System.EventHandler(this.btnOpenDoc_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(108, 350);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 52);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Nuevo";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnQuickGrab
            // 
            this.btnQuickGrab.Location = new System.Drawing.Point(15, 350);
            this.btnQuickGrab.Name = "btnQuickGrab";
            this.btnQuickGrab.Size = new System.Drawing.Size(87, 52);
            this.btnQuickGrab.TabIndex = 3;
            this.btnQuickGrab.Text = "Reciente";
            this.btnQuickGrab.UseVisualStyleBackColor = true;
            this.btnQuickGrab.Click += new System.EventHandler(this.btnQuickGrab_Click_1);
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(14, 418);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(202, 17);
            this.lblFileName.TabIndex = 2;
            this.lblFileName.Text = "No hay archivos seleccionados.";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlDropzone
            // 
            this.pnlDropzone.AllowDrop = true;
            this.pnlDropzone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.pnlDropzone.Controls.Add(this.picPreview);
            this.pnlDropzone.Controls.Add(this.pbDownloadIcon);
            this.pnlDropzone.Controls.Add(this.lblDropPrompt);
            this.pnlDropzone.Location = new System.Drawing.Point(15, 68);
            this.pnlDropzone.Name = "pnlDropzone";
            this.pnlDropzone.Size = new System.Drawing.Size(322, 275);
            this.pnlDropzone.TabIndex = 1;
            // 
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(322, 275);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 2;
            this.picPreview.TabStop = false;
            // 
            // pbDownloadIcon
            // 
            this.pbDownloadIcon.Image = global::CoalcavaDropzone.Properties.Resources._1410633_200;
            this.pbDownloadIcon.Location = new System.Drawing.Point(76, 33);
            this.pbDownloadIcon.Name = "pbDownloadIcon";
            this.pbDownloadIcon.Size = new System.Drawing.Size(167, 192);
            this.pbDownloadIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbDownloadIcon.TabIndex = 1;
            this.pbDownloadIcon.TabStop = false;
            // 
            // lblDropPrompt
            // 
            this.lblDropPrompt.AutoSize = true;
            this.lblDropPrompt.Font = new System.Drawing.Font("Lucida Sans Unicode", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropPrompt.Location = new System.Drawing.Point(16, 246);
            this.lblDropPrompt.Name = "lblDropPrompt";
            this.lblDropPrompt.Size = new System.Drawing.Size(303, 16);
            this.lblDropPrompt.TabIndex = 0;
            this.lblDropPrompt.Text = "Arrastra un archivo aquí o haz clic para buscar.\r\n";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Lucida Sans Unicode", 13.2F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "OneDrive Dropzone";
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // txtClientSearch
            // 
            this.txtClientSearch.Font = new System.Drawing.Font("Malgun Gothic", 9.8F, System.Drawing.FontStyle.Bold);
            this.txtClientSearch.Location = new System.Drawing.Point(387, 25);
            this.txtClientSearch.Name = "txtClientSearch";
            this.txtClientSearch.Size = new System.Drawing.Size(284, 29);
            this.txtClientSearch.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label2.Location = new System.Drawing.Point(384, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Cliente";
            // 
            // cmbDepartment
            // 
            this.cmbDepartment.Font = new System.Drawing.Font("Malgun Gothic", 9.8F, System.Drawing.FontStyle.Bold);
            this.cmbDepartment.FormattingEnabled = true;
            this.cmbDepartment.Location = new System.Drawing.Point(387, 172);
            this.cmbDepartment.Name = "cmbDepartment";
            this.cmbDepartment.Size = new System.Drawing.Size(284, 29);
            this.cmbDepartment.TabIndex = 5;
            this.cmbDepartment.SelectedIndexChanged += new System.EventHandler(this.cmbDepartment_SelectedIndexChanged);
            // 
            // cmbDocType
            // 
            this.cmbDocType.Font = new System.Drawing.Font("Malgun Gothic", 9.8F, System.Drawing.FontStyle.Bold);
            this.cmbDocType.FormattingEnabled = true;
            this.cmbDocType.Location = new System.Drawing.Point(387, 236);
            this.cmbDocType.Name = "cmbDocType";
            this.cmbDocType.Size = new System.Drawing.Size(284, 29);
            this.cmbDocType.TabIndex = 6;
            // 
            // dtpDocDate
            // 
            this.dtpDocDate.Font = new System.Drawing.Font("Malgun Gothic", 9.8F, System.Drawing.FontStyle.Bold);
            this.dtpDocDate.Location = new System.Drawing.Point(387, 296);
            this.dtpDocDate.Name = "dtpDocDate";
            this.dtpDocDate.Size = new System.Drawing.Size(284, 29);
            this.dtpDocDate.TabIndex = 7;
            // 
            // lblPreviewName
            // 
            this.lblPreviewName.AutoSize = true;
            this.lblPreviewName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblPreviewName.Location = new System.Drawing.Point(384, 396);
            this.lblPreviewName.Name = "lblPreviewName";
            this.lblPreviewName.Size = new System.Drawing.Size(23, 17);
            this.lblPreviewName.TabIndex = 8;
            this.lblPreviewName.Text = "---";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(384, 216);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Tipo Documento";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label4.Location = new System.Drawing.Point(384, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Departamento";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label5.Location = new System.Drawing.Point(384, 276);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "Fecha";
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageDropzone);
            this.tabControlMain.Controls.Add(this.tabPageOptions);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControlMain.ItemSize = new System.Drawing.Size(150, 20);
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(763, 552);
            this.tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlMain.TabIndex = 6;
            // 
            // tabPageDropzone
            // 
            this.tabPageDropzone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
            this.tabPageDropzone.Controls.Add(this.lstClientSuggestions);
            this.tabPageDropzone.Controls.Add(this.chkIncludeDay);
            this.tabPageDropzone.Controls.Add(this.panel1);
            this.tabPageDropzone.Controls.Add(this.label5);
            this.tabPageDropzone.Controls.Add(this.txtClientSearch);
            this.tabPageDropzone.Controls.Add(this.label4);
            this.tabPageDropzone.Controls.Add(this.btnSort);
            this.tabPageDropzone.Controls.Add(this.label3);
            this.tabPageDropzone.Controls.Add(this.label2);
            this.tabPageDropzone.Controls.Add(this.lblPreviewName);
            this.tabPageDropzone.Controls.Add(this.cmbDepartment);
            this.tabPageDropzone.Controls.Add(this.dtpDocDate);
            this.tabPageDropzone.Controls.Add(this.cmbDocType);
            this.tabPageDropzone.Location = new System.Drawing.Point(4, 24);
            this.tabPageDropzone.Name = "tabPageDropzone";
            this.tabPageDropzone.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDropzone.Size = new System.Drawing.Size(755, 524);
            this.tabPageDropzone.TabIndex = 0;
            this.tabPageDropzone.Text = "Dropzone";
            // 
            // chkIncludeDay
            // 
            this.chkIncludeDay.AutoSize = true;
            this.chkIncludeDay.Font = new System.Drawing.Font("Malgun Gothic", 6.5F, System.Drawing.FontStyle.Bold);
            this.chkIncludeDay.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.chkIncludeDay.Location = new System.Drawing.Point(387, 331);
            this.chkIncludeDay.Name = "chkIncludeDay";
            this.chkIncludeDay.Size = new System.Drawing.Size(240, 17);
            this.chkIncludeDay.TabIndex = 12;
            this.chkIncludeDay.Text = "Incluir día en el nombre del documento";
            this.chkIncludeDay.UseVisualStyleBackColor = true;
            // 
            // tabPageOptions
            // 
            this.tabPageOptions.Controls.Add(this.btnSaveSettings);
            this.tabPageOptions.Controls.Add(this.btnBrowseRoot);
            this.tabPageOptions.Controls.Add(this.txtRootPath);
            this.tabPageOptions.Controls.Add(this.label6);
            this.tabPageOptions.Location = new System.Drawing.Point(4, 24);
            this.tabPageOptions.Name = "tabPageOptions";
            this.tabPageOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptions.Size = new System.Drawing.Size(755, 524);
            this.tabPageOptions.TabIndex = 1;
            this.tabPageOptions.Text = "Opciones";
            this.tabPageOptions.UseVisualStyleBackColor = true;
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(17, 428);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(158, 58);
            this.btnSaveSettings.TabIndex = 4;
            this.btnSaveSettings.Text = "Guardar";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click_1);
            // 
            // btnBrowseRoot
            // 
            this.btnBrowseRoot.Location = new System.Drawing.Point(344, 93);
            this.btnBrowseRoot.Name = "btnBrowseRoot";
            this.btnBrowseRoot.Size = new System.Drawing.Size(137, 25);
            this.btnBrowseRoot.TabIndex = 3;
            this.btnBrowseRoot.Text = "Buscar Ruta";
            this.btnBrowseRoot.UseVisualStyleBackColor = true;
            this.btnBrowseRoot.Click += new System.EventHandler(this.btnBrowseRoot_Click_1);
            // 
            // txtRootPath
            // 
            this.txtRootPath.Location = new System.Drawing.Point(17, 93);
            this.txtRootPath.Name = "txtRootPath";
            this.txtRootPath.ReadOnly = true;
            this.txtRootPath.Size = new System.Drawing.Size(320, 25);
            this.txtRootPath.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Lucida Sans Unicode", 13.2F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(12, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(176, 27);
            this.label6.TabIndex = 1;
            this.label6.Text = "Configuración";
            // 
            // lstClientSuggestions
            // 
            this.lstClientSuggestions.Font = new System.Drawing.Font("Malgun Gothic", 8.8F, System.Drawing.FontStyle.Bold);
            this.lstClientSuggestions.FormattingEnabled = true;
            this.lstClientSuggestions.ItemHeight = 20;
            this.lstClientSuggestions.Location = new System.Drawing.Point(387, 49);
            this.lstClientSuggestions.Name = "lstClientSuggestions";
            this.lstClientSuggestions.Size = new System.Drawing.Size(284, 224);
            this.lstClientSuggestions.TabIndex = 13;
            this.lstClientSuggestions.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
            this.ClientSize = new System.Drawing.Size(766, 552);
            this.Controls.Add(this.tabControlMain);
            this.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Form1";
            this.Text = "COALCAVA Dropzone";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlDropzone.ResumeLayout(false);
            this.pnlDropzone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDownloadIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageDropzone.ResumeLayout(false);
            this.tabPageDropzone.PerformLayout();
            this.tabPageOptions.ResumeLayout(false);
            this.tabPageOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSort;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlDropzone;
        private System.Windows.Forms.Label lblDropPrompt;
        private System.Windows.Forms.Label label1;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.PictureBox pbDownloadIcon;
        private System.Windows.Forms.Button btnQuickGrab;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnOpenDoc;
        private System.Windows.Forms.Label lblPreviewName;
        private System.Windows.Forms.DateTimePicker dtpDocDate;
        private System.Windows.Forms.ComboBox cmbDocType;
        private System.Windows.Forms.ComboBox cmbDepartment;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtClientSearch;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageDropzone;
        private System.Windows.Forms.TabPage tabPageOptions;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.Button btnBrowseRoot;
        private System.Windows.Forms.TextBox txtRootPath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkIncludeDay;
        private System.Windows.Forms.ListBox lstClientSuggestions;
    }
}

