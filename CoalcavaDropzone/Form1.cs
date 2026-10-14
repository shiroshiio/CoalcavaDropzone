using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CoalcavaDropzone
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool AnimateWindow(IntPtr hwnd, int timeMs, int flags);

        private const int AW_ACTIVATE = 0x20000;
        private const int AW_BLEND = 0x80000; // Native 250ms fade-in

        // Rutas y configuración general
        private string clientRootPath = @"C:\Users\Public\OneDrive - COALCAVA\Clientes\";
        private string dropzoneFlowPath = @"C:\Users\Public\OneDrive - COALCAVA\DROPZONE_FLOW\";
        private List<string> cachedClientList = new List<string>();

        // Placeholders para campos de Reembolso, Autorizaciones y Completivos
        private const string PlaceholderText = "Seleccionar o buscar cliente...";
        private const string PlaceholderPoliza = "POL-00000000";
        private const string PlaceholderCedula = "0000000001";
        private const string PlaceholderMonto = "0.00";
        private const string PlaceholderNotas = "Notas adicionales, diagnósticos o instrucciones de la reclamación...";
        private const string PlaceholderTargetCaseId = "REE-XX-000000";
        private const string PlaceholderCompletivoNotas = "Detalles o justificación de los documentos completivos agregados...";
        private const string PlaceholderAppUser = "John Appleseed";

        private readonly Color NormalDropzoneColor = Color.FromArgb(248, 248, 246);
        private readonly Color HoverDropzoneColor = Color.FromArgb(254, 243, 235);

        // Estado del modo actual
        private bool isCompletivoMode = false;
        private bool isAuthMode = false;

        // Estructura interna para los ítems de Reembolso / Autorización
        public class ReembolsoFileItem
        {
            public string FullPath { get; set; }
            public string FileName => Path.GetFileName(FullPath);
            public string DocumentType { get; set; }
            public long FileSizeBytes => File.Exists(FullPath) ? new FileInfo(FullPath).Length : 0;
            public string FileSizeFormatted => $"{(FileSizeBytes / 1024.0 / 1024.0):F2} MB";
        }

        private List<ReembolsoFileItem> reembolsoItems = new List<ReembolsoFileItem>();

        private readonly string[] docTypeOptions = new string[]
        {
            "Formulario de Solicitud",
            "Factura",
            "Recibo de Pago Sellado",
            "Indicación Médica",
            "Historia Clínica",
            "Resultados de Estudios",
            "Cédula / Documento de Identidad",
            "Otros"
        };

        public Form1()
        {
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            SetupReembolsosUI();
            this.Load += Form1_Load;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                AnimateWindow(this.Handle, 250, AW_BLEND | AW_ACTIVATE);
            }
            catch { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;

            try
            {
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ClientRootPath))
                {
                    clientRootPath = Properties.Settings.Default.ClientRootPath;
                }

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ReembolsoRootPath))
                {
                    dropzoneFlowPath = Properties.Settings.Default.ReembolsoRootPath;
                }
            }
            catch { }

            clientRootPath = clientRootPath.Replace('/', '\\');
            if (!clientRootPath.EndsWith(@"\")) clientRootPath += @"\";

            dropzoneFlowPath = dropzoneFlowPath.Replace('/', '\\');
            if (!dropzoneFlowPath.EndsWith(@"\")) dropzoneFlowPath += @"\";

            if (txtRootPath != null)
            {
                txtRootPath.Text = clientRootPath;
            }

            if (txtReembolsoRootPath != null)
            {
                txtReembolsoRootPath.Text = dropzoneFlowPath;
            }

            InitializeReembolsosTab();
            SetupSettingsTab();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList().Cast<string>().ToArray();
                    if (files.Length > 0)
                    {
                        AddFilesToReembolsoQueue(files);
                    }
                }
            }
        }

        #region Helpers Generales

        private void PlaySound(SystemSound sound)
        {
            bool isMuted = chkMuteSound != null && chkMuteSound.Checked;
            if (!isMuted && sound != null)
            {
                sound.Play();
            }
        }

        private void SetStartupRegistry(bool enableStartup)
        {
            try
            {
                string runKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKeyPath, true))
                {
                    if (key != null)
                    {
                        if (enableStartup)
                        {
                            key.SetValue("CoalcavaDropzone", Application.ExecutablePath);
                        }
                        else
                        {
                            key.DeleteValue("CoalcavaDropzone", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al actualizar registro de inicio: {ex.Message}");
            }
        }

        private void ApplyPlaceholder(Control ctrl, string placeholder)
        {
            if (ctrl == null) return;

            ctrl.Tag = placeholder;
            if (string.IsNullOrWhiteSpace(ctrl.Text) || ctrl.Text == placeholder)
            {
                ctrl.Text = placeholder;
                ctrl.ForeColor = Color.Gray;
            }

            ctrl.GotFocus += (s, e) =>
            {
                if (ctrl.Text == placeholder)
                {
                    ctrl.Text = "";
                    ctrl.ForeColor = Color.Black;
                }
            };

            ctrl.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(ctrl.Text))
                {
                    ctrl.Text = placeholder;
                    ctrl.ForeColor = Color.Gray;
                }
                else if (ctrl.Text != placeholder)
                {
                    ctrl.ForeColor = Color.Black;
                }
            };
        }

        private void ResetPlaceholder(Control ctrl, string placeholder)
        {
            if (ctrl == null) return;
            ctrl.Tag = placeholder;
            ctrl.Text = placeholder;
            ctrl.ForeColor = Color.Gray;
        }

        private string GetCleanText(Control ctrl)
        {
            if (ctrl == null) return string.Empty;
            string placeholder = ctrl.Tag?.ToString() ?? "";
            string val = ctrl.Text.Trim();
            return (val == placeholder) ? string.Empty : val;
        }

        private DateTime GetDefaultReminderDateTime()
        {
            return DateTime.Today.AddDays(2).AddHours(8); // 2 días en el futuro a las 8:00 AM
        }

        private void LoadClientFolders()
        {
            cachedClientList.Clear();
            if (Directory.Exists(clientRootPath))
            {
                var directories = Directory.GetDirectories(clientRootPath);
                foreach (var dir in directories)
                {
                    cachedClientList.Add(new DirectoryInfo(dir).Name);
                }
            }
        }

        #endregion

        #region Reembolsos, Autorizaciones & Completivos

        private void SetupReembolsosUI()
        {
            pnlReembolsoDrop.AllowDrop = true;
            pnlReembolsoDrop.DragEnter -= PnlReembolsoDrop_DragEnter;
            pnlReembolsoDrop.DragEnter += PnlReembolsoDrop_DragEnter;
            pnlReembolsoDrop.DragLeave -= PnlReembolsoDrop_DragLeave;
            pnlReembolsoDrop.DragLeave += PnlReembolsoDrop_DragLeave;
            pnlReembolsoDrop.DragDrop -= PnlReembolsoDrop_DragDrop;
            pnlReembolsoDrop.DragDrop += PnlReembolsoDrop_DragDrop;
            pnlReembolsoDrop.Click -= PnlReembolsoDrop_Click;
            pnlReembolsoDrop.Click += PnlReembolsoDrop_Click;

            if (picDownloadIcon != null)
            {
                picDownloadIcon.AllowDrop = true;
                picDownloadIcon.BackColor = Color.Transparent;
                picDownloadIcon.DragEnter -= PnlReembolsoDrop_DragEnter;
                picDownloadIcon.DragEnter += PnlReembolsoDrop_DragEnter;
                picDownloadIcon.DragLeave -= PnlReembolsoDrop_DragLeave;
                picDownloadIcon.DragLeave += PnlReembolsoDrop_DragLeave;
                picDownloadIcon.DragDrop -= PnlReembolsoDrop_DragDrop;
                picDownloadIcon.DragDrop += PnlReembolsoDrop_DragDrop;
                picDownloadIcon.Click -= PnlReembolsoDrop_Click;
                picDownloadIcon.Click += PnlReembolsoDrop_Click;
            }

            if (lblReembolsoDropPrompt != null)
            {
                lblReembolsoDropPrompt.AllowDrop = true;
                lblReembolsoDropPrompt.DragEnter -= PnlReembolsoDrop_DragEnter;
                lblReembolsoDropPrompt.DragEnter += PnlReembolsoDrop_DragEnter;
                lblReembolsoDropPrompt.DragLeave -= PnlReembolsoDrop_DragLeave;
                lblReembolsoDropPrompt.DragLeave += PnlReembolsoDrop_DragLeave;
                lblReembolsoDropPrompt.DragDrop -= PnlReembolsoDrop_DragDrop;
                lblReembolsoDropPrompt.DragDrop += PnlReembolsoDrop_DragDrop;
                lblReembolsoDropPrompt.Click -= PnlReembolsoDrop_Click;
                lblReembolsoDropPrompt.Click += PnlReembolsoDrop_Click;
            }

            dgvReembolsoFiles.AutoGenerateColumns = false;
            dgvReembolsoFiles.AllowUserToAddRows = false;
            dgvReembolsoFiles.RowHeadersVisible = false;
            dgvReembolsoFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReembolsoFiles.MultiSelect = false;
            dgvReembolsoFiles.ReadOnly = false;
            dgvReembolsoFiles.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvReembolsoFiles.Columns.Clear();

            DataGridViewLinkColumn colName = new DataGridViewLinkColumn
            {
                HeaderText = "Archivo",
                DataPropertyName = "FileName",
                Name = "colName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                LinkBehavior = LinkBehavior.HoverUnderline,
                LinkColor = Color.FromArgb(0, 102, 204),
                TrackVisitedState = false,
                ReadOnly = true
            };
            dgvReembolsoFiles.Columns.Add(colName);

            DataGridViewComboBoxColumn colType = new DataGridViewComboBoxColumn
            {
                HeaderText = "Tipo de Documento",
                Name = "colType",
                Width = 220,
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                ReadOnly = false
            };
            colType.Items.AddRange(docTypeOptions);
            dgvReembolsoFiles.Columns.Add(colType);

            DataGridViewTextBoxColumn colSize = new DataGridViewTextBoxColumn
            {
                HeaderText = "Tamaño",
                DataPropertyName = "FileSizeFormatted",
                Name = "colSize",
                Width = 90,
                ReadOnly = true
            };
            dgvReembolsoFiles.Columns.Add(colSize);

            dgvReembolsoFiles.CellContentClick -= DgvReembolsoFiles_CellContentClick;
            dgvReembolsoFiles.CellContentClick += DgvReembolsoFiles_CellContentClick;

            dgvReembolsoFiles.CellValueChanged -= DgvReembolsoFiles_CellValueChanged;
            dgvReembolsoFiles.CellValueChanged += DgvReembolsoFiles_CellValueChanged;

            dgvReembolsoFiles.CurrentCellDirtyStateChanged -= DgvReembolsoFiles_CurrentCellDirtyStateChanged;
            dgvReembolsoFiles.CurrentCellDirtyStateChanged += DgvReembolsoFiles_CurrentCellDirtyStateChanged;

            btnRemoveSelectedFile.Click -= BtnRemoveSelectedFile_Click;
            btnRemoveSelectedFile.Click += BtnRemoveSelectedFile_Click;

            btnClearReembolso.Click -= BtnClearReembolso_Click;
            btnClearReembolso.Click += BtnClearReembolso_Click;

            btnProcessReembolso.Click -= BtnProcessReembolso_Click;
            btnProcessReembolso.Click += BtnProcessReembolso_Click;

            if (btnToggleMode != null)
            {
                btnToggleMode.Click -= BtnToggleMode_Click;
                btnToggleMode.Click += BtnToggleMode_Click;
            }

            if (btnToggleAuth != null)
            {
                btnToggleAuth.Click -= BtnToggleAuth_Click;
                btnToggleAuth.Click += BtnToggleAuth_Click;
            }

            if (cmbCurrency != null)
            {
                cmbCurrency.Items.Clear();
                cmbCurrency.Items.AddRange(new string[] { "DOP", "USD" });
                cmbCurrency.SelectedIndex = 0;
            }

            if (txtReembolsoMonto != null)
            {
                txtReembolsoMonto.KeyPress -= TxtReembolsoMonto_KeyPress;
                txtReembolsoMonto.KeyPress += TxtReembolsoMonto_KeyPress;

                txtReembolsoMonto.LostFocus -= TxtReembolsoMonto_LostFocus;
                txtReembolsoMonto.LostFocus += TxtReembolsoMonto_LostFocus;

                txtReembolsoMonto.KeyDown -= TxtReembolsoMonto_KeyDown;
                txtReembolsoMonto.KeyDown += TxtReembolsoMonto_KeyDown;
            }

            if (lblPassportID != null)
            {
                lblPassportID.Text = "No. Afiliado";
                lblPassportID.Cursor = Cursors.Default;
            }

            if (dtpReminderTime != null)
            {
                dtpReminderTime.Format = DateTimePickerFormat.Custom;
                dtpReminderTime.CustomFormat = "yyyy-MM-dd HH:mm";
                dtpReminderTime.Value = GetDefaultReminderDateTime();
                dtpReminderTime.Enabled = false;
            }

            if (chkSendReminder != null)
            {
                chkSendReminder.CheckedChanged += (s, e) =>
                {
                    if (dtpReminderTime != null)
                    {
                        dtpReminderTime.Enabled = chkSendReminder.Checked;
                        if (chkSendReminder.Checked)
                        {
                            dtpReminderTime.Value = GetDefaultReminderDateTime();
                        }
                    }
                };
            }

            if (dtpReminderTimeComp != null)
            {
                dtpReminderTimeComp.Format = DateTimePickerFormat.Custom;
                dtpReminderTimeComp.CustomFormat = "yyyy-MM-dd HH:mm";
                dtpReminderTimeComp.Value = GetDefaultReminderDateTime();
                dtpReminderTimeComp.Enabled = false;
            }

            if (chkSendReminderComp != null)
            {
                chkSendReminderComp.CheckedChanged += (s, e) =>
                {
                    if (dtpReminderTimeComp != null)
                    {
                        dtpReminderTimeComp.Enabled = chkSendReminderComp.Checked;
                        if (chkSendReminderComp.Checked)
                        {
                            dtpReminderTimeComp.Value = GetDefaultReminderDateTime();
                        }
                    }
                };
            }
        }

        private void BtnToggleAuth_Click(object sender, EventArgs e)
        {
            isAuthMode = !isAuthMode;

            if (isAuthMode)
            {
                if (label7 != null) label7.Text = "Autorizaciones";

                if (isCompletivoMode)
                {
                    isCompletivoMode = false;
                    if (pnlCompletivos != null) pnlCompletivos.Visible = false;
                    if (btnProcessReembolso != null) btnProcessReembolso.Text = "Procesar";
                }

                if (btnToggleMode != null)
                {
                    btnToggleMode.Enabled = false;
                    btnToggleMode.Visible = false;
                }

                if (txtReembolsoMonto != null)
                {
                    txtReembolsoMonto.Text = string.Empty;
                    txtReembolsoMonto.Enabled = false;
                }

                if (cmbCurrency != null)
                {
                    cmbCurrency.SelectedIndex = -1;
                    cmbCurrency.Enabled = false;
                }

                if (btnToggleAuth != null) btnToggleAuth.Text = "Reembolso";
            }
            else
            {
                if (label7 != null) label7.Text = "Reembolsos";

                if (btnToggleMode != null)
                {
                    btnToggleMode.Enabled = true;
                    btnToggleMode.Visible = true;
                    btnToggleMode.Text = "Completivo";
                }

                if (txtReembolsoMonto != null)
                {
                    txtReembolsoMonto.Enabled = true;
                    ResetPlaceholder(txtReembolsoMonto, PlaceholderMonto);
                }

                if (cmbCurrency != null)
                {
                    cmbCurrency.Enabled = true;
                    if (cmbCurrency.Items.Count > 0) cmbCurrency.SelectedIndex = 0;
                }

                if (btnToggleAuth != null) btnToggleAuth.Text = "Autorización";
            }
        }

        private void TxtReembolsoMonto_LostFocus(object sender, EventArgs e)
        {
            EvaluateMontoExpression();
        }

        private void TxtReembolsoMonto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                EvaluateMontoExpression();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TxtReembolsoMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            Control ctrl = sender as Control;
            if (char.IsControl(e.KeyChar)) return;

            if (char.IsDigit(e.KeyChar) || e.KeyChar == '+') return;

            if (e.KeyChar == '.' && ctrl != null)
            {
                string currentText = ctrl.Text;
                int lastPlus = currentText.LastIndexOf('+');
                string currentSegment = lastPlus >= 0 ? currentText.Substring(lastPlus + 1) : currentText;

                if (!currentSegment.Contains(".")) return;
            }

            e.Handled = true;
        }

        private void EvaluateMontoExpression()
        {
            if (txtReembolsoMonto == null) return;

            string raw = GetCleanText(txtReembolsoMonto);
            if (string.IsNullOrWhiteSpace(raw)) return;

            if (raw.Contains("+"))
            {
                string[] terms = raw.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                decimal total = 0m;
                bool hasValidTerm = false;

                foreach (string term in terms)
                {
                    string cleanTerm = term.Trim().Replace(",", ".");
                    if (decimal.TryParse(cleanTerm, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                    {
                        total += val;
                        hasValidTerm = true;
                    }
                }

                if (hasValidTerm)
                {
                    txtReembolsoMonto.Text = total.ToString("F2", CultureInfo.InvariantCulture);
                    txtReembolsoMonto.ForeColor = Color.Black;
                }
            }
            else
            {
                string single = raw.Trim().Replace(",", ".");
                if (decimal.TryParse(single, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                {
                    txtReembolsoMonto.Text = val.ToString("F2", CultureInfo.InvariantCulture);
                    txtReembolsoMonto.ForeColor = Color.Black;
                }
            }
        }

        private void InitializeReembolsosTab()
        {
            LoadClientFolders();

            cmbReembolsoAseguradora.Items.Clear();
            cmbReembolsoAseguradora.Items.AddRange(new string[] {
                "Sura", "Humano", "Universal", "La Colonial", "Mapfre", "Mapfre BHD",
                "SeNaSa", "Cigna", "Bupa", "BMI", "One Alliance", "Aetna",
                "American Fidelity International", "Yunen", "Seguros Pepin",
                "WorldWide Medical", "Primera", "Aurora", "APS", "COOPSEGUROS",
                "Crecer", "La Monumental", "Seguros Reservas", "Otra Aseguradora"
            });

            cmbReembolsoAseguradora.DropDownStyle = ComboBoxStyle.DropDown;
            cmbReembolsoAseguradora.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbReembolsoAseguradora.AutoCompleteSource = AutoCompleteSource.ListItems;
            if (cmbReembolsoAseguradora.Items.Count > 0) cmbReembolsoAseguradora.SelectedIndex = 0;

            if (txtReembolsoPoliza != null)
            {
                txtReembolsoPoliza.DropDownStyle = ComboBoxStyle.DropDown;
                txtReembolsoPoliza.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txtReembolsoPoliza.AutoCompleteSource = AutoCompleteSource.ListItems;

                txtReembolsoPoliza.SelectedIndexChanged += (s, e) =>
                {
                    if (txtReembolsoPoliza.SelectedIndex >= 0) txtReembolsoPoliza.ForeColor = Color.Black;
                    PopulateAffiliatesForPolicy();
                };

                txtReembolsoPoliza.TextChanged += (s, e) =>
                {
                    string val = txtReembolsoPoliza.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(val) && val != PlaceholderPoliza) txtReembolsoPoliza.ForeColor = Color.Black;
                    PopulateAffiliatesForPolicy();
                };
            }

            if (cmbAfiliado != null)
            {
                cmbAfiliado.DropDownStyle = ComboBoxStyle.DropDown;
                cmbAfiliado.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbAfiliado.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbAfiliado.SelectedIndexChanged += (s, e) => AutoFillCedulaFromAffiliate();
                cmbAfiliado.TextChanged += (s, e) => AutoFillCedulaFromAffiliate();
            }

            if (txtReembolsoCedula != null) ApplyPlaceholder(txtReembolsoCedula, PlaceholderCedula);

            txtReembolsoClientSearch.AutoCompleteMode = AutoCompleteMode.None;
            ApplyPlaceholder(txtReembolsoClientSearch, PlaceholderText);

            ApplyPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            ApplyPlaceholder(txtReembolsoMonto, PlaceholderMonto);
            ApplyPlaceholder(txtReembolsosNotasAdicionales, PlaceholderNotas);

            if (txtTargetCaseId != null) ApplyPlaceholder(txtTargetCaseId, PlaceholderTargetCaseId);
            if (txtCompletivoNotas != null) ApplyPlaceholder(txtCompletivoNotas, PlaceholderCompletivoNotas);

            if (lstReembolsoClientSuggestions != null && txtReembolsoClientSearch != null)
            {
                if (lstReembolsoClientSuggestions.Parent != txtReembolsoClientSearch.Parent)
                {
                    txtReembolsoClientSearch.Parent.Controls.Add(lstReembolsoClientSuggestions);
                }
                lstReembolsoClientSuggestions.Left = txtReembolsoClientSearch.Left;
                lstReembolsoClientSuggestions.Top = txtReembolsoClientSearch.Bottom + 1;
                lstReembolsoClientSuggestions.Width = txtReembolsoClientSearch.Width;
                lstReembolsoClientSuggestions.Visible = false;
                lstReembolsoClientSuggestions.BringToFront();
            }

            txtReembolsoClientSearch.LostFocus += (s, e) =>
            {
                Timer focusTimer = new Timer { Interval = 150 };
                focusTimer.Tick += (st, et) => {
                    focusTimer.Stop();
                    if (!lstReembolsoClientSuggestions.Focused && !txtReembolsoClientSearch.Focused)
                    {
                        lstReembolsoClientSuggestions.Visible = false;
                    }
                };
                focusTimer.Start();
            };

            txtReembolsoClientSearch.TextChanged += TxtReembolsoClientSearch_TextChanged;
            txtReembolsoClientSearch.KeyDown += TxtReembolsoClientSearch_KeyDown;
            lstReembolsoClientSuggestions.Click += LstReembolsoClientSuggestions_Click;
            lstReembolsoClientSuggestions.KeyDown += LstReembolsoClientSuggestions_KeyDown;

            txtReembolsoClientSearch.TextChanged += (s, e) => PopulatePoliciesForClient();

            if (pnlCompletivos != null) pnlCompletivos.Visible = false;
            if (btnProcessReembolso != null) btnProcessReembolso.BringToFront();
        }

        private void PopulatePoliciesForClient()
        {
            if (txtReembolsoPoliza == null) return;

            string client = GetCleanText(txtReembolsoClientSearch);
            txtReembolsoPoliza.Items.Clear();

            if (string.IsNullOrWhiteSpace(client) || !Directory.Exists(clientRootPath)) return;

            string clientPath = Path.Combine(clientRootPath, client);
            if (!Directory.Exists(clientPath)) return;

            var policies = Directory.GetDirectories(clientPath)
                .Select(d => new DirectoryInfo(d).Name)
                .ToArray();

            txtReembolsoPoliza.Items.AddRange(policies);

            if (policies.Length > 0 && string.IsNullOrWhiteSpace(GetCleanText(txtReembolsoPoliza)))
            {
                txtReembolsoPoliza.SelectedIndex = 0;
                txtReembolsoPoliza.ForeColor = Color.Black;
            }
            else
            {
                PopulateAffiliatesForPolicy();
            }
        }

        private void PopulateAffiliatesForPolicy()
        {
            if (cmbAfiliado == null) return;

            string client = GetCleanText(txtReembolsoClientSearch);
            string policy = GetCleanText(txtReembolsoPoliza);

            cmbAfiliado.Items.Clear();

            if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(policy) || !Directory.Exists(clientRootPath)) return;

            string policyPath = Path.Combine(clientRootPath, client, policy);
            if (!Directory.Exists(policyPath)) return;

            var affiliates = Directory.GetDirectories(policyPath)
                .Select(d => new DirectoryInfo(d).Name)
                .ToArray();

            cmbAfiliado.Items.AddRange(affiliates);

            if (cmbAfiliado.Items.Count > 0)
            {
                cmbAfiliado.SelectedIndex = 0;
            }
            else
            {
                AutoFillCedulaFromAffiliate();
            }
        }

        private void AutoFillCedulaFromAffiliate()
        {
            if (txtReembolsoCedula == null) return;

            string client = GetCleanText(txtReembolsoClientSearch);
            string policy = GetCleanText(txtReembolsoPoliza);
            string affiliate = GetCleanText(cmbAfiliado);

            if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(policy) || string.IsNullOrWhiteSpace(affiliate)) return;

            string affiliatePath = Path.Combine(clientRootPath, client, policy, affiliate);
            if (!Directory.Exists(affiliatePath)) return;

            var idFile = Directory.GetFiles(affiliatePath, "ID_*.txt").FirstOrDefault();
            if (idFile != null)
            {
                string rawName = Path.GetFileNameWithoutExtension(idFile);
                if (rawName.Length > 3)
                {
                    string idNumber = rawName.Substring(3).Trim();
                    if (!string.IsNullOrWhiteSpace(idNumber))
                    {
                        txtReembolsoCedula.Text = idNumber;
                        txtReembolsoCedula.ForeColor = Color.Black;
                        return;
                    }
                }
            }

            ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
        }

        private void BtnToggleMode_Click(object sender, EventArgs e)
        {
            if (isAuthMode) return;

            isCompletivoMode = !isCompletivoMode;

            if (isCompletivoMode)
            {
                if (pnlCompletivos != null)
                {
                    pnlCompletivos.Visible = true;
                    pnlCompletivos.BringToFront();
                }

                if (btnProcessReembolso != null)
                {
                    btnProcessReembolso.Text = "Procesar Completivo";
                    btnProcessReembolso.BringToFront();
                }

                if (btnToggleMode != null) btnToggleMode.Text = "Reembolso";
            }
            else
            {
                if (pnlCompletivos != null) pnlCompletivos.Visible = false;

                if (btnProcessReembolso != null)
                {
                    btnProcessReembolso.Text = "Procesar";
                    btnProcessReembolso.BringToFront();
                }

                if (btnToggleMode != null) btnToggleMode.Text = "Completivo";
            }
        }

        private string GetConfiguredAppUser()
        {
            string settingUser = Properties.Settings.Default.SelectedAppUser?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(settingUser) && settingUser != PlaceholderAppUser)
            {
                return settingUser;
            }

            if (txtAppUser != null)
            {
                string textVal = GetCleanText(txtAppUser);
                if (!string.IsNullOrWhiteSpace(textVal)) return textVal;
            }

            return string.Empty;
        }

        private string GetUserInitials(string appUser)
        {
            if (string.IsNullOrWhiteSpace(appUser)) return "XX";

            var parts = appUser.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();
            }

            return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
        }

        private string GenerateCaseId(string userInitials, string modePrefix)
        {
            string initials = string.IsNullOrWhiteSpace(userInitials) ? "XX" : userInitials.Trim().ToUpper();

            byte[] bytes = new byte[6];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            string randPart = BitConverter.ToString(bytes).Replace("-", "");
            return $"{modePrefix}-{initials}-{randPart}";
        }

        private void PnlReembolsoDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                pnlReembolsoDrop.BackColor = HoverDropzoneColor;
            }
        }

        private void PnlReembolsoDrop_DragLeave(object sender, EventArgs e)
        {
            pnlReembolsoDrop.BackColor = NormalDropzoneColor;
        }

        private void PnlReembolsoDrop_DragDrop(object sender, DragEventArgs e)
        {
            pnlReembolsoDrop.BackColor = NormalDropzoneColor;
            string[] dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dropped == null || dropped.Length == 0) return;

            AddFilesToReembolsoQueue(dropped);
        }

        private void PnlReembolsoDrop_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Documentos e Imágenes (*.pdf;*.png;*.jpg;*.jpeg;*.bmp)|*.pdf;*.png;*.jpg;*.jpeg;*.bmp|Todos los Archivos (*.*)|*.*";
                ofd.Multiselect = true;
                ofd.Title = "Seleccionar documentos para Reembolso / Autorización";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    AddFilesToReembolsoQueue(ofd.FileNames);
                }
            }
        }

        private void AddFilesToReembolsoQueue(string[] filePaths)
        {
            int addedCount = 0;
            string[] allowedExts = { ".pdf", ".png", ".jpg", ".jpeg", ".bmp" };

            foreach (string path in filePaths)
            {
                if (File.Exists(path))
                {
                    string ext = Path.GetExtension(path).ToLower();
                    if (allowedExts.Contains(ext))
                    {
                        if (!reembolsoItems.Any(x => x.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                        {
                            string suggestedType = GuessDocTypeFromName(Path.GetFileName(path));

                            var newItem = new ReembolsoFileItem
                            {
                                FullPath = path,
                                DocumentType = suggestedType
                            };

                            reembolsoItems.Add(newItem);
                            addedCount++;
                        }
                    }
                }
            }

            if (addedCount > 0)
            {
                RefreshReembolsosGrid();
            }
        }

        private string GuessDocTypeFromName(string fileName)
        {
            string lower = fileName.ToLower();
            if (lower.Contains("fact") || lower.Contains("inv")) return "Factura";
            if (lower.Contains("recib") || lower.Contains("pago")) return "Recibo de Pago Sellado";
            if (lower.Contains("indica") || lower.Contains("receta")) return "Indicación Médica";
            if (lower.Contains("hist") || lower.Contains("clinic")) return "Historia Clínica";
            if (lower.Contains("estudio") || lower.Contains("lab") || lower.Contains("analis")) return "Resultados de Estudios";
            if (lower.Contains("ced") || lower.Contains("id")) return "Cédula / Documento de Identidad";
            if (lower.Contains("form") || lower.Contains("solicitud")) return "Formulario de Solicitud";
            return "Otros";
        }

        private void RefreshReembolsosGrid()
        {
            dgvReembolsoFiles.Rows.Clear();
            foreach (var item in reembolsoItems)
            {
                int rowIndex = dgvReembolsoFiles.Rows.Add();
                DataGridViewRow row = dgvReembolsoFiles.Rows[rowIndex];

                row.Cells["colName"].Value = item.FileName;
                row.Cells["colType"].Value = item.DocumentType;
                row.Cells["colSize"].Value = item.FileSizeFormatted;
                row.Tag = item;
            }

            if (lblReembolsoDropPrompt != null)
            {
                lblReembolsoDropPrompt.Text = reembolsoItems.Count > 0
                    ? $"{reembolsoItems.Count} documento(s) listos para consolidar."
                    : "Arrastra todos los archivos PDF del caso aquí";
            }
        }

        private void DgvReembolsoFiles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReembolsoFiles.Columns["colName"].Index)
            {
                var item = dgvReembolsoFiles.Rows[e.RowIndex].Tag as ReembolsoFileItem;
                if (item != null && File.Exists(item.FullPath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = item.FullPath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"No se pudo abrir el documento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DgvReembolsoFiles_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvReembolsoFiles.IsCurrentCellDirty && dgvReembolsoFiles.CurrentCell is DataGridViewComboBoxCell)
            {
                dgvReembolsoFiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvReembolsoFiles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReembolsoFiles.Columns["colType"].Index)
            {
                var row = dgvReembolsoFiles.Rows[e.RowIndex];
                var item = row.Tag as ReembolsoFileItem;
                if (item != null)
                {
                    item.DocumentType = row.Cells["colType"].Value?.ToString() ?? "Otros";
                }
            }
        }

        private void BtnRemoveSelectedFile_Click(object sender, EventArgs e)
        {
            if (dgvReembolsoFiles.CurrentRow == null || dgvReembolsoFiles.CurrentRow.Index < 0)
            {
                MessageBox.Show("Seleccione una fila para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = dgvReembolsoFiles.CurrentRow.Tag as ReembolsoFileItem;
            if (selectedItem != null)
            {
                reembolsoItems.Remove(selectedItem);
                dgvReembolsoFiles.ClearSelection();
                RefreshReembolsosGrid();
            }
        }

        private void BtnClearReembolso_Click(object sender, EventArgs e)
        {
            if (reembolsoItems.Count == 0 && string.IsNullOrWhiteSpace(GetCleanText(txtReembolsoPoliza))) return;

            DialogResult confirm = MessageBox.Show(
                "¿Estás seguro de que deseas limpiar la lista de documentos y todos los campos?",
                "Confirmar limpieza",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                ClearReembolsosForm();
            }
        }

        private void ClearReembolsosForm()
        {
            reembolsoItems.Clear();
            dgvReembolsoFiles.Rows.Clear();

            ResetPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
            ResetPlaceholder(txtReembolsoMonto, PlaceholderMonto);
            ResetPlaceholder(txtReembolsosNotasAdicionales, PlaceholderNotas);
            ResetPlaceholder(txtReembolsoClientSearch, PlaceholderText);

            if (txtReembolsoPoliza != null) txtReembolsoPoliza.Items.Clear();

            if (cmbAfiliado != null)
            {
                cmbAfiliado.Items.Clear();
                cmbAfiliado.Text = string.Empty;
            }

            if (txtTargetCaseId != null) ResetPlaceholder(txtTargetCaseId, PlaceholderTargetCaseId);
            if (txtCompletivoNotas != null) ResetPlaceholder(txtCompletivoNotas, PlaceholderCompletivoNotas);

            if (cmbReembolsoAseguradora != null && cmbReembolsoAseguradora.Items.Count > 0)
            {
                cmbReembolsoAseguradora.SelectedIndex = 0;
            }

            if (cmbCurrency != null && cmbCurrency.Items.Count > 0)
            {
                cmbCurrency.SelectedIndex = 0;
            }

            if (lblPassportID != null) lblPassportID.Text = "No. Afiliado";

            if (lblReembolsoDropPrompt != null)
            {
                lblReembolsoDropPrompt.Text = "Arrastra todos los archivos PDF del caso aquí";
            }

            if (chkSendReminder != null) chkSendReminder.Checked = false;
            if (dtpReminderTime != null)
            {
                dtpReminderTime.Value = GetDefaultReminderDateTime();
                dtpReminderTime.Enabled = false;
            }

            if (chkSendReminderComp != null) chkSendReminderComp.Checked = false;
            if (dtpReminderTimeComp != null)
            {
                dtpReminderTimeComp.Value = GetDefaultReminderDateTime();
                dtpReminderTimeComp.Enabled = false;
            }

            this.ActiveControl = null;
        }

        private void TxtReembolsoClientSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtReembolsoClientSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(query) || query == PlaceholderText)
            {
                lstReembolsoClientSuggestions.Visible = false;
                return;
            }

            var matches = cachedClientList
                .Where(c => c.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(c => c.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .ThenBy(c => c)
                .Take(8)
                .ToList();

            if (matches.Count > 0)
            {
                lstReembolsoClientSuggestions.DataSource = matches;
                lstReembolsoClientSuggestions.Visible = true;
                lstReembolsoClientSuggestions.BringToFront();
            }
            else
            {
                lstReembolsoClientSuggestions.Visible = false;
            }
        }

        private void TxtReembolsoClientSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstReembolsoClientSuggestions.Visible && lstReembolsoClientSuggestions.Items.Count > 0)
            {
                lstReembolsoClientSuggestions.Focus();
                lstReembolsoClientSuggestions.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && lstReembolsoClientSuggestions.Visible && lstReembolsoClientSuggestions.Items.Count > 0)
            {
                SelectClientFromReembolsoList();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LstReembolsoClientSuggestions_Click(object sender, EventArgs e) => SelectClientFromReembolsoList();

        private void LstReembolsoClientSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectClientFromReembolsoList();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                lstReembolsoClientSuggestions.Visible = false;
                txtReembolsoClientSearch.Focus();
            }
        }

        private void SelectClientFromReembolsoList()
        {
            if (lstReembolsoClientSuggestions.SelectedItem != null)
            {
                txtReembolsoClientSearch.Text = lstReembolsoClientSuggestions.SelectedItem.ToString();
                txtReembolsoClientSearch.ForeColor = Color.Black;
                txtReembolsoClientSearch.SelectionStart = txtReembolsoClientSearch.Text.Length;
                lstReembolsoClientSuggestions.Visible = false;
                txtReembolsoClientSearch.Focus();
                PopulatePoliciesForClient();
            }
        }

        // Validación estricta y pre-vuelo antes de consolidar
        private bool ValidateItemsForMerge(out string validationErrorMessage)
        {
            validationErrorMessage = string.Empty;
            string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

            foreach (var item in reembolsoItems)
            {
                if (!File.Exists(item.FullPath))
                {
                    validationErrorMessage = $"El archivo ya no existe en el disco:\n{item.FileName}";
                    return false;
                }

                FileInfo fi = new FileInfo(item.FullPath);
                if (fi.Length == 0)
                {
                    validationErrorMessage = $"El archivo está vacío (0 bytes) y no se puede procesar:\n{item.FileName}";
                    return false;
                }

                string ext = fi.Extension.ToLower();

                if (ext == ".pdf")
                {
                    try
                    {
                        using (PdfSharp.Pdf.PdfDocument probe = PdfReader.Open(item.FullPath, PdfDocumentOpenMode.Import))
                        {
                            if (probe.PageCount == 0)
                            {
                                validationErrorMessage = $"El archivo PDF no contiene páginas válidas:\n{item.FileName}";
                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        validationErrorMessage = $"Hay archivos dañados o protegidos contra lectura:\n{item.FileName}";
                        return false;
                    }
                }
                else if (imageExtensions.Contains(ext))
                {
                    try
                    {
                        using (Image probeImg = Image.FromFile(item.FullPath))
                        {
                            if (probeImg.Width == 0 || probeImg.Height == 0)
                            {
                                validationErrorMessage = $"Hay imagenes con dimensiones inválidas:\n{item.FileName}";
                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        validationErrorMessage = $"Hay archivos de imagen que no se pudieron leer o están dañados:\n{item.FileName}\n\nDetalle: {ex.Message}";
                        return false;
                    }
                }
            }

            return true;
        }

        // Ejecución Principal: Pre-Flight Check + Atomic Merge + Metadata + Trigger
        private void BtnProcessReembolso_Click(object sender, EventArgs e)
        {
            string appUser = GetConfiguredAppUser();
            if (string.IsNullOrWhiteSpace(appUser))
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Debe definir y configurar un Usuario (AppUser) en la pestaña de Opciones antes de procesar cualquier caso.", "Usuario Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (tabControlMain != null && tabPageOptions != null)
                {
                    tabControlMain.SelectedTab = tabPageOptions;
                }
                txtAppUser?.Focus();
                return;
            }

            if (reembolsoItems.Count == 0)
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Debe agregar al menos un documento a la lista.", "Faltan Archivos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Verificación previa de integridad de archivos
            if (!ValidateItemsForMerge(out string validationError))
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show(validationError, "Archivo No Válido o Dañado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string userInitials = GetUserInitials(appUser);
            string finalPdfName;
            string finalJsonName;
            object plannerPayload;
            string caseId;

            if (isCompletivoMode)
            {
                string targetCaseId = GetCleanText(txtTargetCaseId);
                string completivoNotes = GetCleanText(txtCompletivoNotas);

                if (string.IsNullOrWhiteSpace(targetCaseId))
                {
                    PlaySound(SystemSounds.Exclamation);
                    MessageBox.Show("Debe ingresar el ID Único del caso original al cual asociar este completivo.", "ID de Caso Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTargetCaseId?.Focus();
                    return;
                }

                string formattedDate = DateTime.Now.ToString("ddMMMyy", new CultureInfo("es-ES"));
                string safeTargetId = string.Concat(targetCaseId.Split(Path.GetInvalidFileNameChars())).Replace(" ", "");
                caseId = GenerateCaseId(userInitials, "CMP");

                finalPdfName = $"CMP_{safeTargetId}_{formattedDate}.pdf";
                finalJsonName = $"CMP_{safeTargetId}_{formattedDate}.json";

                List<string> presentDocTypes = reembolsoItems.Select(x => x.DocumentType).Distinct().ToList();

                string compReminderString = (chkSendReminderComp != null && chkSendReminderComp.Checked && dtpReminderTimeComp != null)
                    ? dtpReminderTimeComp.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : null;

                plannerPayload = new
                {
                    CaseType = "CMP",
                    CaseId = caseId,
                    TargetCaseId = targetCaseId,
                    DocumentTypes = presentDocTypes,
                    SourceFilesCount = reembolsoItems.Count,
                    AdditionalNotes = string.IsNullOrWhiteSpace(completivoNotes) ? "Sin notas adicionales." : completivoNotes,
                    ReminderDate = compReminderString,
                    GeneratedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    PdfFileName = finalPdfName
                };
            }
            else
            {
                string clientName = GetCleanText(txtReembolsoClientSearch);
                if (string.IsNullOrWhiteSpace(clientName))
                {
                    PlaySound(SystemSounds.Exclamation);
                    MessageBox.Show("Por favor indique o seleccione un cliente válido.", "Cliente Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtReembolsoClientSearch.Focus();
                    return;
                }

                string policyNumber = GetCleanText(txtReembolsoPoliza);
                string affiliate = GetCleanText(cmbAfiliado);
                string affiliateIdNumber = GetCleanText(txtReembolsoCedula);
                string insurer = cmbReembolsoAseguradora.SelectedItem?.ToString() ?? "Aseguradora";
                string rawAmount = GetCleanText(txtReembolsoMonto);
                string currency = cmbCurrency?.SelectedItem?.ToString() ?? "DOP";
                string notes = GetCleanText(txtReembolsosNotasAdicionales);

                if (string.IsNullOrWhiteSpace(policyNumber))
                {
                    PlaySound(SystemSounds.Exclamation);
                    MessageBox.Show("Por favor introduzca el número de póliza del cliente.", "Póliza Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtReembolsoPoliza.Focus();
                    return;
                }

                decimal numericAmount = 0.00m;
                if (!string.IsNullOrWhiteSpace(rawAmount))
                {
                    decimal.TryParse(rawAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out numericAmount);
                }

                EnsureDirectoryHierarchy(clientName, policyNumber, affiliate, affiliateIdNumber);

                string modePrefix = isAuthMode ? "AUT" : "REE";
                caseId = GenerateCaseId(userInitials, modePrefix);

                finalPdfName = $"{caseId}.pdf";
                finalJsonName = $"{caseId}.json";

                List<string> presentDocTypes = reembolsoItems.Select(x => x.DocumentType).Distinct().ToList();

                string reminderString = (chkSendReminder != null && chkSendReminder.Checked && dtpReminderTime != null)
                    ? dtpReminderTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : null;

                plannerPayload = new
                {
                    CaseType = modePrefix,
                    CaseId = caseId,
                    ClientName = clientName,
                    PolicyNumber = policyNumber,
                    Afiliado = string.IsNullOrWhiteSpace(affiliate) ? clientName : affiliate,
                    Insurer = insurer,
                    IdDocumentType = "No. Afiliado",
                    CedulaOrPassport = string.IsNullOrWhiteSpace(affiliateIdNumber) ? "No indicado" : affiliateIdNumber,
                    ClaimAmount = numericAmount,
                    Currency = currency,
                    AdditionalNotes = string.IsNullOrWhiteSpace(notes) ? "Sin notas adicionales." : notes,
                    ReminderDate = reminderString,
                    DocumentTypes = presentDocTypes,
                    SourceFilesCount = reembolsoItems.Count,
                    GeneratedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    PdfFileName = finalPdfName
                };
            }

            // Usar una ruta temporal aislada para construir el PDF
            // Esto evita que archivos corruptos o incompletos toquen la carpeta DROPZONE_FLOW
            string stagingPdfTempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tmp");
            string finalPdfLocalPath = Path.Combine(dropzoneFlowPath, finalPdfName);
            string finalJsonLocalPath = Path.Combine(dropzoneFlowPath, finalJsonName);

            try
            {
                if (!Directory.Exists(dropzoneFlowPath))
                {
                    Directory.CreateDirectory(dropzoneFlowPath);
                }

                // 2. Compilación en área de preparación local
                using (PdfSharp.Pdf.PdfDocument outputDoc = new PdfSharp.Pdf.PdfDocument())
                {
                    string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

                    foreach (var item in reembolsoItems)
                    {
                        string ext = Path.GetExtension(item.FullPath).ToLower();

                        if (ext == ".pdf")
                        {
                            using (PdfSharp.Pdf.PdfDocument inputDoc = PdfReader.Open(item.FullPath, PdfDocumentOpenMode.Import))
                            {
                                int count = inputDoc.PageCount;
                                for (int idx = 0; idx < count; idx++)
                                {
                                    PdfSharp.Pdf.PdfPage page = inputDoc.Pages[idx];
                                    outputDoc.AddPage(page);
                                }
                            }
                        }
                        else if (imageExtensions.Contains(ext))
                        {
                            byte[] imageBytes = File.ReadAllBytes(item.FullPath);
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            using (XImage img = XImage.FromStream(ms))
                            {
                                PdfSharp.Pdf.PdfPage page = outputDoc.AddPage();
                                page.Width = XUnit.FromPoint(img.PointWidth);
                                page.Height = XUnit.FromPoint(img.PointHeight);

                                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                                {
                                    gfx.DrawImage(img, 0, 0, page.Width.Point, page.Height.Point);
                                }
                            }
                        }
                    }

                    if (outputDoc.PageCount == 0)
                    {
                        throw new InvalidOperationException("El documento consolidado no contiene páginas.");
                    }

                    // Asegurar soporte de renderizado visual para campos editables
                    try
                    {
                        if (outputDoc.AcroForm != null)
                        {
                            outputDoc.AcroForm.Elements.SetBoolean("/NeedAppearances", true);
                        }
                        else
                        {
                            if (!outputDoc.Internals.Catalog.Elements.ContainsKey("/AcroForm"))
                            {
                                outputDoc.Internals.Catalog.Elements.Add("/AcroForm", new PdfDictionary(outputDoc));
                            }
                            PdfDictionary acroForm = outputDoc.Internals.Catalog.Elements.GetDictionary("/AcroForm");
                            acroForm.Elements.SetBoolean("/NeedAppearances", true);
                        }
                    }
                    catch (Exception exAcro)
                    {
                        Debug.WriteLine($"Aviso AcroForm: {exAcro.Message}");
                    }

                    outputDoc.Save(stagingPdfTempPath);
                }

                // 3. Promoción atómica a DROPZONE_FLOW
                // Solo si el PDF se guardó exitosamente y sin excepciones, movemos el PDF y escribimos el JSON
                if (File.Exists(finalPdfLocalPath)) File.Delete(finalPdfLocalPath);
                File.Move(stagingPdfTempPath, finalPdfLocalPath);

                string jsonContent = JsonSerializer.Serialize(plannerPayload, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(finalJsonLocalPath, jsonContent);

                // 4. Eliminación de archivos originales
                foreach (var item in reembolsoItems)
                {
                    try
                    {
                        if (File.Exists(item.FullPath)) File.Delete(item.FullPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"No se pudo borrar {item.FullPath}: {ex.Message}");
                    }
                }

                string modeLabel = isCompletivoMode ? "Completivo" : (isAuthMode ? "Autorización" : "Expediente");

                PlaySound(SystemSounds.Asterisk);
                MessageBox.Show(
                    $"{modeLabel} generado exitosamente:\n\n" +
                    $"ID de Caso: {caseId}\n" +
                    $"PDF: {finalPdfName}\n\n" +
                    $"Microsoft Lists será actualizado dentro de 2 minutos.",
                    "Operación Completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                LoadClientFolders();
                ClearReembolsosForm();
            }
            catch (Exception ex)
            {
                // Limpieza de staging en caso de fallo intermedio
                try
                {
                    if (File.Exists(stagingPdfTempPath)) File.Delete(stagingPdfTempPath);
                    if (File.Exists(finalPdfLocalPath)) File.Delete(finalPdfLocalPath);
                    if (File.Exists(finalJsonLocalPath)) File.Delete(finalJsonLocalPath);
                }
                catch { }

                PlaySound(SystemSounds.Hand);
                MessageBox.Show($"Ocurrió un error al procesar el caso:\n{ex.Message}\n\nNo se subió ningún archivo corrupto a la carpeta Dropzone.", "Error de Procesamiento", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureDirectoryHierarchy(string client, string policy, string affiliate, string affiliateIdNumber = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(client) || !Directory.Exists(clientRootPath)) return;

                string clientDir = Path.Combine(clientRootPath, client);
                if (!Directory.Exists(clientDir)) Directory.CreateDirectory(clientDir);

                if (!string.IsNullOrWhiteSpace(policy))
                {
                    string policyDir = Path.Combine(clientDir, policy);
                    if (!Directory.Exists(policyDir)) Directory.CreateDirectory(policyDir);

                    if (!string.IsNullOrWhiteSpace(affiliate))
                    {
                        string affiliateDir = Path.Combine(policyDir, affiliate);
                        if (!Directory.Exists(affiliateDir)) Directory.CreateDirectory(affiliateDir);

                        if (!string.IsNullOrWhiteSpace(affiliateIdNumber) && affiliateIdNumber != PlaceholderCedula)
                        {
                            string cleanDoc = string.Concat(affiliateIdNumber.Split(Path.GetInvalidFileNameChars())).Trim();
                            string targetIdFilePath = Path.Combine(affiliateDir, $"ID_{cleanDoc}.txt");

                            var oldFiles = Directory.GetFiles(affiliateDir, "ID_*.txt");
                            foreach (var old in oldFiles)
                            {
                                if (!old.Equals(targetIdFilePath, StringComparison.OrdinalIgnoreCase))
                                {
                                    try { File.Delete(old); } catch { }
                                }
                            }

                            if (!File.Exists(targetIdFilePath))
                            {
                                File.WriteAllText(targetIdFilePath, affiliateIdNumber);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creando jerarquía de directorios: {ex.Message}");
            }
        }

        #endregion

        #region Configuración y Guardado

        private void SetupSettingsTab()
        {
            if (btnBrowseReembolsoRoot != null)
            {
                btnBrowseReembolsoRoot.Click += btnBrowseReembolsoRoot_Click;
            }

            if (txtAppUser != null)
            {
                ApplyPlaceholder(txtAppUser, PlaceholderAppUser);
                string savedUser = Properties.Settings.Default.SelectedAppUser;
                if (!string.IsNullOrWhiteSpace(savedUser))
                {
                    txtAppUser.Text = savedUser;
                    txtAppUser.ForeColor = Color.Black;
                }
            }
        }

        private void btnBrowseRoot_Click_1(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccione la carpeta raíz de Clientes";
                folderDialog.SelectedPath = Directory.Exists(clientRootPath) ? clientRootPath : @"C:\";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtRootPath.Text = folderDialog.SelectedPath.Replace('/', '\\');
                }
            }
        }

        private void btnBrowseReembolsoRoot_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccione la carpeta DROPZONE receptora para los flujos de Reembolso";
                folderDialog.SelectedPath = Directory.Exists(dropzoneFlowPath) ? dropzoneFlowPath : @"C:\";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtReembolsoRootPath.Text = folderDialog.SelectedPath.Replace('/', '\\');
                }
            }
        }

        private void btnSaveSettings_Click_1(object sender, EventArgs e)
        {
            string newClientPath = txtRootPath.Text.Trim().Replace('/', '\\');
            string newReembolsoPath = txtReembolsoRootPath != null ? txtReembolsoRootPath.Text.Trim().Replace('/', '\\') : dropzoneFlowPath;

            if (!ValidateOrCreateDirectory(ref newClientPath, "Clientes")) return;
            if (!ValidateOrCreateDirectory(ref newReembolsoPath, "Reembolsos Flow")) return;

            clientRootPath = newClientPath;
            dropzoneFlowPath = newReembolsoPath;

            try
            {
                Properties.Settings.Default.ClientRootPath = clientRootPath;
                Properties.Settings.Default.ReembolsoRootPath = dropzoneFlowPath;

                if (txtAppUser != null)
                {
                    string user = GetCleanText(txtAppUser);
                    Properties.Settings.Default.SelectedAppUser = user;
                }

                if (chkMuteSound != null)
                {
                    Properties.Settings.Default.IsMuted = chkMuteSound.Checked;
                }

                if (chkOpenOnStartup != null)
                {
                    bool enableStartup = chkOpenOnStartup.Checked;
                    Properties.Settings.Default.OpenOnStartup = enableStartup;
                    SetStartupRegistry(enableStartup);
                }

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al guardar configuración: {ex.Message}");
            }

            LoadClientFolders();
            MessageBox.Show("Configuración guardada correctamente.", "Opciones", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (tabControlMain != null && tabPageReembolsos != null)
            {
                tabControlMain.SelectedTab = tabPageReembolsos;
            }
        }

        private bool ValidateOrCreateDirectory(ref string path, string friendlyName)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            if (!Directory.Exists(path))
            {
                var result = MessageBox.Show(
                    $"La carpeta especificada para [{friendlyName}] no existe en este equipo:\n{path}\n\n¿Desea crearla ahora?",
                    "Confirmar Ruta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"No se pudo crear la carpeta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!path.EndsWith(@"\")) path += @"\";
            return true;
        }

        #endregion
    }
}