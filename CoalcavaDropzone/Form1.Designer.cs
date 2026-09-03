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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnSort = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOpenDoc = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnQuickGrab = new System.Windows.Forms.Button();
            this.pnlDropzone = new System.Windows.Forms.Panel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.pbDownloadIcon = new System.Windows.Forms.PictureBox();
            this.lblFileName = new System.Windows.Forms.Label();
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
            this.lstClientSuggestions = new System.Windows.Forms.ListBox();
            this.chkIncludeDay = new System.Windows.Forms.CheckBox();
            this.tabPageReembolsos = new System.Windows.Forms.TabPage();
            this.lstReembolsoClientSuggestions = new System.Windows.Forms.ListBox();
            this.btnProcessReembolso = new System.Windows.Forms.Button();
            this.rbDeleteOriginals = new System.Windows.Forms.RadioButton();
            this.rbSaveOriginalsToFolder = new System.Windows.Forms.RadioButton();
            this.txtReembolsoCedula = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cmbReembolsoAseguradora = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtReembolsoMonto = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtReembolsoPoliza = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtReembolsoClientSearch = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnClearReembolso = new System.Windows.Forms.Button();
            this.btnRemoveSelectedFile = new System.Windows.Forms.Button();
            this.dgvReembolsoFiles = new System.Windows.Forms.DataGridView();
            this.Nombre = new System.Windows.Forms.DataGridViewLinkColumn();
            this.Tipo = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Tamaño = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label7 = new System.Windows.Forms.Label();
            this.pnlReembolsoDrop = new System.Windows.Forms.Panel();
            this.picDownloadIcon = new System.Windows.Forms.PictureBox();
            this.lblReembolsoDropPrompt = new System.Windows.Forms.Label();
            this.tabPageOptions = new System.Windows.Forms.TabPage();
            this.chkOpenOnStartup = new System.Windows.Forms.CheckBox();
            this.chkMuteSound = new System.Windows.Forms.CheckBox();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.btnBrowseRoot = new System.Windows.Forms.Button();
            this.txtRootPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtReembolsosNotasAdicionales = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.btnBrowseReembolsoRoot = new System.Windows.Forms.Button();
            this.txtReembolsoRootPath = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.pnlDropzone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDownloadIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            this.tabControlMain.SuspendLayout();
            this.tabPageDropzone.SuspendLayout();
            this.tabPageReembolsos.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReembolsoFiles)).BeginInit();
            this.pnlReembolsoDrop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDownloadIcon)).BeginInit();
            this.tabPageOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSort
            // 
            this.btnSort.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSort.Location = new System.Drawing.Point(339, 469);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(169, 47);
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
            this.panel1.Controls.Add(this.pnlDropzone);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(312, 518);
            this.panel1.TabIndex = 2;
            // 
            // btnOpenDoc
            // 
            this.btnOpenDoc.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenDoc.Location = new System.Drawing.Point(13, 408);
            this.btnOpenDoc.Name = "btnOpenDoc";
            this.btnOpenDoc.Size = new System.Drawing.Size(158, 39);
            this.btnOpenDoc.TabIndex = 5;
            this.btnOpenDoc.Text = "Ver Documento";
            this.btnOpenDoc.UseVisualStyleBackColor = true;
            this.btnOpenDoc.Click += new System.EventHandler(this.btnOpenDoc_Click);
            // 
            // btnClear
            // 
            this.btnClear.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(94, 350);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(76, 52);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Nuevo";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnQuickGrab
            // 
            this.btnQuickGrab.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuickGrab.Location = new System.Drawing.Point(13, 350);
            this.btnQuickGrab.Name = "btnQuickGrab";
            this.btnQuickGrab.Size = new System.Drawing.Size(76, 52);
            this.btnQuickGrab.TabIndex = 3;
            this.btnQuickGrab.Text = "Reciente";
            this.btnQuickGrab.UseVisualStyleBackColor = true;
            this.btnQuickGrab.Click += new System.EventHandler(this.btnQuickGrab_Click_1);
            // 
            // pnlDropzone
            // 
            this.pnlDropzone.AllowDrop = true;
            this.pnlDropzone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.pnlDropzone.Controls.Add(this.picPreview);
            this.pnlDropzone.Controls.Add(this.pbDownloadIcon);
            this.pnlDropzone.Controls.Add(this.lblFileName);
            this.pnlDropzone.Location = new System.Drawing.Point(13, 68);
            this.pnlDropzone.Name = "pnlDropzone";
            this.pnlDropzone.Size = new System.Drawing.Size(282, 275);
            this.pnlDropzone.TabIndex = 1;
            // 
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(282, 245);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 2;
            this.picPreview.TabStop = false;
            // 
            // pbDownloadIcon
            // 
            this.pbDownloadIcon.Image = global::CoalcavaDropzone.Properties.Resources._1410633_200;
            this.pbDownloadIcon.Location = new System.Drawing.Point(66, 33);
            this.pbDownloadIcon.Name = "pbDownloadIcon";
            this.pbDownloadIcon.Size = new System.Drawing.Size(146, 192);
            this.pbDownloadIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbDownloadIcon.TabIndex = 1;
            this.pbDownloadIcon.TabStop = false;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(22, 248);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(227, 17);
            this.lblFileName.TabIndex = 2;
            this.lblFileName.Text = "Arrastra un archivo o haz clic aquí.";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Nirmala UI", 20.2F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(14, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 46);
            this.label1.TabIndex = 0;
            this.label1.Text = "Dropzone";
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // txtClientSearch
            // 
            this.txtClientSearch.Font = new System.Drawing.Font("Malgun Gothic", 9.8F, System.Drawing.FontStyle.Bold);
            this.txtClientSearch.Location = new System.Drawing.Point(339, 25);
            this.txtClientSearch.Name = "txtClientSearch";
            this.txtClientSearch.Size = new System.Drawing.Size(249, 29);
            this.txtClientSearch.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label2.Location = new System.Drawing.Point(336, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Cliente";
            // 
            // cmbDepartment
            // 
            this.cmbDepartment.Font = new System.Drawing.Font("Malgun Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDepartment.FormattingEnabled = true;
            this.cmbDepartment.Location = new System.Drawing.Point(339, 172);
            this.cmbDepartment.Name = "cmbDepartment";
            this.cmbDepartment.Size = new System.Drawing.Size(249, 31);
            this.cmbDepartment.TabIndex = 5;
            this.cmbDepartment.SelectedIndexChanged += new System.EventHandler(this.cmbDepartment_SelectedIndexChanged);
            // 
            // cmbDocType
            // 
            this.cmbDocType.Font = new System.Drawing.Font("Malgun Gothic", 9.8F);
            this.cmbDocType.FormattingEnabled = true;
            this.cmbDocType.Location = new System.Drawing.Point(339, 236);
            this.cmbDocType.Name = "cmbDocType";
            this.cmbDocType.Size = new System.Drawing.Size(249, 29);
            this.cmbDocType.TabIndex = 6;
            // 
            // dtpDocDate
            // 
            this.dtpDocDate.Font = new System.Drawing.Font("Malgun Gothic", 8.8F);
            this.dtpDocDate.Location = new System.Drawing.Point(339, 296);
            this.dtpDocDate.Name = "dtpDocDate";
            this.dtpDocDate.Size = new System.Drawing.Size(249, 27);
            this.dtpDocDate.TabIndex = 7;
            // 
            // lblPreviewName
            // 
            this.lblPreviewName.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPreviewName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblPreviewName.Location = new System.Drawing.Point(336, 396);
            this.lblPreviewName.Name = "lblPreviewName";
            this.lblPreviewName.Size = new System.Drawing.Size(385, 65);
            this.lblPreviewName.TabIndex = 8;
            this.lblPreviewName.Text = "---------------------------------------------------------------------------------" +
    "-----------------------";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(336, 216);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Tipo Documento";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label4.Location = new System.Drawing.Point(336, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Departamento";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label5.Location = new System.Drawing.Point(336, 276);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "Fecha";
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageDropzone);
            this.tabControlMain.Controls.Add(this.tabPageReembolsos);
            this.tabControlMain.Controls.Add(this.tabPageOptions);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControlMain.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControlMain.ItemSize = new System.Drawing.Size(150, 20);
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(765, 552);
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
            this.tabPageDropzone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPageDropzone.Location = new System.Drawing.Point(4, 24);
            this.tabPageDropzone.Margin = new System.Windows.Forms.Padding(8);
            this.tabPageDropzone.Name = "tabPageDropzone";
            this.tabPageDropzone.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDropzone.Size = new System.Drawing.Size(757, 524);
            this.tabPageDropzone.TabIndex = 0;
            this.tabPageDropzone.Text = "Dropzone";
            // 
            // lstClientSuggestions
            // 
            this.lstClientSuggestions.Font = new System.Drawing.Font("Malgun Gothic", 8.8F);
            this.lstClientSuggestions.FormattingEnabled = true;
            this.lstClientSuggestions.ItemHeight = 20;
            this.lstClientSuggestions.Location = new System.Drawing.Point(339, 49);
            this.lstClientSuggestions.Name = "lstClientSuggestions";
            this.lstClientSuggestions.Size = new System.Drawing.Size(249, 144);
            this.lstClientSuggestions.TabIndex = 13;
            this.lstClientSuggestions.Visible = false;
            // 
            // chkIncludeDay
            // 
            this.chkIncludeDay.AutoSize = true;
            this.chkIncludeDay.Font = new System.Drawing.Font("Malgun Gothic", 6.5F, System.Drawing.FontStyle.Bold);
            this.chkIncludeDay.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.chkIncludeDay.Location = new System.Drawing.Point(339, 331);
            this.chkIncludeDay.Name = "chkIncludeDay";
            this.chkIncludeDay.Size = new System.Drawing.Size(240, 17);
            this.chkIncludeDay.TabIndex = 12;
            this.chkIncludeDay.Text = "Incluir día en el nombre del documento";
            this.chkIncludeDay.UseVisualStyleBackColor = true;
            // 
            // tabPageReembolsos
            // 
            this.tabPageReembolsos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
            this.tabPageReembolsos.Controls.Add(this.txtReembolsosNotasAdicionales);
            this.tabPageReembolsos.Controls.Add(this.label8);
            this.tabPageReembolsos.Controls.Add(this.lstReembolsoClientSuggestions);
            this.tabPageReembolsos.Controls.Add(this.btnProcessReembolso);
            this.tabPageReembolsos.Controls.Add(this.rbDeleteOriginals);
            this.tabPageReembolsos.Controls.Add(this.rbSaveOriginalsToFolder);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoCedula);
            this.tabPageReembolsos.Controls.Add(this.label13);
            this.tabPageReembolsos.Controls.Add(this.cmbReembolsoAseguradora);
            this.tabPageReembolsos.Controls.Add(this.label12);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoMonto);
            this.tabPageReembolsos.Controls.Add(this.label11);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoPoliza);
            this.tabPageReembolsos.Controls.Add(this.label10);
            this.tabPageReembolsos.Controls.Add(this.label9);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoClientSearch);
            this.tabPageReembolsos.Controls.Add(this.panel2);
            this.tabPageReembolsos.ForeColor = System.Drawing.SystemColors.MenuText;
            this.tabPageReembolsos.Location = new System.Drawing.Point(4, 24);
            this.tabPageReembolsos.Name = "tabPageReembolsos";
            this.tabPageReembolsos.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageReembolsos.Size = new System.Drawing.Size(757, 524);
            this.tabPageReembolsos.TabIndex = 2;
            this.tabPageReembolsos.Text = "Reembolsos";
            // 
            // lstReembolsoClientSuggestions
            // 
            this.lstReembolsoClientSuggestions.Font = new System.Drawing.Font("Malgun Gothic", 8.8F);
            this.lstReembolsoClientSuggestions.FormattingEnabled = true;
            this.lstReembolsoClientSuggestions.ItemHeight = 20;
            this.lstReembolsoClientSuggestions.Location = new System.Drawing.Point(482, 57);
            this.lstReembolsoClientSuggestions.Name = "lstReembolsoClientSuggestions";
            this.lstReembolsoClientSuggestions.Size = new System.Drawing.Size(245, 144);
            this.lstReembolsoClientSuggestions.TabIndex = 16;
            this.lstReembolsoClientSuggestions.Visible = false;
            // 
            // btnProcessReembolso
            // 
            this.btnProcessReembolso.Location = new System.Drawing.Point(641, 468);
            this.btnProcessReembolso.Name = "btnProcessReembolso";
            this.btnProcessReembolso.Size = new System.Drawing.Size(99, 48);
            this.btnProcessReembolso.TabIndex = 3;
            this.btnProcessReembolso.Text = "Guardar";
            this.btnProcessReembolso.UseVisualStyleBackColor = true;
            // 
            // rbDeleteOriginals
            // 
            this.rbDeleteOriginals.AutoSize = true;
            this.rbDeleteOriginals.ForeColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.rbDeleteOriginals.Location = new System.Drawing.Point(473, 415);
            this.rbDeleteOriginals.Name = "rbDeleteOriginals";
            this.rbDeleteOriginals.Size = new System.Drawing.Size(189, 21);
            this.rbDeleteOriginals.TabIndex = 15;
            this.rbDeleteOriginals.TabStop = true;
            this.rbDeleteOriginals.Text = "Borrar archivos originales";
            this.rbDeleteOriginals.UseVisualStyleBackColor = true;
            this.rbDeleteOriginals.CheckedChanged += new System.EventHandler(this.rbDeleteOriginals_CheckedChanged);
            // 
            // rbSaveOriginalsToFolder
            // 
            this.rbSaveOriginalsToFolder.AutoSize = true;
            this.rbSaveOriginalsToFolder.ForeColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.rbSaveOriginalsToFolder.Location = new System.Drawing.Point(473, 442);
            this.rbSaveOriginalsToFolder.Name = "rbSaveOriginalsToFolder";
            this.rbSaveOriginalsToFolder.Size = new System.Drawing.Size(161, 21);
            this.rbSaveOriginalsToFolder.TabIndex = 14;
            this.rbSaveOriginalsToFolder.TabStop = true;
            this.rbSaveOriginalsToFolder.Text = "Guardar en OneDrive";
            this.rbSaveOriginalsToFolder.UseVisualStyleBackColor = true;
            this.rbSaveOriginalsToFolder.CheckedChanged += new System.EventHandler(this.rbSaveOriginalsToFolder_CheckedChanged);
            // 
            // txtReembolsoCedula
            // 
            this.txtReembolsoCedula.Location = new System.Drawing.Point(482, 90);
            this.txtReembolsoCedula.Name = "txtReembolsoCedula";
            this.txtReembolsoCedula.Size = new System.Drawing.Size(245, 25);
            this.txtReembolsoCedula.TabIndex = 13;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label13.Location = new System.Drawing.Point(479, 70);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 17);
            this.label13.TabIndex = 12;
            this.label13.Text = "Cédula";
            // 
            // cmbReembolsoAseguradora
            // 
            this.cmbReembolsoAseguradora.FormattingEnabled = true;
            this.cmbReembolsoAseguradora.Location = new System.Drawing.Point(482, 278);
            this.cmbReembolsoAseguradora.Name = "cmbReembolsoAseguradora";
            this.cmbReembolsoAseguradora.Size = new System.Drawing.Size(245, 25);
            this.cmbReembolsoAseguradora.TabIndex = 11;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label12.Location = new System.Drawing.Point(479, 258);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(86, 17);
            this.label12.TabIndex = 10;
            this.label12.Text = "Aseguradora";
            // 
            // txtReembolsoMonto
            // 
            this.txtReembolsoMonto.Location = new System.Drawing.Point(482, 216);
            this.txtReembolsoMonto.Name = "txtReembolsoMonto";
            this.txtReembolsoMonto.Size = new System.Drawing.Size(245, 25);
            this.txtReembolsoMonto.TabIndex = 9;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label11.Location = new System.Drawing.Point(479, 196);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(50, 17);
            this.label11.TabIndex = 8;
            this.label11.Text = "Monto";
            // 
            // txtReembolsoPoliza
            // 
            this.txtReembolsoPoliza.Location = new System.Drawing.Point(482, 153);
            this.txtReembolsoPoliza.Name = "txtReembolsoPoliza";
            this.txtReembolsoPoliza.Size = new System.Drawing.Size(245, 25);
            this.txtReembolsoPoliza.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label10.Location = new System.Drawing.Point(479, 133);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 17);
            this.label10.TabIndex = 6;
            this.label10.Text = "No. Póliza";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label9.Location = new System.Drawing.Point(479, 13);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 17);
            this.label9.TabIndex = 5;
            this.label9.Text = "Cliente";
            // 
            // txtReembolsoClientSearch
            // 
            this.txtReembolsoClientSearch.Location = new System.Drawing.Point(482, 33);
            this.txtReembolsoClientSearch.Name = "txtReembolsoClientSearch";
            this.txtReembolsoClientSearch.Size = new System.Drawing.Size(245, 25);
            this.txtReembolsoClientSearch.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(245)))), ((int)(((byte)(242)))));
            this.panel2.Controls.Add(this.btnClearReembolso);
            this.panel2.Controls.Add(this.btnRemoveSelectedFile);
            this.panel2.Controls.Add(this.dgvReembolsoFiles);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.pnlReembolsoDrop);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(464, 518);
            this.panel2.TabIndex = 0;
            // 
            // btnClearReembolso
            // 
            this.btnClearReembolso.Location = new System.Drawing.Point(19, 470);
            this.btnClearReembolso.Name = "btnClearReembolso";
            this.btnClearReembolso.Size = new System.Drawing.Size(93, 35);
            this.btnClearReembolso.TabIndex = 4;
            this.btnClearReembolso.Text = "Nuevo";
            this.btnClearReembolso.UseVisualStyleBackColor = true;
            this.btnClearReembolso.Click += new System.EventHandler(this.BtnClearReembolso_Click);
            // 
            // btnRemoveSelectedFile
            // 
            this.btnRemoveSelectedFile.Location = new System.Drawing.Point(288, 470);
            this.btnRemoveSelectedFile.Name = "btnRemoveSelectedFile";
            this.btnRemoveSelectedFile.Size = new System.Drawing.Size(163, 35);
            this.btnRemoveSelectedFile.TabIndex = 3;
            this.btnRemoveSelectedFile.Text = "Remover seleccionado";
            this.btnRemoveSelectedFile.UseVisualStyleBackColor = true;
            this.btnRemoveSelectedFile.Click += new System.EventHandler(this.BtnRemoveSelectedFile_Click);
            // 
            // dgvReembolsoFiles
            // 
            this.dgvReembolsoFiles.AllowUserToAddRows = false;
            this.dgvReembolsoFiles.AllowUserToDeleteRows = false;
            this.dgvReembolsoFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReembolsoFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Nombre,
            this.Tipo,
            this.Tamaño});
            this.dgvReembolsoFiles.Location = new System.Drawing.Point(19, 262);
            this.dgvReembolsoFiles.Name = "dgvReembolsoFiles";
            this.dgvReembolsoFiles.ReadOnly = true;
            this.dgvReembolsoFiles.RowHeadersWidth = 51;
            this.dgvReembolsoFiles.RowTemplate.Height = 24;
            this.dgvReembolsoFiles.Size = new System.Drawing.Size(432, 202);
            this.dgvReembolsoFiles.TabIndex = 2;
            // 
            // Nombre
            // 
            this.Nombre.HeaderText = "Nombre";
            this.Nombre.MinimumWidth = 6;
            this.Nombre.Name = "Nombre";
            this.Nombre.ReadOnly = true;
            this.Nombre.Width = 125;
            // 
            // Tipo
            // 
            this.Tipo.HeaderText = "Tipo";
            this.Tipo.MinimumWidth = 6;
            this.Tipo.Name = "Tipo";
            this.Tipo.ReadOnly = true;
            this.Tipo.Width = 125;
            // 
            // Tamaño
            // 
            this.Tamaño.HeaderText = "Tamaño";
            this.Tamaño.MinimumWidth = 6;
            this.Tamaño.Name = "Tamaño";
            this.Tamaño.ReadOnly = true;
            this.Tamaño.Width = 125;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Nirmala UI", 20.2F, System.Drawing.FontStyle.Bold);
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.label7.Location = new System.Drawing.Point(11, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(211, 46);
            this.label7.TabIndex = 1;
            this.label7.Text = "Reembolsos";
            // 
            // pnlReembolsoDrop
            // 
            this.pnlReembolsoDrop.Controls.Add(this.picDownloadIcon);
            this.pnlReembolsoDrop.Controls.Add(this.lblReembolsoDropPrompt);
            this.pnlReembolsoDrop.Location = new System.Drawing.Point(19, 68);
            this.pnlReembolsoDrop.Name = "pnlReembolsoDrop";
            this.pnlReembolsoDrop.Size = new System.Drawing.Size(432, 168);
            this.pnlReembolsoDrop.TabIndex = 0;
            // 
            // picDownloadIcon
            // 
            this.picDownloadIcon.Image = global::CoalcavaDropzone.Properties.Resources._1410633_200;
            this.picDownloadIcon.Location = new System.Drawing.Point(161, 0);
            this.picDownloadIcon.Name = "picDownloadIcon";
            this.picDownloadIcon.Size = new System.Drawing.Size(103, 139);
            this.picDownloadIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picDownloadIcon.TabIndex = 4;
            this.picDownloadIcon.TabStop = false;
            // 
            // lblReembolsoDropPrompt
            // 
            this.lblReembolsoDropPrompt.AutoSize = true;
            this.lblReembolsoDropPrompt.Location = new System.Drawing.Point(96, 139);
            this.lblReembolsoDropPrompt.Name = "lblReembolsoDropPrompt";
            this.lblReembolsoDropPrompt.Size = new System.Drawing.Size(227, 17);
            this.lblReembolsoDropPrompt.TabIndex = 3;
            this.lblReembolsoDropPrompt.Text = "Arrastra un archivo o haz clic aquí.";
            this.lblReembolsoDropPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPageOptions
            // 
            this.tabPageOptions.Controls.Add(this.label15);
            this.tabPageOptions.Controls.Add(this.btnBrowseReembolsoRoot);
            this.tabPageOptions.Controls.Add(this.txtReembolsoRootPath);
            this.tabPageOptions.Controls.Add(this.label14);
            this.tabPageOptions.Controls.Add(this.chkOpenOnStartup);
            this.tabPageOptions.Controls.Add(this.chkMuteSound);
            this.tabPageOptions.Controls.Add(this.btnSaveSettings);
            this.tabPageOptions.Controls.Add(this.btnBrowseRoot);
            this.tabPageOptions.Controls.Add(this.txtRootPath);
            this.tabPageOptions.Controls.Add(this.label6);
            this.tabPageOptions.Location = new System.Drawing.Point(4, 24);
            this.tabPageOptions.Name = "tabPageOptions";
            this.tabPageOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptions.Size = new System.Drawing.Size(757, 524);
            this.tabPageOptions.TabIndex = 1;
            this.tabPageOptions.Text = "Opciones";
            this.tabPageOptions.UseVisualStyleBackColor = true;
            // 
            // chkOpenOnStartup
            // 
            this.chkOpenOnStartup.AutoSize = true;
            this.chkOpenOnStartup.Location = new System.Drawing.Point(15, 249);
            this.chkOpenOnStartup.Name = "chkOpenOnStartup";
            this.chkOpenOnStartup.Size = new System.Drawing.Size(270, 21);
            this.chkOpenOnStartup.TabIndex = 6;
            this.chkOpenOnStartup.Text = "Iniciar automáticamente con Windows";
            this.chkOpenOnStartup.UseVisualStyleBackColor = true;
            // 
            // chkMuteSound
            // 
            this.chkMuteSound.AutoSize = true;
            this.chkMuteSound.Location = new System.Drawing.Point(15, 222);
            this.chkMuteSound.Name = "chkMuteSound";
            this.chkMuteSound.Size = new System.Drawing.Size(195, 21);
            this.chkMuteSound.TabIndex = 5;
            this.chkMuteSound.Text = "Silenciar sonidos de alerta";
            this.chkMuteSound.UseVisualStyleBackColor = true;
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(15, 428);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(138, 58);
            this.btnSaveSettings.TabIndex = 4;
            this.btnSaveSettings.Text = "Guardar";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click_1);
            // 
            // btnBrowseRoot
            // 
            this.btnBrowseRoot.Location = new System.Drawing.Point(422, 103);
            this.btnBrowseRoot.Name = "btnBrowseRoot";
            this.btnBrowseRoot.Size = new System.Drawing.Size(120, 25);
            this.btnBrowseRoot.TabIndex = 3;
            this.btnBrowseRoot.Text = "Buscar Ruta";
            this.btnBrowseRoot.UseVisualStyleBackColor = true;
            this.btnBrowseRoot.Click += new System.EventHandler(this.btnBrowseRoot_Click_1);
            // 
            // txtRootPath
            // 
            this.txtRootPath.Location = new System.Drawing.Point(15, 103);
            this.txtRootPath.Name = "txtRootPath";
            this.txtRootPath.ReadOnly = true;
            this.txtRootPath.Size = new System.Drawing.Size(401, 25);
            this.txtRootPath.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Lucida Sans Unicode", 13.2F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(22, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(176, 27);
            this.label6.TabIndex = 1;
            this.label6.Text = "Configuración";
            // 
            // txtReembolsosNotasAdicionales
            // 
            this.txtReembolsosNotasAdicionales.Location = new System.Drawing.Point(482, 336);
            this.txtReembolsosNotasAdicionales.Multiline = true;
            this.txtReembolsosNotasAdicionales.Name = "txtReembolsosNotasAdicionales";
            this.txtReembolsosNotasAdicionales.Size = new System.Drawing.Size(245, 73);
            this.txtReembolsosNotasAdicionales.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label8.Location = new System.Drawing.Point(479, 316);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 17);
            this.label8.TabIndex = 17;
            this.label8.Text = "Notas";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 83);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(126, 17);
            this.label14.TabIndex = 7;
            this.label14.Text = "Folder de Clientes ";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(12, 150);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(222, 17);
            this.label15.TabIndex = 10;
            this.label15.Text = "Folder de Reembolsos DROPZONE";
            // 
            // btnBrowseReembolsoRoot
            // 
            this.btnBrowseReembolsoRoot.Location = new System.Drawing.Point(422, 170);
            this.btnBrowseReembolsoRoot.Name = "btnBrowseReembolsoRoot";
            this.btnBrowseReembolsoRoot.Size = new System.Drawing.Size(120, 25);
            this.btnBrowseReembolsoRoot.TabIndex = 9;
            this.btnBrowseReembolsoRoot.Text = "Buscar Ruta";
            this.btnBrowseReembolsoRoot.UseVisualStyleBackColor = true;
            // 
            // txtReembolsoRootPath
            // 
            this.txtReembolsoRootPath.Location = new System.Drawing.Point(15, 170);
            this.txtReembolsoRootPath.Name = "txtReembolsoRootPath";
            this.txtReembolsoRootPath.ReadOnly = true;
            this.txtReembolsoRootPath.Size = new System.Drawing.Size(401, 25);
            this.txtReembolsoRootPath.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
            this.ClientSize = new System.Drawing.Size(756, 552);
            this.Controls.Add(this.tabControlMain);
            this.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
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
            this.tabPageReembolsos.ResumeLayout(false);
            this.tabPageReembolsos.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReembolsoFiles)).EndInit();
            this.pnlReembolsoDrop.ResumeLayout(false);
            this.pnlReembolsoDrop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDownloadIcon)).EndInit();
            this.tabPageOptions.ResumeLayout(false);
            this.tabPageOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSort;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlDropzone;
        private System.Windows.Forms.Label label1;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
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
        private System.Windows.Forms.CheckBox chkOpenOnStartup;
        private System.Windows.Forms.CheckBox chkMuteSound;
        private System.Windows.Forms.TabPage tabPageReembolsos;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel pnlReembolsoDrop;
        private System.Windows.Forms.TextBox txtReembolsoClientSearch;
        private System.Windows.Forms.DataGridView dgvReembolsoFiles;
        private System.Windows.Forms.DataGridViewLinkColumn Nombre;
        private System.Windows.Forms.DataGridViewComboBoxColumn Tipo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tamaño;
        private System.Windows.Forms.Label lblReembolsoDropPrompt;
        private System.Windows.Forms.RadioButton rbDeleteOriginals;
        private System.Windows.Forms.RadioButton rbSaveOriginalsToFolder;
        private System.Windows.Forms.TextBox txtReembolsoCedula;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cmbReembolsoAseguradora;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtReembolsoMonto;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtReembolsoPoliza;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnProcessReembolso;
        private System.Windows.Forms.Button btnRemoveSelectedFile;
        private System.Windows.Forms.Button btnClearReembolso;
        private System.Windows.Forms.PictureBox pbDownloadIcon;
        private System.Windows.Forms.ListBox lstReembolsoClientSuggestions;
        private System.Windows.Forms.PictureBox picDownloadIcon;
        private System.Windows.Forms.TextBox txtReembolsosNotasAdicionales;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnBrowseReembolsoRoot;
        private System.Windows.Forms.TextBox txtReembolsoRootPath;
        private System.Windows.Forms.Label label14;
    }
}

