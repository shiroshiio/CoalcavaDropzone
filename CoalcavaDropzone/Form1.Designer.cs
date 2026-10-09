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
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageReembolsos = new System.Windows.Forms.TabPage();
            this.pnlCompletivos = new System.Windows.Forms.Panel();
            this.chkSendReminderComp = new System.Windows.Forms.CheckBox();
            this.dtpReminderTimeComp = new System.Windows.Forms.DateTimePicker();
            this.txtCompletivoNotas = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtTargetCaseId = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtReembolsoCedula = new System.Windows.Forms.TextBox();
            this.lstReembolsoClientSuggestions = new System.Windows.Forms.ListBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cmbReembolsoAseguradora = new System.Windows.Forms.ComboBox();
            this.chkSendReminder = new System.Windows.Forms.CheckBox();
            this.dtpReminderTime = new System.Windows.Forms.DateTimePicker();
            this.cmbAfiliado = new System.Windows.Forms.ComboBox();
            this.txtReembolsoPoliza = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.cmbCurrency = new System.Windows.Forms.ComboBox();
            this.txtReembolsosNotasAdicionales = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnProcessReembolso = new System.Windows.Forms.Button();
            this.lblPassportID = new System.Windows.Forms.Label();
            this.txtReembolsoMonto = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtReembolsoClientSearch = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnToggleAuth = new System.Windows.Forms.Button();
            this.btnToggleMode = new System.Windows.Forms.Button();
            this.btnClearReembolso = new System.Windows.Forms.Button();
            this.btnRemoveSelectedFile = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.dgvReembolsoFiles = new System.Windows.Forms.DataGridView();
            this.Nombre = new System.Windows.Forms.DataGridViewLinkColumn();
            this.Tipo = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Tamaño = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label7 = new System.Windows.Forms.Label();
            this.pnlReembolsoDrop = new System.Windows.Forms.Panel();
            this.picDownloadIcon = new System.Windows.Forms.PictureBox();
            this.lblReembolsoDropPrompt = new System.Windows.Forms.Label();
            this.tabPageOptions = new System.Windows.Forms.TabPage();
            this.txtAppUser = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.btnBrowseReembolsoRoot = new System.Windows.Forms.Button();
            this.txtReembolsoRootPath = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.chkOpenOnStartup = new System.Windows.Forms.CheckBox();
            this.chkMuteSound = new System.Windows.Forms.CheckBox();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.btnBrowseRoot = new System.Windows.Forms.Button();
            this.txtRootPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlDropzone = new System.Windows.Forms.Panel();
            this.lblFileName = new System.Windows.Forms.Label();
            this.pbDownloadIcon = new System.Windows.Forms.PictureBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnQuickGrab = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnOpenDoc = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            this.tabControlMain.SuspendLayout();
            this.tabPageReembolsos.SuspendLayout();
            this.pnlCompletivos.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReembolsoFiles)).BeginInit();
            this.pnlReembolsoDrop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDownloadIcon)).BeginInit();
            this.tabPageOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDownloadIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageReembolsos);
            this.tabControlMain.Controls.Add(this.tabPageOptions);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControlMain.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControlMain.ItemSize = new System.Drawing.Size(150, 20);
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(765, 652);
            this.tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlMain.TabIndex = 6;
            // 
            // tabPageReembolsos
            // 
            this.tabPageReembolsos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
            this.tabPageReembolsos.Controls.Add(this.pnlCompletivos);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoCedula);
            this.tabPageReembolsos.Controls.Add(this.lstReembolsoClientSuggestions);
            this.tabPageReembolsos.Controls.Add(this.label12);
            this.tabPageReembolsos.Controls.Add(this.cmbReembolsoAseguradora);
            this.tabPageReembolsos.Controls.Add(this.chkSendReminder);
            this.tabPageReembolsos.Controls.Add(this.dtpReminderTime);
            this.tabPageReembolsos.Controls.Add(this.cmbAfiliado);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoPoliza);
            this.tabPageReembolsos.Controls.Add(this.label18);
            this.tabPageReembolsos.Controls.Add(this.cmbCurrency);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsosNotasAdicionales);
            this.tabPageReembolsos.Controls.Add(this.label8);
            this.tabPageReembolsos.Controls.Add(this.btnProcessReembolso);
            this.tabPageReembolsos.Controls.Add(this.lblPassportID);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoMonto);
            this.tabPageReembolsos.Controls.Add(this.label11);
            this.tabPageReembolsos.Controls.Add(this.label10);
            this.tabPageReembolsos.Controls.Add(this.label9);
            this.tabPageReembolsos.Controls.Add(this.txtReembolsoClientSearch);
            this.tabPageReembolsos.Controls.Add(this.panel2);
            this.tabPageReembolsos.ForeColor = System.Drawing.SystemColors.MenuText;
            this.tabPageReembolsos.Location = new System.Drawing.Point(4, 24);
            this.tabPageReembolsos.Name = "tabPageReembolsos";
            this.tabPageReembolsos.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageReembolsos.Size = new System.Drawing.Size(757, 624);
            this.tabPageReembolsos.TabIndex = 2;
            this.tabPageReembolsos.Text = "Reembolsos";
            // 
            // pnlCompletivos
            // 
            this.pnlCompletivos.Controls.Add(this.chkSendReminderComp);
            this.pnlCompletivos.Controls.Add(this.dtpReminderTimeComp);
            this.pnlCompletivos.Controls.Add(this.txtCompletivoNotas);
            this.pnlCompletivos.Controls.Add(this.label16);
            this.pnlCompletivos.Controls.Add(this.txtTargetCaseId);
            this.pnlCompletivos.Controls.Add(this.label13);
            this.pnlCompletivos.Location = new System.Drawing.Point(469, 0);
            this.pnlCompletivos.Name = "pnlCompletivos";
            this.pnlCompletivos.Size = new System.Drawing.Size(288, 526);
            this.pnlCompletivos.TabIndex = 7;
            // 
            // chkSendReminderComp
            // 
            this.chkSendReminderComp.AutoSize = true;
            this.chkSendReminderComp.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.chkSendReminderComp.Location = new System.Drawing.Point(13, 208);
            this.chkSendReminderComp.Name = "chkSendReminderComp";
            this.chkSendReminderComp.Size = new System.Drawing.Size(169, 21);
            this.chkSendReminderComp.TabIndex = 27;
            this.chkSendReminderComp.Text = "Fecha de Recordatorio";
            this.chkSendReminderComp.UseVisualStyleBackColor = true;
            // 
            // dtpReminderTimeComp
            // 
            this.dtpReminderTimeComp.Location = new System.Drawing.Point(13, 231);
            this.dtpReminderTimeComp.Name = "dtpReminderTimeComp";
            this.dtpReminderTimeComp.Size = new System.Drawing.Size(245, 25);
            this.dtpReminderTimeComp.TabIndex = 26;
            // 
            // txtCompletivoNotas
            // 
            this.txtCompletivoNotas.Location = new System.Drawing.Point(13, 90);
            this.txtCompletivoNotas.Multiline = true;
            this.txtCompletivoNotas.Name = "txtCompletivoNotas";
            this.txtCompletivoNotas.Size = new System.Drawing.Size(245, 111);
            this.txtCompletivoNotas.TabIndex = 21;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label16.Location = new System.Drawing.Point(10, 70);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(44, 17);
            this.label16.TabIndex = 20;
            this.label16.Text = "Notas";
            // 
            // txtTargetCaseId
            // 
            this.txtTargetCaseId.Location = new System.Drawing.Point(13, 33);
            this.txtTargetCaseId.Name = "txtTargetCaseId";
            this.txtTargetCaseId.Size = new System.Drawing.Size(245, 25);
            this.txtTargetCaseId.TabIndex = 20;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label13.Location = new System.Drawing.Point(10, 12);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(76, 17);
            this.label13.TabIndex = 20;
            this.label13.Text = "ID de Caso";
            // 
            // txtReembolsoCedula
            // 
            this.txtReembolsoCedula.Location = new System.Drawing.Point(482, 195);
            this.txtReembolsoCedula.Name = "txtReembolsoCedula";
            this.txtReembolsoCedula.Size = new System.Drawing.Size(245, 25);
            this.txtReembolsoCedula.TabIndex = 29;
            // 
            // lstReembolsoClientSuggestions
            // 
            this.lstReembolsoClientSuggestions.Font = new System.Drawing.Font("Malgun Gothic", 8.8F);
            this.lstReembolsoClientSuggestions.FormattingEnabled = true;
            this.lstReembolsoClientSuggestions.ItemHeight = 20;
            this.lstReembolsoClientSuggestions.Location = new System.Drawing.Point(482, 51);
            this.lstReembolsoClientSuggestions.Name = "lstReembolsoClientSuggestions";
            this.lstReembolsoClientSuggestions.Size = new System.Drawing.Size(245, 144);
            this.lstReembolsoClientSuggestions.TabIndex = 16;
            this.lstReembolsoClientSuggestions.Visible = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label12.Location = new System.Drawing.Point(479, 235);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(86, 17);
            this.label12.TabIndex = 10;
            this.label12.Text = "Aseguradora";
            // 
            // cmbReembolsoAseguradora
            // 
            this.cmbReembolsoAseguradora.FormattingEnabled = true;
            this.cmbReembolsoAseguradora.Location = new System.Drawing.Point(482, 255);
            this.cmbReembolsoAseguradora.Name = "cmbReembolsoAseguradora";
            this.cmbReembolsoAseguradora.Size = new System.Drawing.Size(245, 25);
            this.cmbReembolsoAseguradora.TabIndex = 11;
            // 
            // chkSendReminder
            // 
            this.chkSendReminder.AutoSize = true;
            this.chkSendReminder.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.chkSendReminder.Location = new System.Drawing.Point(482, 478);
            this.chkSendReminder.Name = "chkSendReminder";
            this.chkSendReminder.Size = new System.Drawing.Size(169, 21);
            this.chkSendReminder.TabIndex = 28;
            this.chkSendReminder.Text = "Fecha de Recordatorio";
            this.chkSendReminder.UseVisualStyleBackColor = true;
            // 
            // dtpReminderTime
            // 
            this.dtpReminderTime.Location = new System.Drawing.Point(482, 501);
            this.dtpReminderTime.Name = "dtpReminderTime";
            this.dtpReminderTime.Size = new System.Drawing.Size(245, 25);
            this.dtpReminderTime.TabIndex = 24;
            // 
            // cmbAfiliado
            // 
            this.cmbAfiliado.FormattingEnabled = true;
            this.cmbAfiliado.Location = new System.Drawing.Point(482, 139);
            this.cmbAfiliado.Name = "cmbAfiliado";
            this.cmbAfiliado.Size = new System.Drawing.Size(245, 25);
            this.cmbAfiliado.TabIndex = 21;
            // 
            // txtReembolsoPoliza
            // 
            this.txtReembolsoPoliza.FormattingEnabled = true;
            this.txtReembolsoPoliza.Location = new System.Drawing.Point(482, 84);
            this.txtReembolsoPoliza.Name = "txtReembolsoPoliza";
            this.txtReembolsoPoliza.Size = new System.Drawing.Size(245, 25);
            this.txtReembolsoPoliza.TabIndex = 22;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label18.Location = new System.Drawing.Point(479, 119);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(57, 17);
            this.label18.TabIndex = 20;
            this.label18.Text = "Afiliado";
            // 
            // cmbCurrency
            // 
            this.cmbCurrency.FormattingEnabled = true;
            this.cmbCurrency.Location = new System.Drawing.Point(650, 314);
            this.cmbCurrency.Name = "cmbCurrency";
            this.cmbCurrency.Size = new System.Drawing.Size(77, 25);
            this.cmbCurrency.TabIndex = 19;
            // 
            // txtReembolsosNotasAdicionales
            // 
            this.txtReembolsosNotasAdicionales.Location = new System.Drawing.Point(482, 362);
            this.txtReembolsosNotasAdicionales.Multiline = true;
            this.txtReembolsosNotasAdicionales.Name = "txtReembolsosNotasAdicionales";
            this.txtReembolsosNotasAdicionales.Size = new System.Drawing.Size(245, 113);
            this.txtReembolsosNotasAdicionales.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label8.Location = new System.Drawing.Point(479, 342);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 17);
            this.label8.TabIndex = 17;
            this.label8.Text = "Notas";
            // 
            // btnProcessReembolso
            // 
            this.btnProcessReembolso.Location = new System.Drawing.Point(652, 570);
            this.btnProcessReembolso.Name = "btnProcessReembolso";
            this.btnProcessReembolso.Size = new System.Drawing.Size(99, 48);
            this.btnProcessReembolso.TabIndex = 3;
            this.btnProcessReembolso.Text = "Guardar";
            this.btnProcessReembolso.UseVisualStyleBackColor = true;
            // 
            // lblPassportID
            // 
            this.lblPassportID.AutoSize = true;
            this.lblPassportID.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassportID.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblPassportID.Location = new System.Drawing.Point(479, 175);
            this.lblPassportID.Name = "lblPassportID";
            this.lblPassportID.Size = new System.Drawing.Size(83, 17);
            this.lblPassportID.TabIndex = 12;
            this.lblPassportID.Text = "No. Afiliado";
            // 
            // txtReembolsoMonto
            // 
            this.txtReembolsoMonto.Location = new System.Drawing.Point(482, 314);
            this.txtReembolsoMonto.Name = "txtReembolsoMonto";
            this.txtReembolsoMonto.Size = new System.Drawing.Size(168, 25);
            this.txtReembolsoMonto.TabIndex = 9;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label11.Location = new System.Drawing.Point(479, 294);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(50, 17);
            this.label11.TabIndex = 8;
            this.label11.Text = "Monto";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label10.Location = new System.Drawing.Point(479, 64);
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
            this.label9.Location = new System.Drawing.Point(479, 8);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 17);
            this.label9.TabIndex = 5;
            this.label9.Text = "Cliente";
            // 
            // txtReembolsoClientSearch
            // 
            this.txtReembolsoClientSearch.Location = new System.Drawing.Point(482, 28);
            this.txtReembolsoClientSearch.Name = "txtReembolsoClientSearch";
            this.txtReembolsoClientSearch.Size = new System.Drawing.Size(245, 25);
            this.txtReembolsoClientSearch.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(245)))), ((int)(((byte)(242)))));
            this.panel2.Controls.Add(this.btnToggleAuth);
            this.panel2.Controls.Add(this.btnToggleMode);
            this.panel2.Controls.Add(this.btnClearReembolso);
            this.panel2.Controls.Add(this.btnRemoveSelectedFile);
            this.panel2.Controls.Add(this.label19);
            this.panel2.Controls.Add(this.dgvReembolsoFiles);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.pnlReembolsoDrop);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(464, 618);
            this.panel2.TabIndex = 0;
            // 
            // btnToggleAuth
            // 
            this.btnToggleAuth.Location = new System.Drawing.Point(319, 536);
            this.btnToggleAuth.Name = "btnToggleAuth";
            this.btnToggleAuth.Size = new System.Drawing.Size(122, 35);
            this.btnToggleAuth.TabIndex = 6;
            this.btnToggleAuth.Text = "Autorización";
            this.btnToggleAuth.UseVisualStyleBackColor = true;
            // 
            // btnToggleMode
            // 
            this.btnToggleMode.Location = new System.Drawing.Point(319, 577);
            this.btnToggleMode.Name = "btnToggleMode";
            this.btnToggleMode.Size = new System.Drawing.Size(122, 35);
            this.btnToggleMode.TabIndex = 5;
            this.btnToggleMode.Text = "Completivo";
            this.btnToggleMode.UseVisualStyleBackColor = true;
            // 
            // btnClearReembolso
            // 
            this.btnClearReembolso.Location = new System.Drawing.Point(19, 536);
            this.btnClearReembolso.Name = "btnClearReembolso";
            this.btnClearReembolso.Size = new System.Drawing.Size(93, 35);
            this.btnClearReembolso.TabIndex = 4;
            this.btnClearReembolso.Text = "Nuevo";
            this.btnClearReembolso.UseVisualStyleBackColor = true;
            this.btnClearReembolso.Click += new System.EventHandler(this.BtnClearReembolso_Click);
            // 
            // btnRemoveSelectedFile
            // 
            this.btnRemoveSelectedFile.Location = new System.Drawing.Point(118, 536);
            this.btnRemoveSelectedFile.Name = "btnRemoveSelectedFile";
            this.btnRemoveSelectedFile.Size = new System.Drawing.Size(195, 35);
            this.btnRemoveSelectedFile.TabIndex = 3;
            this.btnRemoveSelectedFile.Text = "Remover seleccionado";
            this.btnRemoveSelectedFile.UseVisualStyleBackColor = true;
            this.btnRemoveSelectedFile.Click += new System.EventHandler(this.BtnRemoveSelectedFile_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Malgun Gothic", 4.8F, System.Drawing.FontStyle.Bold);
            this.label19.Location = new System.Drawing.Point(3, 605);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(40, 11);
            this.label19.TabIndex = 6;
            this.label19.Text = "ver. 0.2.3";
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
            this.dgvReembolsoFiles.Location = new System.Drawing.Point(19, 241);
            this.dgvReembolsoFiles.Name = "dgvReembolsoFiles";
            this.dgvReembolsoFiles.RowHeadersWidth = 51;
            this.dgvReembolsoFiles.RowTemplate.Height = 24;
            this.dgvReembolsoFiles.Size = new System.Drawing.Size(432, 289);
            this.dgvReembolsoFiles.TabIndex = 2;
            // 
            // Nombre
            // 
            this.Nombre.HeaderText = "Nombre";
            this.Nombre.MinimumWidth = 6;
            this.Nombre.Name = "Nombre";
            this.Nombre.Width = 125;
            // 
            // Tipo
            // 
            this.Tipo.HeaderText = "Tipo";
            this.Tipo.MinimumWidth = 6;
            this.Tipo.Name = "Tipo";
            this.Tipo.Width = 125;
            // 
            // Tamaño
            // 
            this.Tamaño.HeaderText = "Tamaño";
            this.Tamaño.MinimumWidth = 6;
            this.Tamaño.Name = "Tamaño";
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
            this.pnlReembolsoDrop.Size = new System.Drawing.Size(432, 167);
            this.pnlReembolsoDrop.TabIndex = 0;
            // 
            // picDownloadIcon
            // 
            this.picDownloadIcon.Image = global::CoalcavaDropzone.Properties.Resources._1410633_200;
            this.picDownloadIcon.Location = new System.Drawing.Point(149, 0);
            this.picDownloadIcon.Name = "picDownloadIcon";
            this.picDownloadIcon.Size = new System.Drawing.Size(126, 142);
            this.picDownloadIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picDownloadIcon.TabIndex = 4;
            this.picDownloadIcon.TabStop = false;
            // 
            // lblReembolsoDropPrompt
            // 
            this.lblReembolsoDropPrompt.AutoSize = true;
            this.lblReembolsoDropPrompt.Location = new System.Drawing.Point(96, 145);
            this.lblReembolsoDropPrompt.Name = "lblReembolsoDropPrompt";
            this.lblReembolsoDropPrompt.Size = new System.Drawing.Size(227, 17);
            this.lblReembolsoDropPrompt.TabIndex = 3;
            this.lblReembolsoDropPrompt.Text = "Arrastra un archivo o haz clic aquí.";
            this.lblReembolsoDropPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPageOptions
            // 
            this.tabPageOptions.Controls.Add(this.txtAppUser);
            this.tabPageOptions.Controls.Add(this.label17);
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
            this.tabPageOptions.Size = new System.Drawing.Size(757, 624);
            this.tabPageOptions.TabIndex = 1;
            this.tabPageOptions.Text = "Opciones";
            this.tabPageOptions.UseVisualStyleBackColor = true;
            // 
            // txtAppUser
            // 
            this.txtAppUser.Location = new System.Drawing.Point(15, 236);
            this.txtAppUser.Name = "txtAppUser";
            this.txtAppUser.Size = new System.Drawing.Size(401, 25);
            this.txtAppUser.TabIndex = 13;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(12, 215);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(100, 17);
            this.label17.TabIndex = 12;
            this.label17.Text = "Usuario Activo";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(12, 150);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(223, 17);
            this.label15.TabIndex = 10;
            this.label15.Text = "Folder de Recepción [Reembolsos]";
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
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 83);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(126, 17);
            this.label14.TabIndex = 7;
            this.label14.Text = "Folder de Clientes ";
            // 
            // chkOpenOnStartup
            // 
            this.chkOpenOnStartup.AutoSize = true;
            this.chkOpenOnStartup.Location = new System.Drawing.Point(15, 312);
            this.chkOpenOnStartup.Name = "chkOpenOnStartup";
            this.chkOpenOnStartup.Size = new System.Drawing.Size(270, 21);
            this.chkOpenOnStartup.TabIndex = 6;
            this.chkOpenOnStartup.Text = "Iniciar automáticamente con Windows";
            this.chkOpenOnStartup.UseVisualStyleBackColor = true;
            // 
            // chkMuteSound
            // 
            this.chkMuteSound.AutoSize = true;
            this.chkMuteSound.Location = new System.Drawing.Point(15, 285);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Nirmala UI", 20.2F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(14, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 46);
            this.label1.TabIndex = 0;
            // 
            // pnlDropzone
            // 
            this.pnlDropzone.AllowDrop = true;
            this.pnlDropzone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.pnlDropzone.Location = new System.Drawing.Point(13, 68);
            this.pnlDropzone.Name = "pnlDropzone";
            this.pnlDropzone.Size = new System.Drawing.Size(282, 275);
            this.pnlDropzone.TabIndex = 1;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(22, 248);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(227, 17);
            this.lblFileName.TabIndex = 2;
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(282, 245);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 2;
            this.picPreview.TabStop = false;
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
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
            this.ClientSize = new System.Drawing.Size(768, 652);
            this.Controls.Add(this.tabControlMain);
            this.Font = new System.Drawing.Font("Malgun Gothic", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "COALCAVA Dropzone";
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageReembolsos.ResumeLayout(false);
            this.tabPageReembolsos.PerformLayout();
            this.pnlCompletivos.ResumeLayout(false);
            this.pnlCompletivos.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReembolsoFiles)).EndInit();
            this.pnlReembolsoDrop.ResumeLayout(false);
            this.pnlReembolsoDrop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDownloadIcon)).EndInit();
            this.tabPageOptions.ResumeLayout(false);
            this.tabPageOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDownloadIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageOptions;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.Button btnBrowseRoot;
        private System.Windows.Forms.TextBox txtRootPath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkOpenOnStartup;
        private System.Windows.Forms.CheckBox chkMuteSound;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel pnlReembolsoDrop;
        private System.Windows.Forms.TextBox txtReembolsoClientSearch;
        private System.Windows.Forms.DataGridView dgvReembolsoFiles;
        private System.Windows.Forms.DataGridViewLinkColumn Nombre;
        private System.Windows.Forms.DataGridViewComboBoxColumn Tipo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tamaño;
        private System.Windows.Forms.Label lblReembolsoDropPrompt;
        private System.Windows.Forms.Label lblPassportID;
        private System.Windows.Forms.TextBox txtReembolsoMonto;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnProcessReembolso;
        private System.Windows.Forms.Button btnRemoveSelectedFile;
        private System.Windows.Forms.Button btnClearReembolso;
        private System.Windows.Forms.ListBox lstReembolsoClientSuggestions;
        private System.Windows.Forms.PictureBox picDownloadIcon;
        private System.Windows.Forms.TextBox txtReembolsosNotasAdicionales;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnBrowseReembolsoRoot;
        private System.Windows.Forms.TextBox txtReembolsoRootPath;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox cmbCurrency;
        private System.Windows.Forms.Panel pnlCompletivos;
        private System.Windows.Forms.TextBox txtCompletivoNotas;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtTargetCaseId;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnToggleMode;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtAppUser;
        private System.Windows.Forms.ComboBox cmbAfiliado;
        private System.Windows.Forms.ComboBox txtReembolsoPoliza;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ComboBox cmbReembolsoAseguradora;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TabPage tabPageReembolsos;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.DateTimePicker dtpReminderTimeComp;
        private System.Windows.Forms.DateTimePicker dtpReminderTime;
        private System.Windows.Forms.CheckBox chkSendReminderComp;
        private System.Windows.Forms.CheckBox chkSendReminder;
        private System.Windows.Forms.TextBox txtReembolsoCedula;
        private System.Windows.Forms.Button btnToggleAuth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlDropzone;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.PictureBox pbDownloadIcon;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Button btnQuickGrab;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnOpenDoc;
    }
}

