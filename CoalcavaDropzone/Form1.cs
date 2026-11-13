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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Core;

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

        // Separador único para carpeta de Póliza ~ Aseguradora
        private const string PolicyInsurerSeparator = " ~ ";

        // SharePoint Configuration
        private readonly string tenantId = "47d4e187-f644-4ecf-8b7d-e4456388865f";
        private readonly string clientId = "ce2d41ed-73df-43da-ae41-fbfa9914df9e";
        private readonly string sharePointSiteId = "coalcava.sharepoint.com,96ae7750-2f9b-4430-895d-461cac1fb449,6a0d36fb-6fab-487d-9353-71e43239c7ba";

        private List<string> cachedClientList = new List<string>();
        private List<ClientRecordItem> allClientRecords = new List<ClientRecordItem>();

        private TokenCredential sharedCredential;
        private GraphServiceClient sharedGraphClient;

        private GraphServiceClient GetGraphClient()
        {
            if (sharedGraphClient == null)
            {
                sharedCredential = new AuthServiceCredential();
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
        private const string PlaceholderSearchRegistros = "Buscar por cliente, póliza, aseguradora, afiliado o cédula...";

        private readonly Color NormalDropzoneColor = Color.FromArgb(248, 248, 246);
        private readonly Color HoverDropzoneColor = Color.FromArgb(254, 243, 235);

        // Estado del modo
        private bool isCompletivoMode = false;
        private bool isAuthMode = false;
        private bool isRegistrosEditMode = false;
        private bool isSuppressingCascadeEvents = false;

        public class ReembolsoFileItem
        {
            public string FullPath { get; set; }
            public string FileName => Path.GetFileName(FullPath);
            public string DocumentType { get; set; }
            public long FileSizeBytes => File.Exists(FullPath) ? new FileInfo(FullPath).Length : 0;
            public string FileSizeFormatted => $"{(FileSizeBytes / 1024.0 / 1024.0):F2} MB";
        }

        public class ClientRecordItem
        {
            public string OriginalClient { get; set; }
            public string OriginalPolicyFolder { get; set; }
            public string OriginalAffiliate { get; set; }
            public string OriginalIdNumber { get; set; }

            public string Client { get; set; }
            public string Policy { get; set; }
            public string Insurer { get; set; }
            public string Affiliate { get; set; }
            public string IdNumber { get; set; }
        }

        private List<ReembolsoFileItem> reembolsoItems = new List<ReembolsoFileItem>();

        private readonly string[] insurerOptions = new string[]
        {
            "Sura", "Humano", "Universal", "La Colonial", "Mapfre", "Mapfre BHD",
            "SeNaSa", "Cigna", "Bupa", "BMI", "One Alliance", "Aetna",
            "American Fidelity International", "Yunen", "Seguros Pepin",
            "WorldWide Medical", "Primera", "Aurora", "APS", "COOPSEGUROS",
            "Crecer", "La Monumental", "Seguros Reservas", "Otra Aseguradora"
        };

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
            }
            catch { }

            clientRootPath = clientRootPath.Replace('/', '\\');
            if (!clientRootPath.EndsWith(@"\")) clientRootPath += @"\";

            if (txtRootPath != null) txtRootPath.Text = clientRootPath;

            SetupRegistrosTab();
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
                        string[] possibleFields = { "IDdeCaso", "ID_x0020_de_x0020_Caso" };

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

        private async Task UploadToSharePointAsync(
            string listName,
            Dictionary<string, object> itemFields,
            string pdfPath,
            string finalFileName,
            bool isCompletivo,
            string targetCaseId,
            DateTime? reminderDateTime = null,
            string policyNumber = null)
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

                if (itemFields.ContainsKey("Fecha_x0020_Seguimiento"))
                {
                    updateFields.Add("Fecha_x0020_Seguimiento", itemFields["Fecha_x0020_Seguimiento"]);
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

            var spTokenContext = new TokenRequestContext(new[] { $"https://{tenantDomain}/.default" });
            var spToken = await sharedCredential.GetTokenAsync(spTokenContext, CancellationToken.None);

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

            // 4. Create Microsoft To Do Reminder Task
            if (reminderDateTime.HasValue)
            {
                string caseId = isCompletivo
                    ? targetCaseId
                    : (itemFields.ContainsKey("IDdeCaso") ? itemFields["IDdeCaso"].ToString() : finalFileName);

                string afiliado = itemFields.ContainsKey("Afiliado")
                    ? itemFields["Afiliado"].ToString()
                    : (itemFields.ContainsKey("Title") ? itemFields["Title"].ToString() : "N/A");

                string poliza = !string.IsNullOrWhiteSpace(policyNumber) ? policyNumber : "N/A";

                string notas = itemFields.ContainsKey("Notas") && !string.IsNullOrWhiteSpace(itemFields["Notas"]?.ToString())
                    ? itemFields["Notas"].ToString()
                    : "Sin notas adicionales.";

                string title = $"Seguimiento: {afiliado} - {caseId}";

                var bodyBuilder = new System.Text.StringBuilder();
                bodyBuilder.AppendLine($"ID del Caso: {caseId}");
                bodyBuilder.AppendLine($"Póliza: {poliza}");
                bodyBuilder.AppendLine($"Afiliado: {afiliado}");
                bodyBuilder.Append($"Notas: {notas}");

                await CreateToDoReminderTaskAsync(graphClient, title, bodyBuilder.ToString(), reminderDateTime.Value);
            }
        }

        #endregion

        #region Helpers Generales & Parsers de Directorio

        private (string PolicyNumber, string Insurer) ParsePolicyFolderName(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return (string.Empty, string.Empty);

            int sepIndex = folderName.IndexOf(PolicyInsurerSeparator, StringComparison.Ordinal);
            if (sepIndex >= 0)
            {
                string pol = folderName.Substring(0, sepIndex).Trim();
                string ins = folderName.Substring(sepIndex + PolicyInsurerSeparator.Length).Trim();
                return (pol, ins);
            }

            return (folderName.Trim(), string.Empty);
        }

        private string BuildPolicyFolderName(string policyNumber, string insurer)
        {
            string cleanPolicy = SanitizeFolderName(policyNumber);
            string cleanInsurer = SanitizeFolderName(insurer);

            if (string.IsNullOrWhiteSpace(cleanInsurer))
                return cleanPolicy;

            return $"{cleanPolicy}{PolicyInsurerSeparator}{cleanInsurer}";
        }

        private string SanitizeFolderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            return string.Concat(name.Split(Path.GetInvalidFileNameChars())).Trim();
        }

        private string FindPolicyDirectoryPath(string clientDir, string policyNumber)
        {
            if (!Directory.Exists(clientDir) || string.IsNullOrWhiteSpace(policyNumber))
                return null;

            foreach (var dir in Directory.GetDirectories(clientDir))
            {
                string dirName = new DirectoryInfo(dir).Name;
                var parsed = ParsePolicyFolderName(dirName);
                if (parsed.PolicyNumber.Equals(policyNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return dir;
                }
            }
            return null;
        }

        private void MoveOrMergeDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(sourceDir) || string.Equals(sourceDir, targetDir, StringComparison.OrdinalIgnoreCase))
                return;

            if (!Directory.Exists(targetDir))
            {
                Directory.Move(sourceDir, targetDir);
                return;
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                }
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(targetDir, new DirectoryInfo(subDir).Name);
                MoveOrMergeDirectory(subDir, destSubDir);
            }

            if (!Directory.EnumerateFileSystemEntries(sourceDir).Any())
            {
                Directory.Delete(sourceDir, false);
            }
        }

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
            RefreshRegistrosData();
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

            if (btnRemoveSelectedFile != null)
            {
                btnRemoveSelectedFile.Click -= BtnRemoveSelectedFile_Click;
                btnRemoveSelectedFile.Click += BtnRemoveSelectedFile_Click;
            }
            if (btnClearReembolso != null)
            {
                btnClearReembolso.Click -= BtnClearReembolso_Click;
                btnClearReembolso.Click += BtnClearReembolso_Click;
            }
            if (btnProcessReembolso != null)
            {
                btnProcessReembolso.Click -= BtnProcessReembolso_Click;
                btnProcessReembolso.Click += BtnProcessReembolso_Click;
            }
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
                txtReembolsoMonto.KeyPress += TxtReembolsoMonto_KeyPress;
                txtReembolsoMonto.LostFocus += TxtReembolsoMonto_LostFocus;
                txtReembolsoMonto.KeyDown += TxtReembolsoMonto_KeyDown;
            }

            // Bindings de recordatorios Reembolsos / Autorizaciones
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
                cmbReembolsoAseguradora.Items.AddRange(insurerOptions);
                cmbReembolsoAseguradora.DropDownStyle = ComboBoxStyle.DropDown;
                if (cmbReembolsoAseguradora.Items.Count > 0) cmbReembolsoAseguradora.SelectedIndex = 0;
            }

            if (txtReembolsoPoliza != null)
            {
                txtReembolsoPoliza.DropDownStyle = ComboBoxStyle.DropDown;
                txtReembolsoPoliza.SelectedIndexChanged += (s, e) =>
                {
                    if (isSuppressingCascadeEvents) return;
                    AutoFillInsurerFromPolicy();
                    PopulateAffiliatesForPolicy();
                };
                txtReembolsoPoliza.TextChanged += (s, e) =>
                {
                    if (isSuppressingCascadeEvents) return;
                    AutoFillInsurerFromPolicy();
                    PopulateAffiliatesForPolicy();
                };
            }

            if (cmbAfiliado != null)
            {
                cmbAfiliado.DropDownStyle = ComboBoxStyle.DropDown;
                cmbAfiliado.SelectedIndexChanged += (s, e) =>
                {
                    if (isSuppressingCascadeEvents) return;
                    AutoFillCedulaFromAffiliate();
                };
                cmbAfiliado.TextChanged += (s, e) =>
                {
                    if (isSuppressingCascadeEvents) return;
                    AutoFillCedulaFromAffiliate();
                };
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
                txtReembolsoClientSearch.TextChanged += (s, e) =>
                {
                    if (isSuppressingCascadeEvents) return;
                    PopulatePoliciesForClient();
                };
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
            if (cmbAfiliado != null)
            {
                cmbAfiliado.Items.Clear();
                cmbAfiliado.Text = string.Empty;
            }
            ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);

            if (string.IsNullOrWhiteSpace(client) || !Directory.Exists(clientRootPath))
            {
                ResetPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
                return;
            }

            string clientPath = Path.Combine(clientRootPath, client);
            if (!Directory.Exists(clientPath))
            {
                ResetPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
                return;
            }

            var policyNumbers = Directory.GetDirectories(clientPath)
                .Select(d => ParsePolicyFolderName(new DirectoryInfo(d).Name).PolicyNumber)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToArray();

            txtReembolsoPoliza.Items.AddRange(policyNumbers);

            if (policyNumbers.Length > 0)
            {
                txtReembolsoPoliza.SelectedIndex = 0;
                txtReembolsoPoliza.ForeColor = Color.Black;
                AutoFillInsurerFromPolicy();
                PopulateAffiliatesForPolicy();
            }
            else
            {
                ResetPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            }
        }

        private void AutoFillInsurerFromPolicy()
        {
            if (cmbReembolsoAseguradora == null) return;
            string client = GetCleanText(txtReembolsoClientSearch);
            string policy = GetCleanText(txtReembolsoPoliza);

            if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(policy)) return;

            string clientPath = Path.Combine(clientRootPath, client);
            string actualPolicyDir = FindPolicyDirectoryPath(clientPath, policy);
            if (actualPolicyDir == null) return;

            string folderName = new DirectoryInfo(actualPolicyDir).Name;
            var parsed = ParsePolicyFolderName(folderName);

            if (!string.IsNullOrWhiteSpace(parsed.Insurer))
            {
                SetInsurerComboValue(parsed.Insurer);
            }
        }

        private void SetInsurerComboValue(string insurerName)
        {
            if (cmbReembolsoAseguradora == null) return;

            if (string.IsNullOrWhiteSpace(insurerName))
            {
                if (cmbReembolsoAseguradora.Items.Count > 0)
                    cmbReembolsoAseguradora.SelectedIndex = 0;
                return;
            }

            int matchIdx = -1;
            for (int i = 0; i < cmbReembolsoAseguradora.Items.Count; i++)
            {
                if (string.Equals(cmbReembolsoAseguradora.Items[i].ToString(), insurerName, StringComparison.OrdinalIgnoreCase))
                {
                    matchIdx = i;
                    break;
                }
            }

            if (matchIdx >= 0)
            {
                cmbReembolsoAseguradora.SelectedIndex = matchIdx;
            }
            else
            {
                cmbReembolsoAseguradora.Text = insurerName;
            }
        }

        private void PopulateAffiliatesForPolicy()
        {
            if (cmbAfiliado == null) return;
            string client = GetCleanText(txtReembolsoClientSearch);
            string policy = GetCleanText(txtReembolsoPoliza);
            cmbAfiliado.Items.Clear();
            cmbAfiliado.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(policy))
            {
                ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
                return;
            }

            string clientPath = Path.Combine(clientRootPath, client);
            string policyPath = FindPolicyDirectoryPath(clientPath, policy);
            if (policyPath == null || !Directory.Exists(policyPath))
            {
                ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
                return;
            }

            var affiliates = Directory.GetDirectories(policyPath).Select(d => new DirectoryInfo(d).Name).ToArray();
            cmbAfiliado.Items.AddRange(affiliates);

            if (cmbAfiliado.Items.Count > 0)
            {
                cmbAfiliado.SelectedIndex = 0;
                AutoFillCedulaFromAffiliate();
            }
            else
            {
                ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
            }
        }

        private void AutoFillCedulaFromAffiliate()
        {
            if (txtReembolsoCedula == null) return;
            string client = GetCleanText(txtReembolsoClientSearch);
            string policy = GetCleanText(txtReembolsoPoliza);
            string affiliate = GetCleanText(cmbAfiliado);

            if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(policy) || string.IsNullOrWhiteSpace(affiliate))
            {
                ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
                return;
            }

            string clientPath = Path.Combine(clientRootPath, client);
            string policyPath = FindPolicyDirectoryPath(clientPath, policy);
            if (policyPath == null)
            {
                ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
                return;
            }

            string affiliatePath = Path.Combine(policyPath, affiliate);
            if (!Directory.Exists(affiliatePath))
            {
                ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
                return;
            }

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
            isCompletivoMode = !isCompletivoMode;

            if (pnlCompletivos != null) pnlCompletivos.Visible = isCompletivoMode;
            if (btnProcessReembolso != null) btnProcessReembolso.Text = "Procesar";

            if (btnToggleMode != null)
            {
                string activeType = isAuthMode ? "Autorización" : "Reembolso";
                btnToggleMode.Text = isCompletivoMode ? activeType : "Completivo";
            }
        }

        private void BtnToggleAuth_Click(object sender, EventArgs e)
        {
            isAuthMode = !isAuthMode;

            if (label7 != null)
                label7.Text = isAuthMode ? "Autorizaciones" : "Reembolsos";

            if (btnToggleAuth != null)
                btnToggleAuth.Text = isAuthMode ? "Reembolso" : "Autorización";

            if (btnToggleMode != null)
            {
                btnToggleMode.Enabled = true;
                btnToggleMode.Visible = true;
                string activeType = isAuthMode ? "Autorización" : "Reembolso";
                btnToggleMode.Text = isCompletivoMode ? activeType : "Completivo";
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
            if (lower.Contains("ced") || lower.Contains("id") || lower.Contains("afiliado")) return "Cédula / Documento de Identidad";
            if (lower.Contains("form") || lower.Contains("solicitud")) return "Formulario de Solicitud";
            return "Cédula / Documento de Identidad";
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
            if (lblReembolsoDropPrompt != null) lblReembolsoDropPrompt.Text = reembolsoItems.Count > 0 ? $"{reembolsoItems.Count} documento(s) listos para consolidar." : "Arrastra un archivo o haz clic aquí.";
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
            reembolsoItems.Clear();
            dgvReembolsoFiles.Rows.Clear();

            if (lblReembolsoDropPrompt != null)
                lblReembolsoDropPrompt.Text = "Arrastra todos los archivos PDF del caso aquí";

            if (lstReembolsoClientSuggestions != null)
                lstReembolsoClientSuggestions.Visible = false;

            ResetPlaceholder(txtReembolsoClientSearch, PlaceholderText);

            if (txtReembolsoPoliza != null)
            {
                txtReembolsoPoliza.Items.Clear();
                ResetPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            }

            if (cmbAfiliado != null)
            {
                cmbAfiliado.Items.Clear();
                cmbAfiliado.Text = string.Empty;
            }

            if (cmbReembolsoAseguradora != null && cmbReembolsoAseguradora.Items.Count > 0)
            {
                cmbReembolsoAseguradora.SelectedIndex = 0;
            }

            ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
            ResetPlaceholder(txtReembolsoMonto, PlaceholderMonto);
            ResetPlaceholder(txtReembolsosNotasAdicionales, PlaceholderNotas);
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
            if (isSuppressingCascadeEvents) return;

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
                string insurer = cmbReembolsoAseguradora?.SelectedItem?.ToString() ?? cmbReembolsoAseguradora?.Text?.Trim() ?? "Aseguradora";
                string currency = cmbCurrency?.SelectedItem?.ToString() ?? "DOP";
                string notes = GetCleanText(txtReembolsosNotasAdicionales);
                decimal numericAmount = 0.00m;
                decimal.TryParse(GetCleanText(txtReembolsoMonto), NumberStyles.Any, CultureInfo.InvariantCulture, out numericAmount);

                string targetListName;
                Dictionary<string, object> spFields = new Dictionary<string, object>();
                DateTime? reminderDt = null;

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

                    string cleanAffiliate = string.IsNullOrWhiteSpace(affiliate) ? clientName : affiliate;
                    spFields.Add("Afiliado", cleanAffiliate);

                    if (chkSendReminderComp != null && chkSendReminderComp.Checked && dtpReminderTimeComp != null)
                    {
                        spFields.Add("Fecha_x0020_Seguimiento", dtpReminderTimeComp.Value.ToString("s", CultureInfo.InvariantCulture));
                        reminderDt = dtpReminderTimeComp.Value;
                    }
                }
                else
                {
                    targetListName = isAuthMode ? "Autorizaciones" : "Reembolsos";
                    caseId = GenerateCaseId(userInitials, modePrefix);
                    finalPdfName = $"{caseId}.pdf";

                    EnsureDirectoryHierarchy(clientName, policyNumber, affiliate, affiliateIdNumber, insurer);

                    string cleanAffiliate = string.IsNullOrWhiteSpace(affiliate) ? clientName : affiliate;
                    string cleanAffiliateId = string.IsNullOrWhiteSpace(affiliateIdNumber) ? "No indicado" : affiliateIdNumber;

                    spFields.Add("Title", clientName);
                    spFields.Add("Afiliado", cleanAffiliate);
                    spFields.Add("Aseguradora", insurer);
                    spFields.Add("Notas", string.IsNullOrWhiteSpace(notes) ? "Sin notas adicionales." : notes);
                    spFields.Add("FechadeIngreso", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    spFields.Add("IDdeCaso", caseId);

                    if (isAuthMode)
                    {
                        spFields.Add("N_x00fa_merodeAfiliado", cleanAffiliateId);
                        spFields.Add("Poliza", policyNumber);
                        spFields.Add("No_x002e_Caso", caseId);

                        if (chkSendReminder != null && chkSendReminder.Checked && dtpReminderTime != null)
                        {
                            spFields.Add("Fecha_x0020_Seguimiento", dtpReminderTime.Value.ToString("s", CultureInfo.InvariantCulture));
                            reminderDt = dtpReminderTime.Value;
                        }
                    }
                    else
                    {
                        spFields.Add("DocumentoID", cleanAffiliateId);
                        spFields.Add("P_x00f3_liza", policyNumber);
                        spFields.Add("Monto", numericAmount);
                        spFields.Add("Moneda", currency);

                        if (chkSendReminder != null && chkSendReminder.Checked && dtpReminderTime != null)
                        {
                            spFields.Add("Fecha_x0020_Seguimiento", dtpReminderTime.Value.ToString("s", CultureInfo.InvariantCulture));
                            reminderDt = dtpReminderTime.Value;
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
                await UploadToSharePointAsync(
                    targetListName,
                    spFields,
                    stagingPdfTempPath,
                    finalPdfName,
                    isCompletivoMode,
                    targetCaseIdArg,
                    reminderDt,
                    policyNumber);

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

                // Re-indexar automáticamente el caché local y la pestaña Registros
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

        private void EnsureDirectoryHierarchy(string client, string policy, string affiliate, string affiliateIdNumber = "", string insurer = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(client) || !Directory.Exists(clientRootPath)) return;
                string safeClient = SanitizeFolderName(client);
                string clientDir = Path.Combine(clientRootPath, safeClient);
                if (!Directory.Exists(clientDir)) Directory.CreateDirectory(clientDir);

                if (!string.IsNullOrWhiteSpace(policy))
                {
                    string existingPolicyDir = FindPolicyDirectoryPath(clientDir, policy);
                    string targetPolicyFolderName = BuildPolicyFolderName(policy, insurer);
                    string policyDir;

                    if (existingPolicyDir != null)
                    {
                        string currentDirName = new DirectoryInfo(existingPolicyDir).Name;
                        var parsed = ParsePolicyFolderName(currentDirName);

                        if (string.IsNullOrWhiteSpace(parsed.Insurer) && !string.IsNullOrWhiteSpace(insurer))
                        {
                            string renamedPath = Path.Combine(clientDir, targetPolicyFolderName);
                            MoveOrMergeDirectory(existingPolicyDir, renamedPath);
                            policyDir = renamedPath;
                        }
                        else
                        {
                            policyDir = existingPolicyDir;
                        }
                    }
                    else
                    {
                        policyDir = Path.Combine(clientDir, targetPolicyFolderName);
                        if (!Directory.Exists(policyDir)) Directory.CreateDirectory(policyDir);
                    }

                    if (!string.IsNullOrWhiteSpace(affiliate))
                    {
                        string safeAffiliate = SanitizeFolderName(affiliate);
                        string affiliateDir = Path.Combine(policyDir, safeAffiliate);
                        if (!Directory.Exists(affiliateDir)) Directory.CreateDirectory(affiliateDir);

                        if (!string.IsNullOrWhiteSpace(affiliateIdNumber) && affiliateIdNumber != PlaceholderCedula)
                        {
                            string cleanDoc = SanitizeFolderName(affiliateIdNumber);
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

        #region Pestaña Registros (Editor de Clientes, Pólizas, Aseguradoras y Afiliados)

        private void UpdateRegistrosPathLabel(string subPath = "")
        {
            if (lblRegistrosCurrentPath == null) return;

            string displayPath = string.IsNullOrWhiteSpace(subPath)
                ? clientRootPath.TrimEnd('\\')
                : subPath.TrimEnd('\\');

            lblRegistrosCurrentPath.Text = $"Ruta: {displayPath}";
        }

        private void SetRegistrosEditMode(bool editing)
        {
            isRegistrosEditMode = editing;

            if (dgvRegistros != null)
            {
                dgvRegistros.ReadOnly = !editing;
                dgvRegistros.AllowUserToAddRows = editing;
                dgvRegistros.AllowUserToDeleteRows = editing;
            }

            if (btnSaveRegistros != null)
                btnSaveRegistros.Text = editing ? "Guardar" : "Editar";

            if (btnCancelRegistros != null)
                btnCancelRegistros.Visible = editing;

            if (btnDeleteRegistro != null)
                btnDeleteRegistro.Visible = editing;
        }

        private void SetupRegistrosTab()
        {
            if (dgvRegistros == null) return;

            dgvRegistros.AutoGenerateColumns = false;
            dgvRegistros.RowHeadersVisible = true;
            dgvRegistros.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvRegistros.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvRegistros.Columns.Clear();

            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Titular",
                Name = "colRegClient",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25
            });

            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "No. Póliza",
                Name = "colRegPolicy",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 18
            });

            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Afiliado",
                Name = "colRegAffiliate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 22
            });

            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "No. Afiliado",
                Name = "colRegIdNumber",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            });

            DataGridViewComboBoxColumn colInsurer = new DataGridViewComboBoxColumn
            {
                HeaderText = "Aseguradora",
                Name = "colRegInsurer",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 20,
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            colInsurer.Items.Add("");
            colInsurer.Items.AddRange(insurerOptions);
            dgvRegistros.Columns.Add(colInsurer);

            dgvRegistros.DataError += (s, e) => { e.ThrowException = false; };
            dgvRegistros.SelectionChanged -= DgvRegistros_SelectionChanged;
            dgvRegistros.SelectionChanged += DgvRegistros_SelectionChanged;

            dgvRegistros.CurrentCellDirtyStateChanged -= DgvRegistros_CurrentCellDirtyStateChanged;
            dgvRegistros.CurrentCellDirtyStateChanged += DgvRegistros_CurrentCellDirtyStateChanged;
            dgvRegistros.CellValueChanged -= DgvRegistros_CellValueChanged;
            dgvRegistros.CellValueChanged += DgvRegistros_CellValueChanged;

            if (txtSearchRegistros != null)
            {
                ApplyPlaceholder(txtSearchRegistros, PlaceholderSearchRegistros);
                txtSearchRegistros.TextChanged -= TxtSearchRegistros_TextChanged;
                txtSearchRegistros.TextChanged += TxtSearchRegistros_TextChanged;
            }

            if (btnSaveRegistros != null)
            {
                btnSaveRegistros.Click -= BtnSaveRegistros_Click;
                btnSaveRegistros.Click += BtnSaveRegistros_Click;
            }

            if (btnCancelRegistros != null)
            {
                btnCancelRegistros.Click -= BtnCancelRegistros_Click;
                btnCancelRegistros.Click += BtnCancelRegistros_Click;
            }

            if (btnDeleteRegistro != null)
            {
                btnDeleteRegistro.Click -= BtnDeleteRegistro_Click;
                btnDeleteRegistro.Click += BtnDeleteRegistro_Click;
            }

            if (btnRefreshRegistros != null)
            {
                btnRefreshRegistros.Click -= BtnRefreshRegistros_Click;
                btnRefreshRegistros.Click += BtnRefreshRegistros_Click;
            }

            if (btnUseRegistro != null)
            {
                btnUseRegistro.Click -= BtnUseRegistro_Click;
                btnUseRegistro.Click += BtnUseRegistro_Click;
            }

            SetRegistrosEditMode(false);
            UpdateRegistrosPathLabel();
            RefreshRegistrosData();
        }

        private void BtnUseRegistro_Click(object sender, EventArgs e)
        {
            if (dgvRegistros == null || dgvRegistros.CurrentRow == null || dgvRegistros.CurrentRow.IsNewRow)
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Seleccione un registro válido de la tabla primero.", "Registros", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvRegistros.CurrentRow;
            string client = SanitizeFolderName(row.Cells["colRegClient"].Value?.ToString() ?? "");
            string policy = SanitizeFolderName(row.Cells["colRegPolicy"].Value?.ToString() ?? "");
            string insurer = row.Cells["colRegInsurer"].Value?.ToString()?.Trim() ?? "";
            string affiliate = SanitizeFolderName(row.Cells["colRegAffiliate"].Value?.ToString() ?? "");
            string idNumber = SanitizeFolderName(row.Cells["colRegIdNumber"].Value?.ToString() ?? "");

            if (string.IsNullOrWhiteSpace(client))
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("El registro seleccionado no contiene un nombre de Titular válido.", "Registros", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isSuppressingCascadeEvents = true;
            try
            {
                // 1. Llenar Titular / Cliente
                if (txtReembolsoClientSearch != null)
                {
                    txtReembolsoClientSearch.Text = client;
                    txtReembolsoClientSearch.ForeColor = Color.Black;
                }
                if (lstReembolsoClientSuggestions != null)
                    lstReembolsoClientSuggestions.Visible = false;

                // 2. Poblar y seleccionar Póliza
                if (txtReembolsoPoliza != null)
                {
                    txtReembolsoPoliza.Items.Clear();
                    string clientDir = Path.Combine(clientRootPath, client);
                    if (Directory.Exists(clientDir))
                    {
                        var policyNumbers = Directory.GetDirectories(clientDir)
                            .Select(d => ParsePolicyFolderName(new DirectoryInfo(d).Name).PolicyNumber)
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Distinct()
                            .ToArray();
                        txtReembolsoPoliza.Items.AddRange(policyNumbers);
                    }

                    if (!string.IsNullOrWhiteSpace(policy))
                    {
                        txtReembolsoPoliza.Text = policy;
                        txtReembolsoPoliza.ForeColor = Color.Black;
                    }
                    else
                    {
                        ResetPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
                    }
                }

                // 3. Seleccionar Aseguradora
                SetInsurerComboValue(insurer);

                // 4. Poblar y seleccionar Afiliado
                if (cmbAfiliado != null)
                {
                    cmbAfiliado.Items.Clear();
                    string clientDir = Path.Combine(clientRootPath, client);
                    string policyDir = FindPolicyDirectoryPath(clientDir, policy);
                    if (policyDir != null && Directory.Exists(policyDir))
                    {
                        var affiliates = Directory.GetDirectories(policyDir).Select(d => new DirectoryInfo(d).Name).ToArray();
                        cmbAfiliado.Items.AddRange(affiliates);
                    }

                    if (!string.IsNullOrWhiteSpace(affiliate))
                    {
                        cmbAfiliado.Text = affiliate;
                        cmbAfiliado.ForeColor = Color.Black;
                    }
                    else
                    {
                        cmbAfiliado.Text = string.Empty;
                    }
                }

                // 5. Llenar No. Afiliado / Cédula
                if (txtReembolsoCedula != null)
                {
                    if (!string.IsNullOrWhiteSpace(idNumber))
                    {
                        txtReembolsoCedula.Text = idNumber;
                        txtReembolsoCedula.ForeColor = Color.Black;
                    }
                    else
                    {
                        ResetPlaceholder(txtReembolsoCedula, PlaceholderCedula);
                    }
                }
            }
            finally
            {
                isSuppressingCascadeEvents = false;
            }

            // Cambiar automáticamente a la pestaña de Reembolsos / Autorizaciones
            TabControl mainTabControl = FindParentTabControl(dgvRegistros);
            if (mainTabControl != null && mainTabControl.TabPages.Count > 0)
            {
                mainTabControl.SelectedIndex = 0;
            }

            PlaySound(SystemSounds.Asterisk);
        }

        private TabControl FindParentTabControl(Control ctrl)
        {
            Control current = ctrl;
            while (current != null)
            {
                if (current is TabControl tc) return tc;
                current = current.Parent;
            }
            return null;
        }

        private void DgvRegistros_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvRegistros != null && dgvRegistros.IsCurrentCellDirty && dgvRegistros.CurrentCell is DataGridViewComboBoxCell)
            {
                dgvRegistros.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private bool isSyncingInsurerCells = false;
        private void DgvRegistros_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (isSyncingInsurerCells || dgvRegistros == null || e.RowIndex < 0) return;

            if (e.ColumnIndex == dgvRegistros.Columns["colRegInsurer"].Index)
            {
                var editedRow = dgvRegistros.Rows[e.RowIndex];
                string client = SanitizeFolderName(editedRow.Cells["colRegClient"].Value?.ToString() ?? "");
                string policy = SanitizeFolderName(editedRow.Cells["colRegPolicy"].Value?.ToString() ?? "");
                string newInsurer = editedRow.Cells["colRegInsurer"].Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(client) || string.IsNullOrWhiteSpace(policy)) return;

                isSyncingInsurerCells = true;
                try
                {
                    foreach (DataGridViewRow row in dgvRegistros.Rows)
                    {
                        if (row.IsNewRow || row.Index == e.RowIndex) continue;

                        string rowClient = SanitizeFolderName(row.Cells["colRegClient"].Value?.ToString() ?? "");
                        string rowPolicy = SanitizeFolderName(row.Cells["colRegPolicy"].Value?.ToString() ?? "");

                        if (string.Equals(client, rowClient, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(policy, rowPolicy, StringComparison.OrdinalIgnoreCase))
                        {
                            var insurerCell = row.Cells["colRegInsurer"] as DataGridViewComboBoxCell;
                            if (insurerCell != null && !string.IsNullOrWhiteSpace(newInsurer) && !insurerCell.Items.Contains(newInsurer))
                            {
                                insurerCell.Items.Add(newInsurer);
                            }
                            row.Cells["colRegInsurer"].Value = newInsurer;
                        }
                    }
                }
                finally
                {
                    isSyncingInsurerCells = false;
                }
            }
        }

        private void BtnCancelRegistros_Click(object sender, EventArgs e)
        {
            if (dgvRegistros != null) dgvRegistros.CancelEdit();
            SetRegistrosEditMode(false);
            RefreshRegistrosData();
        }

        private void BtnDeleteRegistro_Click(object sender, EventArgs e)
        {
            if (!isRegistrosEditMode || dgvRegistros == null || dgvRegistros.CurrentRow == null || dgvRegistros.CurrentRow.IsNewRow)
                return;

            var row = dgvRegistros.CurrentRow;
            var rec = row.Tag as ClientRecordItem;

            if (rec == null)
            {
                dgvRegistros.Rows.Remove(row);
                return;
            }

            string desc = $"{rec.Client}" +
                (!string.IsNullOrWhiteSpace(rec.Policy) ? $" > {rec.Policy}" : "") +
                (!string.IsNullOrWhiteSpace(rec.Affiliate) ? $" > {rec.Affiliate}" : "");

            if (MessageBox.Show(
                $"¿Está seguro de eliminar permanentemente este registro?\n\n{desc}",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                string clientDir = Path.Combine(clientRootPath, rec.OriginalClient);

                if (!string.IsNullOrWhiteSpace(rec.OriginalPolicyFolder))
                {
                    string policyDir = Path.Combine(clientDir, rec.OriginalPolicyFolder);

                    if (!string.IsNullOrWhiteSpace(rec.OriginalAffiliate))
                    {
                        string affDir = Path.Combine(policyDir, rec.OriginalAffiliate);
                        if (Directory.Exists(affDir))
                            Directory.Delete(affDir, true);

                        if (Directory.Exists(policyDir) && !Directory.EnumerateFileSystemEntries(policyDir).Any())
                            Directory.Delete(policyDir, false);
                    }
                    else if (Directory.Exists(policyDir))
                    {
                        Directory.Delete(policyDir, true);
                    }

                    if (Directory.Exists(clientDir) && !Directory.EnumerateFileSystemEntries(clientDir).Any())
                        Directory.Delete(clientDir, false);
                }
                else if (Directory.Exists(clientDir))
                {
                    Directory.Delete(clientDir, true);
                }

                LoadClientFolders();
                PlaySound(SystemSounds.Asterisk);
                MessageBox.Show("Registro eliminado correctamente.", "Registros", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show($"Error al eliminar la carpeta local:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvRegistros_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRegistros == null || dgvRegistros.CurrentRow == null || dgvRegistros.CurrentRow.IsNewRow)
            {
                UpdateRegistrosPathLabel();
                return;
            }

            var row = dgvRegistros.CurrentRow;
            string client = SanitizeFolderName(row.Cells["colRegClient"].Value?.ToString() ?? "");
            string policy = SanitizeFolderName(row.Cells["colRegPolicy"].Value?.ToString() ?? "");
            string insurer = SanitizeFolderName(row.Cells["colRegInsurer"].Value?.ToString() ?? "");
            string affiliate = SanitizeFolderName(row.Cells["colRegAffiliate"].Value?.ToString() ?? "");

            if (string.IsNullOrWhiteSpace(client))
            {
                UpdateRegistrosPathLabel();
                return;
            }

            string targetPath = Path.Combine(clientRootPath, client);

            if (!string.IsNullOrWhiteSpace(policy))
            {
                string policyFolder = BuildPolicyFolderName(policy, insurer);
                targetPath = Path.Combine(targetPath, policyFolder);

                if (!string.IsNullOrWhiteSpace(affiliate))
                {
                    targetPath = Path.Combine(targetPath, affiliate);
                }
            }

            UpdateRegistrosPathLabel(targetPath);
        }

        private void BtnRefreshRegistros_Click(object sender, EventArgs e)
        {
            if (isRegistrosEditMode)
                SetRegistrosEditMode(false);

            LoadClientFolders();
            PlaySound(SystemSounds.Asterisk);
        }

        private void RefreshRegistrosData()
        {
            if (dgvRegistros == null) return;

            allClientRecords.Clear();

            if (Directory.Exists(clientRootPath))
            {
                foreach (var clientDir in Directory.GetDirectories(clientRootPath))
                {
                    string clientName = new DirectoryInfo(clientDir).Name;
                    var policyDirs = Directory.GetDirectories(clientDir);

                    if (policyDirs.Length == 0)
                    {
                        allClientRecords.Add(new ClientRecordItem
                        {
                            OriginalClient = clientName,
                            Client = clientName
                        });
                        continue;
                    }

                    foreach (var polDir in policyDirs)
                    {
                        string rawPolName = new DirectoryInfo(polDir).Name;
                        var parsedPol = ParsePolicyFolderName(rawPolName);
                        var affDirs = Directory.GetDirectories(polDir);

                        if (affDirs.Length == 0)
                        {
                            allClientRecords.Add(new ClientRecordItem
                            {
                                OriginalClient = clientName,
                                OriginalPolicyFolder = rawPolName,
                                Client = clientName,
                                Policy = parsedPol.PolicyNumber,
                                Insurer = parsedPol.Insurer
                            });
                            continue;
                        }

                        foreach (var affDir in affDirs)
                        {
                            string affName = new DirectoryInfo(affDir).Name;
                            string idNumber = string.Empty;

                            var idFile = Directory.GetFiles(affDir, "ID_*.txt").FirstOrDefault();
                            if (idFile != null)
                            {
                                string rawId = Path.GetFileNameWithoutExtension(idFile);
                                if (rawId.Length > 3) idNumber = rawId.Substring(3).Trim();
                            }

                            allClientRecords.Add(new ClientRecordItem
                            {
                                OriginalClient = clientName,
                                OriginalPolicyFolder = rawPolName,
                                OriginalAffiliate = affName,
                                OriginalIdNumber = idNumber,
                                Client = clientName,
                                Policy = parsedPol.PolicyNumber,
                                Insurer = parsedPol.Insurer,
                                Affiliate = affName,
                                IdNumber = idNumber
                            });
                        }
                    }
                }
            }

            UpdateRegistrosPathLabel();
            string currentFilter = txtSearchRegistros != null ? GetCleanText(txtSearchRegistros) : string.Empty;
            PopulateRegistrosGrid(currentFilter);
        }

        private void TxtSearchRegistros_TextChanged(object sender, EventArgs e)
        {
            string query = GetCleanText(txtSearchRegistros);
            PopulateRegistrosGrid(query);
        }

        private void PopulateRegistrosGrid(string searchFilter)
        {
            if (dgvRegistros == null || dgvRegistros.Columns.Count == 0) return;

            dgvRegistros.Rows.Clear();

            IEnumerable<ClientRecordItem> filtered = allClientRecords;
            if (!string.IsNullOrWhiteSpace(searchFilter))
            {
                filtered = allClientRecords.Where(r =>
                    (r.Client?.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.Policy?.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.Insurer?.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.Affiliate?.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.IdNumber?.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            foreach (var rec in filtered)
            {
                int idx = dgvRegistros.Rows.Add();
                var row = dgvRegistros.Rows[idx];
                row.Cells["colRegClient"].Value = rec.Client;
                row.Cells["colRegPolicy"].Value = rec.Policy;

                var insurerCell = row.Cells["colRegInsurer"] as DataGridViewComboBoxCell;
                if (insurerCell != null && !string.IsNullOrWhiteSpace(rec.Insurer) && !insurerCell.Items.Contains(rec.Insurer))
                {
                    insurerCell.Items.Add(rec.Insurer);
                }
                row.Cells["colRegInsurer"].Value = rec.Insurer ?? "";

                row.Cells["colRegAffiliate"].Value = rec.Affiliate;
                row.Cells["colRegIdNumber"].Value = rec.IdNumber;
                row.Tag = rec;
            }
        }

        private void BtnSaveRegistros_Click(object sender, EventArgs e)
        {
            if (dgvRegistros == null) return;

            if (!isRegistrosEditMode)
            {
                SetRegistrosEditMode(true);
                return;
            }

            dgvRegistros.EndEdit();

            if (!Directory.Exists(clientRootPath))
            {
                MessageBox.Show("La carpeta raíz de Clientes no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int changesCount = 0;

            try
            {
                var rowData = new List<(ClientRecordItem Orig, string NewClient, string NewPolicy, string NewInsurer, string NewAffiliate, string NewIdNumber)>();

                foreach (DataGridViewRow row in dgvRegistros.Rows)
                {
                    if (row.IsNewRow) continue;

                    string newClient = SanitizeFolderName(row.Cells["colRegClient"].Value?.ToString() ?? "");
                    string newPolicy = SanitizeFolderName(row.Cells["colRegPolicy"].Value?.ToString() ?? "");
                    string newInsurer = SanitizeFolderName(row.Cells["colRegInsurer"].Value?.ToString() ?? "");
                    string newAffiliate = SanitizeFolderName(row.Cells["colRegAffiliate"].Value?.ToString() ?? "");
                    string newIdNumber = SanitizeFolderName(row.Cells["colRegIdNumber"].Value?.ToString() ?? "");

                    if (string.IsNullOrWhiteSpace(newClient)) continue;

                    rowData.Add((row.Tag as ClientRecordItem, newClient, newPolicy, newInsurer, newAffiliate, newIdNumber));
                }

                // 1. Renombrar o fusionar carpetas de Cliente una sola vez por carpeta original
                var processedClients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in rowData.Where(x => x.Orig != null && !string.IsNullOrWhiteSpace(x.Orig.OriginalClient)))
                {
                    if (!processedClients.Add(item.Orig.OriginalClient)) continue;

                    string oldClientDir = Path.Combine(clientRootPath, item.Orig.OriginalClient);
                    string targetClientDir = Path.Combine(clientRootPath, item.NewClient);

                    if (!string.Equals(item.Orig.OriginalClient, item.NewClient, StringComparison.OrdinalIgnoreCase) && Directory.Exists(oldClientDir))
                    {
                        MoveOrMergeDirectory(oldClientDir, targetClientDir);
                        changesCount++;
                    }
                    else if (!Directory.Exists(targetClientDir))
                    {
                        Directory.CreateDirectory(targetClientDir);
                        changesCount++;
                    }
                }

                // 2. Renombrar o fusionar carpetas de Póliza ~ Aseguradora una sola vez por carpeta original
                var processedPolicies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in rowData.Where(x => x.Orig != null && !string.IsNullOrWhiteSpace(x.Orig.OriginalPolicyFolder) && !string.IsNullOrWhiteSpace(x.NewPolicy)))
                {
                    string policyKey = $"{item.NewClient}|{item.Orig.OriginalPolicyFolder}";
                    if (!processedPolicies.Add(policyKey)) continue;

                    string activeClientDir = Path.Combine(clientRootPath, item.NewClient);
                    string oldPolicyDir = Path.Combine(activeClientDir, item.Orig.OriginalPolicyFolder);
                    string targetPolicyFolderName = BuildPolicyFolderName(item.NewPolicy, item.NewInsurer);
                    string targetPolicyDir = Path.Combine(activeClientDir, targetPolicyFolderName);

                    if (!string.Equals(item.Orig.OriginalPolicyFolder, targetPolicyFolderName, StringComparison.OrdinalIgnoreCase) && Directory.Exists(oldPolicyDir))
                    {
                        MoveOrMergeDirectory(oldPolicyDir, targetPolicyDir);
                        changesCount++;
                    }
                    else if (!Directory.Exists(targetPolicyDir))
                    {
                        Directory.CreateDirectory(targetPolicyDir);
                        changesCount++;
                    }
                }

                // 3. Procesar cada fila de Afiliado / ID y filas nuevas
                foreach (var item in rowData)
                {
                    string activeClientDir = Path.Combine(clientRootPath, item.NewClient);
                    if (!Directory.Exists(activeClientDir))
                    {
                        Directory.CreateDirectory(activeClientDir);
                    }

                    if (string.IsNullOrWhiteSpace(item.NewPolicy)) continue;

                    string targetPolicyFolderName = BuildPolicyFolderName(item.NewPolicy, item.NewInsurer);
                    string activePolicyDir = Path.Combine(activeClientDir, targetPolicyFolderName);
                    if (!Directory.Exists(activePolicyDir))
                    {
                        Directory.CreateDirectory(activePolicyDir);
                        changesCount++;
                    }

                    if (string.IsNullOrWhiteSpace(item.NewAffiliate)) continue;

                    string targetAffDir = Path.Combine(activePolicyDir, item.NewAffiliate);

                    if (item.Orig != null && !string.IsNullOrWhiteSpace(item.Orig.OriginalAffiliate))
                    {
                        string oldAffDir = Path.Combine(activePolicyDir, item.Orig.OriginalAffiliate);
                        if (!string.Equals(item.Orig.OriginalAffiliate, item.NewAffiliate, StringComparison.OrdinalIgnoreCase) && Directory.Exists(oldAffDir))
                        {
                            MoveOrMergeDirectory(oldAffDir, targetAffDir);
                            changesCount++;
                        }
                        else if (!Directory.Exists(targetAffDir))
                        {
                            Directory.CreateDirectory(targetAffDir);
                            changesCount++;
                        }

                        if (!string.Equals(item.Orig.OriginalIdNumber ?? "", item.NewIdNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var oldIdFile in Directory.GetFiles(targetAffDir, "ID_*.txt"))
                            {
                                try { File.Delete(oldIdFile); } catch { }
                            }

                            if (!string.IsNullOrWhiteSpace(item.NewIdNumber))
                            {
                                File.WriteAllText(Path.Combine(targetAffDir, $"ID_{item.NewIdNumber}.txt"), item.NewIdNumber);
                            }
                            changesCount++;
                        }
                    }
                    else
                    {
                        EnsureDirectoryHierarchy(item.NewClient, item.NewPolicy, item.NewAffiliate, item.NewIdNumber, item.NewInsurer);
                        changesCount++;
                    }
                }

                SetRegistrosEditMode(false);
                LoadClientFolders();
                PlaySound(SystemSounds.Asterisk);
                MessageBox.Show($"Cambios guardados. ({changesCount} actualización(es) aplicada(s)).", "Registros Actualizados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show($"Error al guardar cambios en las carpetas locales:\n{ex.Message}", "Error de Guardado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Configuración Opciones

        private void SetupSettingsTab()
        {
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
                        sharedGraphClient = null;
                        sharedCredential = null;
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

        private void btnSaveSettings_Click_1(object sender, EventArgs e)
        {
            string newClientPath = txtRootPath != null ? txtRootPath.Text.Trim().Replace('/', '\\') : clientRootPath;

            if (!ValidateOrCreateDirectory(ref newClientPath, "Clientes")) return;

            clientRootPath = newClientPath;

            try
            {
                Properties.Settings.Default.ClientRootPath = clientRootPath;

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
            UpdateRegistrosPathLabel();
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

    public class AuthServiceCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
        }

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            string token = await AuthService.GetAccessTokenAsync(requestContext.Scopes);
            return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(45));
        }
    }
}