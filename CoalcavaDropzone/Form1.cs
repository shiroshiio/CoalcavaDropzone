using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;

namespace CoalcavaDropzone
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool AnimateWindow(IntPtr hwnd, int timeMs, int flags);

        private const int AW_ACTIVATE = 0x20000;
        private const int AW_BLEND = 0x80000;

        // Rutas y configuración general
        private string clientRootPath = @"C:\Users\Public\OneDrive - COALCAVA\Clientes\";
        private string dropzoneFlowPath = @"C:\Users\Public\OneDrive - COALCAVA\Recepcion Reembolsos\";

        // SharePoint Configuration
        private readonly string tenantId = "47d4e187-f644-4ecf-8b7d-e4456388865f";
        private readonly string clientId = "ce2d41ed-73df-43da-ae41-fbfa9914df9e";
        private readonly string sharePointSiteId = "coalcava.sharepoint.com,96ae7750-2f9b-4430-895d-461cac1fb449,6a0d36fb-6fab-487d-9353-71e43239c7ba";

        private List<string> cachedClientList = new List<string>();

        private InteractiveBrowserCredential sharedCredential;
        private GraphServiceClient sharedGraphClient;

        private GraphServiceClient GetGraphClient()
        {
            if (sharedGraphClient == null)
            {
                sharedCredential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
                {
                    TenantId = tenantId,
                    ClientId = clientId
                });
                sharedGraphClient = new GraphServiceClient(sharedCredential, new[] { "Sites.ReadWrite.All", "Tasks.ReadWrite" });
            }
            return sharedGraphClient;
        }

        // Placeholders
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

        // Estado del modo
        private bool isCompletivoMode = false;
        private bool isAuthMode = false;

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
            try { AnimateWindow(this.Handle, 250, AW_BLEND | AW_ACTIVATE); } catch { }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;

            try
            {
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ClientRootPath))
                    clientRootPath = Properties.Settings.Default.ClientRootPath;

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ReembolsoRootPath))
                    dropzoneFlowPath = Properties.Settings.Default.ReembolsoRootPath;
            }
            catch { }

            clientRootPath = clientRootPath.Replace('/', '\\');
            if (!clientRootPath.EndsWith(@"\")) clientRootPath += @"\";

            dropzoneFlowPath = dropzoneFlowPath.Replace('/', '\\');
            if (!dropzoneFlowPath.EndsWith(@"\")) dropzoneFlowPath += @"\";

            if (txtRootPath != null) txtRootPath.Text = clientRootPath;
            if (txtReembolsoRootPath != null) txtReembolsoRootPath.Text = dropzoneFlowPath;

            InitializeReembolsosTab();
            SetupSettingsTab();

            await CheckSilentSignInAsync();
        }

        private async Task CheckSilentSignInAsync()
        {
            bool signedIn = await AuthService.TrySilentSignInAsync();
            UpdateAuthUI();

            if (signedIn && !string.IsNullOrWhiteSpace(AuthService.CurrentDisplayName))
            {
                if (txtAppUser != null && string.IsNullOrWhiteSpace(GetCleanText(txtAppUser)))
                {
                    txtAppUser.Text = AuthService.CurrentDisplayName;
                    txtAppUser.ForeColor = Color.Black;
                }
            }
        }

        private void UpdateAuthUI()
        {
            if (btnAuthAction != null)
                btnAuthAction.Text = AuthService.IsSignedIn ? "Cerrar Sesión" : "Iniciar Sesión";

            if (lblAuthStatus != null)
            {
                lblAuthStatus.Text = AuthService.IsSignedIn
                    ? $"Sesión activa: {AuthService.CurrentDisplayName} ({AuthService.CurrentUserEmail})"
                    : "No hay sesión iniciada de Microsoft.";
                lblAuthStatus.ForeColor = AuthService.IsSignedIn ? Color.DarkGreen : Color.DarkRed;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList().Cast<string>().ToArray();
                    if (files.Length > 0) AddFilesToReembolsoQueue(files);
                }
            }
        }

        #region Microsoft Graph SharePoint Upload & Reminders

        private async Task<ListItem> FindListItemByCaseIdAsync(GraphServiceClient graphClient, string listId, string targetCaseId)
        {
            var items = await graphClient.Sites[sharePointSiteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = new[] { "fields" };
            });

            if (items?.Value != null)
            {
                foreach (var item in items.Value)
                {
                    if (item.Fields != null && item.Fields.AdditionalData != null)
                    {
                        // Check all possible internal name variants for ID de Caso / No. Caso
                        string[] possibleFields = { "IDdeCaso", "ID_x0020_de_x0020_Caso", "No_x002e_Caso", "NoCaso" };

                        foreach (var field in possibleFields)
                        {
                            if (item.Fields.AdditionalData.TryGetValue(field, out var val) &&
                                string.Equals(val?.ToString()?.Trim(), targetCaseId.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private async Task CreateToDoReminderTaskAsync(GraphServiceClient graphClient, string taskTitle, string taskBody, DateTime reminderDateTime)
        {
            try
            {
                var todoLists = await graphClient.Me.Todo.Lists.GetAsync();
                var defaultList = todoLists?.Value?.FirstOrDefault();

                if (defaultList == null) return;

                var reminderTimeZone = TimeZoneInfo.Local.StandardName;

                var newTask = new TodoTask
                {
                    Title = taskTitle,
                    Body = new ItemBody
                    {
                        Content = taskBody,
                        ContentType = BodyType.Text
                    },
                    DueDateTime = new DateTimeTimeZone
                    {
                        DateTime = reminderDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        TimeZone = reminderTimeZone
                    },
                    ReminderDateTime = new DateTimeTimeZone
                    {
                        DateTime = reminderDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        TimeZone = reminderTimeZone
                    },
                    IsReminderOn = true
                };

                await graphClient.Me.Todo.Lists[defaultList.Id].Tasks.PostAsync(newTask);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al crear recordatorio en Microsoft To Do: {ex.Message}");
            }
        }

        private async Task UploadToSharePointAsync(string listName, Dictionary<string, object> itemFields, string pdfPath, string finalFileName, bool isCompletivo, string targetCaseId)
        {
            var graphClient = GetGraphClient();

            // 1. Resolve the List ID directly using the composite Site ID
            var lists = await graphClient.Sites[sharePointSiteId].Lists.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"displayName eq '{listName}'";
            });
            var listId = lists?.Value?.FirstOrDefault()?.Id;

            if (string.IsNullOrEmpty(listId))
                throw new Exception($"No se encontró la lista de SharePoint llamada: {listName}");

            string targetItemId = null;

            if (isCompletivo)
            {
                // 2a. Find existing item and append Notes
                var targetItem = await FindListItemByCaseIdAsync(graphClient, listId, targetCaseId);
                if (targetItem == null)
                {
                    throw new Exception($"No se encontró un caso con el ID '{targetCaseId}' en la lista '{listName}'. Verifique que el ID sea correcto.");
                }

                targetItemId = targetItem.Id;

                string existingNotes = "";
                if (targetItem.Fields.AdditionalData.TryGetValue("Notas", out var prevNotas))
                {
                    existingNotes = prevNotas?.ToString() ?? "";
                }

                string newNoteText = itemFields.ContainsKey("Notas") ? itemFields["Notas"].ToString() : "Sin notas adicionales.";
                string dateStamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

                string appendedNotes = string.IsNullOrWhiteSpace(existingNotes)
                    ? $"[{dateStamp} - Completivo]: {newNoteText}"
                    : $"{existingNotes}\n\n[{dateStamp} - Completivo]: {newNoteText}";

                var updateFields = new Dictionary<string, object> { { "Notas", appendedNotes } };

                if (itemFields.ContainsKey("FechaPr_x00f3_ximoSeguimiento"))
                {
                    updateFields.Add("FechaPr_x00f3_ximoSeguimiento", itemFields["FechaPr_x00f3_ximoSeguimiento"]);
                }

                var updateItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = updateFields
                    }
                };

                await graphClient.Sites[sharePointSiteId].Lists[listId].Items[targetItemId].PatchAsync(updateItem);
            }
            else
            {
                // 2b. Create a new List Item with metadata
                var listItem = new ListItem
                {
                    Fields = new FieldValueSet { AdditionalData = itemFields }
                };
                var createdItem = await graphClient.Sites[sharePointSiteId].Lists[listId].Items.PostAsync(listItem);
                targetItemId = createdItem.Id;
            }

            // 3. Attach the PDF file using SharePoint REST API
            string tenantDomain = "coalcava.sharepoint.com";
            string siteName = "COALCAVA787";

            var spTokenContext = new Azure.Core.TokenRequestContext(new[] { $"https://{tenantDomain}/.default" });
            var spToken = await sharedCredential.GetTokenAsync(spTokenContext);

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spToken.Token);
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string restUrl = $"https://{tenantDomain}/sites/{siteName}/_api/web/lists/getByTitle('{listName}')/items({targetItemId})/AttachmentFiles/add(FileName='{finalFileName}')";

                byte[] pdfBytes = File.ReadAllBytes(pdfPath);
                var content = new System.Net.Http.ByteArrayContent(pdfBytes);

                var response = await httpClient.PostAsync(restUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Falló al subir el PDF adjunto. Detalle: {errorDetail}");
                }
            }

            // 4. Create Microsoft To Do Reminder Task if enabled
            if (itemFields.ContainsKey("__ReminderDateTime") && itemFields["__ReminderDateTime"] is DateTime reminderDt)
            {
                string caseLabel = isCompletivoMode ? $"Completivo para {targetCaseId}" : (itemFields.ContainsKey("Title") ? itemFields["Title"].ToString() : targetCaseId);
                string title = $"Seguimiento: {caseLabel} ({listName})";
                string body = isCompletivoMode
                    ? $"Documentos completivos agregados al caso {targetCaseId}.\nNotas: {(itemFields.ContainsKey("Notas") ? itemFields["Notas"].ToString() : "")}"
                    : $"Seguimiento programado para expediente {finalFileName}.\nNotas: {(itemFields.ContainsKey("Notas") ? itemFields["Notas"].ToString() : "")}";

                await CreateToDoReminderTaskAsync(graphClient, title, body, reminderDt);
            }
        }

        #endregion

        #region Helpers Generales

        private void PlaySound(SystemSound sound)
        {
            bool isMuted = chkMuteSound != null && chkMuteSound.Checked;
            if (!isMuted && sound != null) sound.Play();
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
                        if (enableStartup) key.SetValue("CoalcavaDropzone", System.Windows.Forms.Application.ExecutablePath);
                        else key.DeleteValue("CoalcavaDropzone", false);
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error al actualizar registro: {ex.Message}"); }
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
                else if (ctrl.Text != placeholder) ctrl.ForeColor = Color.Black;
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
            return DateTime.Today.AddDays(2).AddHours(8);
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
            pnlReembolsoDrop.DragEnter += PnlReembolsoDrop_DragEnter;
            pnlReembolsoDrop.DragLeave += PnlReembolsoDrop_DragLeave;
            pnlReembolsoDrop.DragDrop += PnlReembolsoDrop_DragDrop;
            pnlReembolsoDrop.Click += PnlReembolsoDrop_Click;

            if (picDownloadIcon != null)
            {
                picDownloadIcon.AllowDrop = true;
                picDownloadIcon.BackColor = Color.Transparent;
                picDownloadIcon.DragEnter += PnlReembolsoDrop_DragEnter;
                picDownloadIcon.DragLeave += PnlReembolsoDrop_DragLeave;
                picDownloadIcon.DragDrop += PnlReembolsoDrop_DragDrop;
                picDownloadIcon.Click += PnlReembolsoDrop_Click;
            }

            if (lblReembolsoDropPrompt != null)
            {
                lblReembolsoDropPrompt.AllowDrop = true;
                lblReembolsoDropPrompt.DragEnter += PnlReembolsoDrop_DragEnter;
                lblReembolsoDropPrompt.DragLeave += PnlReembolsoDrop_DragLeave;
                lblReembolsoDropPrompt.DragDrop += PnlReembolsoDrop_DragDrop;
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

            dgvReembolsoFiles.CellContentClick += DgvReembolsoFiles_CellContentClick;
            dgvReembolsoFiles.CellValueChanged += DgvReembolsoFiles_CellValueChanged;
            dgvReembolsoFiles.CurrentCellDirtyStateChanged += DgvReembolsoFiles_CurrentCellDirtyStateChanged;

            if (btnRemoveSelectedFile != null) btnRemoveSelectedFile.Click += BtnRemoveSelectedFile_Click;
            if (btnClearReembolso != null) btnClearReembolso.Click += BtnClearReembolso_Click;
            if (btnProcessReembolso != null) btnProcessReembolso.Click += BtnProcessReembolso_Click;
            if (btnToggleMode != null) btnToggleMode.Click += BtnToggleMode_Click;
            if (btnToggleAuth != null) btnToggleAuth.Click += BtnToggleAuth_Click;

            if (cmbCurrency != null)
            {
                cmbCurrency.Items.Clear();
                cmbCurrency.Items.AddRange(new string[] { "DOP", "USD" });
                cmbCurrency.SelectedIndex = 0;
            }

            if (txtReembolsoMonto != null)
            {
                txtReembolsoMonto.KeyPress += TxtReembolsoMonto_KeyPress;
                txtReembolsoMonto.LostFocus += TxtReembolsoMonto_LostFocus;
                txtReembolsoMonto.KeyDown += TxtReembolsoMonto_KeyDown;
            }

            // Bindings de recordatorios Reembolsos
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
                    if (chkSendReminder.Checked && !AuthService.IsSignedIn)
                    {
                        PlaySound(SystemSounds.Exclamation);
                        MessageBox.Show("Debe iniciar sesión con su cuenta corporativa de Microsoft en la pestaña Opciones para programar recordatorios de Microsoft To-Do.", "Inicio de Sesión Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        chkSendReminder.Checked = false;
                        return;
                    }

                    if (dtpReminderTime != null)
                    {
                        dtpReminderTime.Enabled = chkSendReminder.Checked;
                        if (chkSendReminder.Checked) dtpReminderTime.Value = GetDefaultReminderDateTime();
                    }
                };
            }

            // Bindings de recordatorios Completivos
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
                    if (chkSendReminderComp.Checked && !AuthService.IsSignedIn)
                    {
                        PlaySound(SystemSounds.Exclamation);
                        MessageBox.Show("Debe iniciar sesión con su cuenta corporativa de Microsoft en la pestaña Opciones para programar recordatorios de Microsoft To-Do.", "Inicio de Sesión Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        chkSendReminderComp.Checked = false;
                        return;
                    }

                    if (dtpReminderTimeComp != null)
                    {
                        dtpReminderTimeComp.Enabled = chkSendReminderComp.Checked;
                        if (chkSendReminderComp.Checked) dtpReminderTimeComp.Value = GetDefaultReminderDateTime();
                    }
                };
            }
        }

        private void InitializeReembolsosTab()
        {
            LoadClientFolders();

            if (cmbReembolsoAseguradora != null)
            {
                cmbReembolsoAseguradora.Items.Clear();
                cmbReembolsoAseguradora.Items.AddRange(new string[] {
                    "Sura", "Humano", "Universal", "La Colonial", "Mapfre", "Mapfre BHD",
                    "SeNaSa", "Cigna", "Bupa", "BMI", "One Alliance", "Aetna",
                    "American Fidelity International", "Yunen", "Seguros Pepin",
                    "WorldWide Medical", "Primera", "Aurora", "APS", "COOPSEGUROS",
                    "Crecer", "La Monumental", "Seguros Reservas", "Otra Aseguradora"
                });
                cmbReembolsoAseguradora.DropDownStyle = ComboBoxStyle.DropDown;
                if (cmbReembolsoAseguradora.Items.Count > 0) cmbReembolsoAseguradora.SelectedIndex = 0;
            }

            if (txtReembolsoPoliza != null)
            {
                txtReembolsoPoliza.DropDownStyle = ComboBoxStyle.DropDown;
                txtReembolsoPoliza.SelectedIndexChanged += (s, e) => { PopulateAffiliatesForPolicy(); };
                txtReembolsoPoliza.TextChanged += (s, e) => { PopulateAffiliatesForPolicy(); };
            }

            if (cmbAfiliado != null)
            {
                cmbAfiliado.DropDownStyle = ComboBoxStyle.DropDown;
                cmbAfiliado.SelectedIndexChanged += (s, e) => AutoFillCedulaFromAffiliate();
                cmbAfiliado.TextChanged += (s, e) => AutoFillCedulaFromAffiliate();
            }

            ApplyPlaceholder(txtReembolsoCedula, PlaceholderCedula);
            ApplyPlaceholder(txtReembolsoClientSearch, PlaceholderText);
            ApplyPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            ApplyPlaceholder(txtReembolsoMonto, PlaceholderMonto);
            ApplyPlaceholder(txtReembolsosNotasAdicionales, PlaceholderNotas);
            ApplyPlaceholder(txtTargetCaseId, PlaceholderTargetCaseId);
            ApplyPlaceholder(txtCompletivoNotas, PlaceholderCompletivoNotas);

            if (txtReembolsoClientSearch != null)
            {
                txtReembolsoClientSearch.TextChanged += TxtReembolsoClientSearch_TextChanged;
                txtReembolsoClientSearch.KeyDown += TxtReembolsoClientSearch_KeyDown;
                txtReembolsoClientSearch.TextChanged += (s, e) => PopulatePoliciesForClient();
            }
            if (lstReembolsoClientSuggestions != null)
            {
                lstReembolsoClientSuggestions.Click += LstReembolsoClientSuggestions_Click;
                lstReembolsoClientSuggestions.KeyDown += LstReembolsoClientSuggestions_KeyDown;
            }

            if (pnlCompletivos != null) pnlCompletivos.Visible = false;
        }

        private void PopulatePoliciesForClient()
        {
            if (txtReembolsoPoliza == null) return;
            string client = GetCleanText(txtReembolsoClientSearch);
            txtReembolsoPoliza.Items.Clear();
            if (string.IsNullOrWhiteSpace(client) || !Directory.Exists(clientRootPath)) return;

            string clientPath = Path.Combine(clientRootPath, client);
            if (!Directory.Exists(clientPath)) return;

            var policies = Directory.GetDirectories(clientPath).Select(d => new DirectoryInfo(d).Name).ToArray();
            txtReembolsoPoliza.Items.AddRange(policies);

            if (policies.Length > 0 && string.IsNullOrWhiteSpace(GetCleanText(txtReembolsoPoliza)))
            {
                txtReembolsoPoliza.SelectedIndex = 0;
                txtReembolsoPoliza.ForeColor = Color.Black;
            }
            else PopulateAffiliatesForPolicy();
        }

        private void PopulateAffiliatesForPolicy()
        {
            if (cmbAfiliado == null) return;
            string client = GetCleanText(txtReembolsoClientSearch);
            string policy = GetCleanText(txtReembolsoPoliza);
            cmbAfiliado.Items.Clear();

            if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(policy)) return;

            string policyPath = Path.Combine(clientRootPath, client, policy);
            if (!Directory.Exists(policyPath)) return;

            var affiliates = Directory.GetDirectories(policyPath).Select(d => new DirectoryInfo(d).Name).ToArray();
            cmbAfiliado.Items.AddRange(affiliates);

            if (cmbAfiliado.Items.Count > 0) cmbAfiliado.SelectedIndex = 0;
            else AutoFillCedulaFromAffiliate();
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
                    txtReembolsoCedula.Text = rawName.Substring(3).Trim();
                    txtReembolsoCedula.ForeColor = Color.Black;
                    return;
                }
            }
            ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
        }

        private void BtnToggleMode_Click(object sender, EventArgs e)
        {
            if (isAuthMode) return;
            isCompletivoMode = !isCompletivoMode;

            if (pnlCompletivos != null) pnlCompletivos.Visible = isCompletivoMode;
            if (btnProcessReembolso != null) btnProcessReembolso.Text = isCompletivoMode ? "Procesar Completivo" : "Procesar";
            if (btnToggleMode != null) btnToggleMode.Text = isCompletivoMode ? "Reembolso" : "Completivo";
        }

        private void BtnToggleAuth_Click(object sender, EventArgs e)
        {
            isAuthMode = !isAuthMode;

            if (isAuthMode)
            {
                if (label7 != null) label7.Text = "Autorizaciones";
                isCompletivoMode = false;
                if (pnlCompletivos != null) pnlCompletivos.Visible = false;
                if (btnProcessReembolso != null) btnProcessReembolso.Text = "Procesar";
                if (btnToggleMode != null) { btnToggleMode.Enabled = false; btnToggleMode.Visible = false; }
                if (btnToggleAuth != null) btnToggleAuth.Text = "Reembolso";
            }
            else
            {
                if (label7 != null) label7.Text = "Reembolsos";
                if (btnToggleMode != null) { btnToggleMode.Enabled = true; btnToggleMode.Visible = true; btnToggleMode.Text = "Completivo"; }
                if (btnToggleAuth != null) btnToggleAuth.Text = "Autorización";
            }
        }

        private string GetConfiguredAppUser()
        {
            if (AuthService.IsSignedIn && !string.IsNullOrWhiteSpace(AuthService.CurrentDisplayName)) return AuthService.CurrentDisplayName;
            string settingUser = Properties.Settings.Default.SelectedAppUser?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(settingUser) && settingUser != PlaceholderAppUser) return settingUser;
            string textVal = GetCleanText(txtAppUser);
            if (!string.IsNullOrWhiteSpace(textVal)) return textVal;
            return string.Empty;
        }

        private string GetUserInitials(string appUser)
        {
            if (string.IsNullOrWhiteSpace(appUser)) return "XX";
            var parts = appUser.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "XX";
            if (parts.Length == 1) return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : $"{parts[0].ToUpper()}X";
            return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
        }

        private string GenerateCaseId(string userInitials, string modePrefix)
        {
            string initials = string.IsNullOrWhiteSpace(userInitials) ? "XX" : userInitials.Trim().ToUpper();
            byte[] bytes = new byte[6];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(bytes); }
            return $"{modePrefix}-{initials}-{BitConverter.ToString(bytes).Replace("-", "")}";
        }

        private void PnlReembolsoDrop_DragEnter(object sender, DragEventArgs e) { if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effect = DragDropEffects.Copy; pnlReembolsoDrop.BackColor = HoverDropzoneColor; } }
        private void PnlReembolsoDrop_DragLeave(object sender, EventArgs e) => pnlReembolsoDrop.BackColor = NormalDropzoneColor;
        private void PnlReembolsoDrop_DragDrop(object sender, DragEventArgs e) { pnlReembolsoDrop.BackColor = NormalDropzoneColor; string[] dropped = (string[])e.Data.GetData(DataFormats.FileDrop); if (dropped != null && dropped.Length > 0) AddFilesToReembolsoQueue(dropped); }
        private void PnlReembolsoDrop_Click(object sender, EventArgs e) { using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Documentos (*.pdf;*.png;*.jpg;*.jpeg;*.bmp)|*.pdf;*.png;*.jpg;*.jpeg;*.bmp|Todos|*.*", Multiselect = true }) { if (ofd.ShowDialog() == DialogResult.OK) AddFilesToReembolsoQueue(ofd.FileNames); } }

        private void AddFilesToReembolsoQueue(string[] filePaths)
        {
            int addedCount = 0;
            string[] allowedExts = { ".pdf", ".png", ".jpg", ".jpeg", ".bmp" };
            foreach (string path in filePaths)
            {
                if (File.Exists(path) && allowedExts.Contains(Path.GetExtension(path).ToLower()))
                {
                    if (!reembolsoItems.Any(x => x.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    {
                        reembolsoItems.Add(new ReembolsoFileItem { FullPath = path, DocumentType = GuessDocTypeFromName(Path.GetFileName(path)) });
                        addedCount++;
                    }
                }
            }
            if (addedCount > 0) RefreshReembolsosGrid();
        }

        private string GuessDocTypeFromName(string fileName)
        {
            string lower = fileName.ToLower();
            if (lower.Contains("fact") || lower.Contains("inv")) return "Factura";
            if (lower.Contains("recib") || lower.Contains("pago")) return "Recibo de Pago Sellado";
            if (lower.Contains("indica") || lower.Contains("receta")) return "Indicación Médica";
            if (lower.Contains("hist") || lower.Contains("clinic")) return "Historia Clínica";
            if (lower.Contains("estudio") || lower.Contains("lab")) return "Resultados de Estudios";
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
                dgvReembolsoFiles.Rows[rowIndex].Cells["colName"].Value = item.FileName;
                dgvReembolsoFiles.Rows[rowIndex].Cells["colType"].Value = item.DocumentType;
                dgvReembolsoFiles.Rows[rowIndex].Cells["colSize"].Value = item.FileSizeFormatted;
                dgvReembolsoFiles.Rows[rowIndex].Tag = item;
            }
            if (lblReembolsoDropPrompt != null) lblReembolsoDropPrompt.Text = reembolsoItems.Count > 0 ? $"{reembolsoItems.Count} documento(s) listos para consolidar." : "Arrastra todos los archivos PDF del caso aquí";
        }

        private void DgvReembolsoFiles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReembolsoFiles.Columns["colName"].Index)
            {
                var item = dgvReembolsoFiles.Rows[e.RowIndex].Tag as ReembolsoFileItem;
                if (item != null && File.Exists(item.FullPath))
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = item.FullPath, UseShellExecute = true }); } catch { }
                }
            }
        }

        private void DgvReembolsoFiles_CurrentCellDirtyStateChanged(object sender, EventArgs e) { if (dgvReembolsoFiles.IsCurrentCellDirty && dgvReembolsoFiles.CurrentCell is DataGridViewComboBoxCell) dgvReembolsoFiles.CommitEdit(DataGridViewDataErrorContexts.Commit); }
        private void DgvReembolsoFiles_CellValueChanged(object sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0 && e.ColumnIndex == dgvReembolsoFiles.Columns["colType"].Index) { var item = dgvReembolsoFiles.Rows[e.RowIndex].Tag as ReembolsoFileItem; if (item != null) item.DocumentType = dgvReembolsoFiles.Rows[e.RowIndex].Cells["colType"].Value?.ToString() ?? "Otros"; } }
        private void BtnRemoveSelectedFile_Click(object sender, EventArgs e) { if (dgvReembolsoFiles.CurrentRow != null) { reembolsoItems.Remove(dgvReembolsoFiles.CurrentRow.Tag as ReembolsoFileItem); RefreshReembolsosGrid(); } }
        private void BtnClearReembolso_Click(object sender, EventArgs e) { if (MessageBox.Show("¿Limpiar todo?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) ClearReembolsosForm(); }

        private void ClearReembolsosForm()
        {
            reembolsoItems.Clear(); dgvReembolsoFiles.Rows.Clear();
            ResetPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
            ResetPlaceholder(txtReembolsoMonto, PlaceholderMonto);
            ResetPlaceholder(txtReembolsosNotasAdicionales, PlaceholderNotas);
            ResetPlaceholder(txtReembolsoClientSearch, PlaceholderText);
            ResetPlaceholder(txtTargetCaseId, PlaceholderTargetCaseId);
            ResetPlaceholder(txtCompletivoNotas, PlaceholderCompletivoNotas);

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
        }

        private void SelectClientFromReembolsoList()
        {
            if (lstReembolsoClientSuggestions.SelectedItem != null)
            {
                txtReembolsoClientSearch.Text = lstReembolsoClientSuggestions.SelectedItem.ToString();
                txtReembolsoClientSearch.ForeColor = Color.Black;
                lstReembolsoClientSuggestions.Visible = false;
                PopulatePoliciesForClient();
            }
        }

        private void TxtReembolsoMonto_LostFocus(object sender, EventArgs e) { EvaluateMontoExpression(); }
        private void TxtReembolsoMonto_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { EvaluateMontoExpression(); e.Handled = true; } }

        private void TxtReembolsoMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '+' && e.KeyChar != '.') e.Handled = true;
        }

        private void EvaluateMontoExpression()
        {
            if (txtReembolsoMonto == null) return;
            string raw = GetCleanText(txtReembolsoMonto);
            if (string.IsNullOrWhiteSpace(raw)) return;

            decimal total = 0m;
            foreach (string term in raw.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (decimal.TryParse(term.Trim().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val)) total += val;
            }
            txtReembolsoMonto.Text = total.ToString("F2", CultureInfo.InvariantCulture);
            txtReembolsoMonto.ForeColor = Color.Black;
        }

        private bool ValidateItemsForMerge(out string validationErrorMessage)
        {
            validationErrorMessage = string.Empty;
            foreach (var item in reembolsoItems)
            {
                if (!File.Exists(item.FullPath) || new FileInfo(item.FullPath).Length == 0)
                {
                    validationErrorMessage = $"Archivo inválido o vacío: {item.FileName}";
                    return false;
                }
            }
            return true;
        }

        // =========================================================================
        // MÉTODO PRINCIPAL DE PROCESAMIENTO REESCRITO PARA MICROSOFT GRAPH
        // =========================================================================
        private async void BtnProcessReembolso_Click(object sender, EventArgs e)
        {
            string appUser = GetConfiguredAppUser();
            if (string.IsNullOrWhiteSpace(appUser))
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Debe definir y configurar un Usuario (AppUser).", "Usuario Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string validationError = string.Empty;
            if (reembolsoItems.Count == 0 || !ValidateItemsForMerge(out validationError))
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show(reembolsoItems.Count == 0 ? "Agregue documentos." : validationError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnProcessReembolso.Enabled = false;

            try
            {
                string userInitials = GetUserInitials(appUser);
                string modePrefix = isAuthMode ? "AUT" : "REE";
                string caseId;
                string finalPdfName;

                string clientName = GetCleanText(txtReembolsoClientSearch);
                string policyNumber = GetCleanText(txtReembolsoPoliza);
                string affiliate = GetCleanText(cmbAfiliado);
                string affiliateIdNumber = GetCleanText(txtReembolsoCedula);
                string insurer = cmbReembolsoAseguradora.SelectedItem?.ToString() ?? "Aseguradora";
                string currency = cmbCurrency?.SelectedItem?.ToString() ?? "DOP";
                string notes = GetCleanText(txtReembolsosNotasAdicionales);
                decimal numericAmount = 0.00m;
                decimal.TryParse(GetCleanText(txtReembolsoMonto), NumberStyles.Any, CultureInfo.InvariantCulture, out numericAmount);

                List<string> presentDocTypes = reembolsoItems.Select(x => x.DocumentType).Distinct().ToList();
                string docTypesString = string.Join(", ", presentDocTypes);

                string targetListName;
                Dictionary<string, object> spFields = new Dictionary<string, object>();

                if (isCompletivoMode)
                {
                    string targetCaseId = GetCleanText(txtTargetCaseId);
                    if (string.IsNullOrWhiteSpace(targetCaseId)) throw new Exception("Debe ingresar el ID Único del caso original.");

                    if (targetCaseId.StartsWith("AUT", StringComparison.OrdinalIgnoreCase))
                    {
                        targetListName = "Autorizaciones";
                    }
                    else if (targetCaseId.StartsWith("REE", StringComparison.OrdinalIgnoreCase))
                    {
                        targetListName = "Reembolsos";
                    }
                    else
                    {
                        throw new Exception("El ID ingresado no es válido. Debe comenzar con 'REE-' o 'AUT-'.");
                    }

                    caseId = targetCaseId;
                    string safeTargetId = string.Concat(targetCaseId.Split(Path.GetInvalidFileNameChars())).Replace(" ", "");
                    finalPdfName = $"CMP_{safeTargetId}_{DateTime.Now.ToString("ddMMMyy", new CultureInfo("es-ES"))}.pdf";

                    string completivoNote = string.IsNullOrWhiteSpace(GetCleanText(txtCompletivoNotas)) ? "Sin notas adicionales." : GetCleanText(txtCompletivoNotas);
                    spFields.Add("Notas", completivoNote);

                    // Recordatorio para Completivo
                    if (chkSendReminderComp != null && chkSendReminderComp.Checked && dtpReminderTimeComp != null)
                    {
                        spFields.Add("FechaPr_x00f3_ximoSeguimiento", dtpReminderTimeComp.Value.ToString("s", CultureInfo.InvariantCulture));
                        spFields.Add("__ReminderDateTime", dtpReminderTimeComp.Value);
                    }
                }
                else
                {
                    targetListName = isAuthMode ? "Autorizaciones" : "Reembolsos";
                    caseId = GenerateCaseId(userInitials, modePrefix);
                    finalPdfName = $"{caseId}.pdf";

                    EnsureDirectoryHierarchy(clientName, policyNumber, affiliate, affiliateIdNumber);

                    spFields.Add("Title", clientName);
                    spFields.Add("Afiliado", string.IsNullOrWhiteSpace(affiliate) ? clientName : affiliate);
                    spFields.Add("Aseguradora", insurer);
                    spFields.Add("Notas", string.IsNullOrWhiteSpace(notes) ? "Sin notas adicionales." : notes);
                    spFields.Add("FechadeIngreso", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    string cleanAffiliateId = string.IsNullOrWhiteSpace(affiliateIdNumber) ? "No indicado" : affiliateIdNumber;

                    if (isAuthMode)
                    {
                        // Nombre interno específico para Autorizaciones
                        spFields.Add("N_x00fa_merodeAfiliado", cleanAffiliateId);
                        spFields.Add("Poliza", policyNumber);
                        spFields.Add("No_x002e_Caso", caseId);
                        spFields.Add("IDdeCaso", caseId);

                        // Recordatorio To Do para Autorizaciones
                        if (chkSendReminder != null && chkSendReminder.Checked && dtpReminderTime != null)
                        {
                            spFields.Add("__ReminderDateTime", dtpReminderTime.Value);
                        }
                    }
                    else
                    {
                        // Nombre interno específico para Reembolsos
                        spFields.Add("DocumentoID", cleanAffiliateId);
                        spFields.Add("P_x00f3_liza", policyNumber);
                        spFields.Add("Monto", numericAmount);
                        spFields.Add("Moneda", currency);
                        spFields.Add("IDdeCaso", caseId);

                        // Recordatorio SharePoint List y To Do para Reembolsos
                        if (chkSendReminder != null && chkSendReminder.Checked && dtpReminderTime != null)
                        {
                            spFields.Add("FechaPr_x00f3_ximoSeguimiento", dtpReminderTime.Value.ToString("s", CultureInfo.InvariantCulture));
                            spFields.Add("__ReminderDateTime", dtpReminderTime.Value);
                        }
                    }
                }

                // 2. Compilación del PDF en el área temporal
                string stagingPdfTempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tmp");
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
                                for (int idx = 0; idx < inputDoc.PageCount; idx++) outputDoc.AddPage(inputDoc.Pages[idx]);
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
                                using (XGraphics gfx = XGraphics.FromPdfPage(page)) { gfx.DrawImage(img, 0, 0, page.Width.Point, page.Height.Point); }
                            }
                        }
                    }
                    outputDoc.Save(stagingPdfTempPath);
                }

                // 3. Carga directa a SharePoint mediante Microsoft Graph API
                string targetCaseIdArg = isCompletivoMode ? GetCleanText(txtTargetCaseId) : null;
                await UploadToSharePointAsync(targetListName, spFields, stagingPdfTempPath, finalPdfName, isCompletivoMode, targetCaseIdArg);

                // 4. Eliminación de archivos temporales y de origen locales
                foreach (var item in reembolsoItems)
                {
                    try { if (File.Exists(item.FullPath)) File.Delete(item.FullPath); } catch { }
                }
                try { if (File.Exists(stagingPdfTempPath)) File.Delete(stagingPdfTempPath); } catch { }

                PlaySound(SystemSounds.Asterisk);
                MessageBox.Show(
                    $"Registro generado y subido a SharePoint exitosamente:\n\n" +
                    $"ID de Caso: {caseId}\n" +
                    $"Lista Destino: {targetListName}",
                    "Operación Completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                LoadClientFolders();
                ClearReembolsosForm();
            }
            catch (Exception ex)
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show($"Ocurrió un error al procesar/subir el caso a SharePoint:\n{ex.Message}", "Error de Procesamiento", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnProcessReembolso.Enabled = true;
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

                            if (!File.Exists(targetIdFilePath)) File.WriteAllText(targetIdFilePath, affiliateIdNumber);
                        }
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error creando jerarquía: {ex.Message}"); }
        }

        #endregion

        #region Configuración Opciones

        private void SetupSettingsTab()
        {
            if (btnBrowseReembolsoRoot != null)
            {
                btnBrowseReembolsoRoot.Click -= btnBrowseReembolsoRoot_Click;
                btnBrowseReembolsoRoot.Click += btnBrowseReembolsoRoot_Click;
            }
            if (btnBrowseRoot != null)
            {
                btnBrowseRoot.Click -= btnBrowseRoot_Click_1;
                btnBrowseRoot.Click += btnBrowseRoot_Click_1;
            }
            if (btnSaveSettings != null)
            {
                btnSaveSettings.Click -= btnSaveSettings_Click_1;
                btnSaveSettings.Click += btnSaveSettings_Click_1;
            }
            if (btnAuthAction != null)
            {
                btnAuthAction.Click -= BtnAuthAction_Click;
                btnAuthAction.Click += BtnAuthAction_Click;
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

            UpdateAuthUI();
        }

        private async void BtnAuthAction_Click(object sender, EventArgs e)
        {
            if (btnAuthAction != null) btnAuthAction.Enabled = false;
            try
            {
                if (AuthService.IsSignedIn)
                {
                    await AuthService.SignOutAsync();
                    sharedGraphClient = null;
                    sharedCredential = null;
                    UpdateAuthUI();
                    MessageBox.Show("Sesión cerrada correctamente.", "Microsoft 365", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    bool ok = await AuthService.SignInInteractiveAsync(this.Handle);
                    if (ok)
                    {
                        UpdateAuthUI();
                        if (txtAppUser != null) { txtAppUser.Text = AuthService.CurrentDisplayName; txtAppUser.ForeColor = Color.Black; }
                        MessageBox.Show($"Bienvenido(a), {AuthService.CurrentDisplayName}.\nSesión iniciada exitosamente.", "Microsoft 365", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex) { PlaySound(SystemSounds.Hand); MessageBox.Show(ex.Message, "Error de Autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { if (btnAuthAction != null) btnAuthAction.Enabled = true; }
        }

        private void btnBrowseRoot_Click_1(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccione la carpeta raíz de Clientes";
                folderDialog.SelectedPath = Directory.Exists(clientRootPath) ? clientRootPath : @"C:\";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    if (txtRootPath != null) txtRootPath.Text = folderDialog.SelectedPath.Replace('/', '\\');
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
                    if (txtReembolsoRootPath != null) txtReembolsoRootPath.Text = folderDialog.SelectedPath.Replace('/', '\\');
                }
            }
        }

        private void btnSaveSettings_Click_1(object sender, EventArgs e)
        {
            string newClientPath = txtRootPath != null ? txtRootPath.Text.Trim().Replace('/', '\\') : clientRootPath;
            string newReembolsoPath = txtReembolsoRootPath != null ? txtReembolsoRootPath.Text.Trim().Replace('/', '\\') : dropzoneFlowPath;

            if (!ValidateOrCreateDirectory(ref newClientPath, "Clientes")) return;
            if (!ValidateOrCreateDirectory(ref newReembolsoPath, "Reembolsos Flow")) return;

            clientRootPath = newClientPath;
            dropzoneFlowPath = newReembolsoPath;

            try
            {
                Properties.Settings.Default.ClientRootPath = clientRootPath;
                Properties.Settings.Default.ReembolsoRootPath = dropzoneFlowPath;

                if (txtAppUser != null) Properties.Settings.Default.SelectedAppUser = GetCleanText(txtAppUser);
                if (chkMuteSound != null) Properties.Settings.Default.IsMuted = chkMuteSound.Checked;

                if (chkOpenOnStartup != null)
                {
                    Properties.Settings.Default.OpenOnStartup = chkOpenOnStartup.Checked;
                    SetStartupRegistry(chkOpenOnStartup.Checked);
                }

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar configuración: {ex.Message}");
            }

            LoadClientFolders();
            MessageBox.Show("Configuración guardada correctamente.", "Opciones", MessageBoxButtons.OK, MessageBoxIcon.Information);
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