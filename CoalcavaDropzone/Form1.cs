using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Win32;
using PdfiumViewer;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace CoalcavaDropzone
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        // Rutas y configuración general
        private string clientRootPath = @"C:\Users\Public\OneDrive - COALCAVA\Clientes\";
        private string dropzoneFlowPath = @"C:\Users\Public\OneDrive - COALCAVA\DROPZONE_FLOW\";
        private List<string> cachedClientList = new List<string>();
        private string selectedFilePath = string.Empty;
        private const string PlaceholderText = "Seleccionar o buscar cliente...";

        // Placeholders para campos de Reembolso
        private const string PlaceholderPoliza = "POL-00000000";
        private const string PlaceholderCedula = "001-0000000-0";
        private const string PlaceholderMonto = "$00.00";
        private const string PlaceholderNotas = "Notas adicionales, diagnósticos o instrucciones de la reclamación...";

        private readonly Color NormalDropzoneColor = Color.FromArgb(248, 248, 246);
        private readonly Color HoverDropzoneColor = Color.FromArgb(254, 243, 235);

        // Estructura interna para los ítems de Reembolso
        public class ReembolsoFileItem
        {
            public string FullPath { get; set; }
            public string FileName => Path.GetFileName(FullPath);
            public string DocumentType { get; set; }
            public long FileSizeBytes => new FileInfo(FullPath).Length;
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
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupDropzoneUI();
            SetupReembolsosUI();
            ConfigureLabelTextSafety();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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

            InitializeSearchAndDropdowns();
            InitializeReembolsosTab();
            SetupSettingsTab();
        }

        #region UI Dropzone Clasificador Normal

        private void SetupDropzoneUI()
        {
            pnlDropzone.AllowDrop = true;
            pbDownloadIcon.AllowDrop = true;
            picPreview.AllowDrop = true;

            pnlDropzone.DragEnter += Dropzone_DragEnter;
            pnlDropzone.DragLeave += Dropzone_DragLeave;
            pnlDropzone.DragDrop += Dropzone_DragDrop;

            pbDownloadIcon.DragEnter += Dropzone_DragEnter;
            pbDownloadIcon.DragLeave += Dropzone_DragLeave;
            pbDownloadIcon.DragDrop += Dropzone_DragDrop;

            picPreview.DragEnter += Dropzone_DragEnter;
            picPreview.DragLeave += Dropzone_DragLeave;
            picPreview.DragDrop += Dropzone_DragDrop;

            pnlDropzone.Click += Dropzone_Click;
            pbDownloadIcon.Click += Dropzone_Click;
            picPreview.Click += Dropzone_Click;

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            pbDownloadIcon.BackColor = Color.Transparent;
            picPreview.BackColor = Color.Transparent;

            ResetDropzone();
        }

        private void ConfigureLabelTextSafety()
        {
            if (lblPreviewName != null)
            {
                lblPreviewName.AutoEllipsis = false;
                lblPreviewName.AutoSize = false;
                lblPreviewName.Width = 385;
                lblPreviewName.Height = 65;
                lblPreviewName.ForeColor = Color.White;
                lblPreviewName.BringToFront();
            }

            if (lblFileName != null)
            {
                lblFileName.AutoSize = false;
                lblFileName.AutoEllipsis = true;
            }
        }

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

        private void Dropzone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                pnlDropzone.BackColor = HoverDropzoneColor;
            }
        }

        private void Dropzone_DragLeave(object sender, EventArgs e)
        {
            pnlDropzone.BackColor = NormalDropzoneColor;
        }

        private void Dropzone_DragDrop(object sender, DragEventArgs e)
        {
            pnlDropzone.BackColor = NormalDropzoneColor;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                string path = files[0];
                if (Directory.Exists(path))
                {
                    var firstFile = Directory.GetFiles(path).FirstOrDefault();
                    if (firstFile != null) ProcessSelectedFile(firstFile);
                }
                else
                {
                    ProcessSelectedFile(path);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList();
                    if (files.Count > 0)
                    {
                        ProcessSelectedFile(files[0]);
                    }
                }
            }
        }

        private void Dropzone_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                openFileDialog.InitialDirectory = Directory.Exists(downloadsPath) ? downloadsPath : @"C:\";
                openFileDialog.Filter = "Documentos e Imágenes (*.pdf;*.png;*.jpg;*.docx;*.xlsx)|*.pdf;*.png;*.jpg;*.jpeg;*.docx;*.xlsx|Todos los Archivos (*.*)|*.*";
                openFileDialog.Title = "Seleccionar Documento";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessSelectedFile(openFileDialog.FileName);
                }
            }
        }

        private void btnQuickGrab_Click_1(object sender, EventArgs e)
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            if (!Directory.Exists(downloadsPath))
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("No se encontró la carpeta de Descargas del sistema.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var latestFile = new DirectoryInfo(downloadsPath)
                .GetFiles()
                .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (latestFile != null)
            {
                ProcessSelectedFile(latestFile.FullName);
            }
            else
            {
                MessageBox.Show("No se encontraron archivos en la carpeta de Descargas.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ProcessSelectedFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            selectedFilePath = filePath;
            lblFileName.Text = Path.GetFileName(filePath);

            pbDownloadIcon.Visible = false;
            picPreview.Visible = true;
            btnClear.Visible = true;
            btnOpenDoc.Visible = true;

            GeneratePreview(filePath);
            UpdateFilenamePreview(null, null);
        }

        public void ResetDropzone()
        {
            selectedFilePath = string.Empty;
            lblFileName.Text = "Ningún archivo seleccionado";

            if (picPreview.Image != null)
            {
                picPreview.Image.Dispose();
                picPreview.Image = null;
            }

            picPreview.Visible = false;
            btnClear.Visible = false;
            btnOpenDoc.Visible = false;

            pbDownloadIcon.Visible = true;
            pnlDropzone.BackColor = NormalDropzoneColor;

            UpdateFilenamePreview(null, null);
        }

        private Bitmap ExtractDocxThumbnail(string filePath)
        {
            try
            {
                string ext = Path.GetExtension(filePath).ToLower();
                if (ext != ".docx" && ext != ".docm") return null;

                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    var entry = archive.Entries.FirstOrDefault(e =>
                        e.FullName.Equals("docProps/thumbnail.jpeg", StringComparison.OrdinalIgnoreCase) ||
                        e.FullName.Equals("docProps/thumbnail.png", StringComparison.OrdinalIgnoreCase));

                    if (entry != null)
                    {
                        using (Stream stream = entry.Open())
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            ms.Position = 0;
                            using (Image tempImg = Image.FromStream(ms))
                            {
                                return new Bitmap(tempImg);
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private void GeneratePreview(string filePath)
        {
            if (picPreview.Image != null)
            {
                var oldImg = picPreview.Image;
                picPreview.Image = null;
                oldImg.Dispose();
            }

            string extension = Path.GetExtension(filePath).ToLower();

            string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
            if (imageExtensions.Contains(extension))
            {
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        ms.Position = 0;
                        using (Image img = Image.FromStream(ms))
                        {
                            picPreview.Image = new Bitmap(img);
                            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
                            picPreview.Visible = true;
                            return;
                        }
                    }
                }
                catch { }
            }

            if (extension == ".pdf")
            {
                try
                {
                    using (var pdfDocument = PdfiumViewer.PdfDocument.Load(filePath))
                    {
                        Bitmap pageImage = (Bitmap)pdfDocument.Render(
                            page: 0,
                            dpiX: 300,
                            dpiY: 300,
                            flags: PdfiumViewer.PdfRenderFlags.CorrectFromDpi
                        );

                        picPreview.Image = pageImage;
                        picPreview.SizeMode = PictureBoxSizeMode.Zoom;
                        picPreview.Visible = true;
                        return;
                    }
                }
                catch { }
            }

            if (extension == ".docx" || extension == ".docm")
            {
                Bitmap docxThumb = ExtractDocxThumbnail(filePath);
                if (docxThumb != null)
                {
                    picPreview.Image = docxThumb;
                    picPreview.SizeMode = PictureBoxSizeMode.Zoom;
                    picPreview.Visible = true;
                    return;
                }
            }

            try
            {
                using (Icon fileIcon = Icon.ExtractAssociatedIcon(filePath))
                {
                    if (fileIcon != null)
                    {
                        picPreview.Image = fileIcon.ToBitmap();
                        picPreview.SizeMode = PictureBoxSizeMode.CenterImage;
                        picPreview.Visible = true;
                        return;
                    }
                }
            }
            catch { }

            picPreview.Visible = false;
            pbDownloadIcon.Visible = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ResetDropzone();
        }

        private void btnOpenDoc_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                MessageBox.Show("No hay ningún archivo seleccionado para abrir.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = selectedFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el archivo: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Búsqueda y Clasificación Normal

        private void InitializeSearchAndDropdowns()
        {
            LoadClientFolders();

            txtClientSearch.AutoCompleteMode = AutoCompleteMode.None;
            ApplyPlaceholder(txtClientSearch, PlaceholderText);

            txtClientSearch.LostFocus += (s, e) => {
                Timer focusTimer = new Timer { Interval = 150 };
                focusTimer.Tick += (st, et) => {
                    focusTimer.Stop();
                    if (!lstClientSuggestions.Focused && !txtClientSearch.Focused)
                    {
                        lstClientSuggestions.Visible = false;
                    }
                };
                focusTimer.Start();
            };

            txtClientSearch.TextChanged += TxtClientSearch_TextChanged;
            txtClientSearch.KeyDown += TxtClientSearch_KeyDown;
            lstClientSuggestions.Click += LstClientSuggestions_Click;
            lstClientSuggestions.KeyDown += LstClientSuggestions_KeyDown;

            cmbDepartment.Items.Clear();
            cmbDepartment.Items.AddRange(new string[] {
                "Documentos / KYC",
                "Personas",
                "Riesgos Generales",
                "Fianzas",
                "Reclamaciones",
                "Cobros"
            });

            cmbDepartment.SelectedIndexChanged += cmbDepartment_SelectedIndexChanged;
            if (cmbDepartment.Items.Count > 0) cmbDepartment.SelectedIndex = 0;

            dtpDocDate.Value = DateTime.Now;

            txtClientSearch.TextChanged += UpdateFilenamePreview;
            cmbDocType.SelectedIndexChanged += UpdateFilenamePreview;
            cmbDocType.TextChanged += UpdateFilenamePreview;
            dtpDocDate.ValueChanged += UpdateFilenamePreview;

            if (chkIncludeDay != null)
            {
                chkIncludeDay.CheckedChanged += UpdateFilenamePreview;
            }

            UpdateFilenamePreview(null, null);
        }

        private void cmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateDocTypesForDepartment();
            UpdateFilenamePreview(null, null);
        }

        private void PopulateDocTypesForDepartment()
        {
            string selectedDept = cmbDepartment.SelectedItem?.ToString() ?? "";
            cmbDocType.Items.Clear();

            switch (selectedDept)
            {
                case "Personas":
                    cmbDocType.Items.AddRange(new string[] {
                        "Factura", "Recibo", "Historial Clínico", "Resultado de Estudios",
                        "Indicación Médica", "Póliza", "Cotización"
                    });
                    break;
                case "Documentos / KYC":
                    cmbDocType.Items.AddRange(new string[] { "KYC", "Cédula", "RNC", "Mercantil", "Acta" });
                    break;
                case "Cobros":
                    cmbDocType.Items.AddRange(new string[] { "Factura", "Recibo", "Estado de Cuenta", "Remisión" });
                    break;
                case "Reclamaciones":
                    cmbDocType.Items.AddRange(new string[] { "Reclamación", "Factura", "Informe", "Ajuste" });
                    break;
                default:
                    cmbDocType.Items.AddRange(new string[] { "Póliza", "Cotización", "Factura", "Marbete", "Endoso", "Remisión" });
                    break;
            }

            if (cmbDocType.Items.Count > 0) cmbDocType.SelectedIndex = 0;
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

        private void TxtClientSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtClientSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(query) || query == PlaceholderText)
            {
                lstClientSuggestions.Visible = false;
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
                lstClientSuggestions.DataSource = matches;
                lstClientSuggestions.Visible = true;
                lstClientSuggestions.BringToFront();
            }
            else
            {
                lstClientSuggestions.Visible = false;
            }
        }

        private void TxtClientSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstClientSuggestions.Visible && lstClientSuggestions.Items.Count > 0)
            {
                lstClientSuggestions.Focus();
                lstClientSuggestions.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && lstClientSuggestions.Visible && lstClientSuggestions.Items.Count > 0)
            {
                SelectClientFromList();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LstClientSuggestions_Click(object sender, EventArgs e) => SelectClientFromList();

        private void LstClientSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectClientFromList();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                lstClientSuggestions.Visible = false;
                txtClientSearch.Focus();
            }
        }

        private void SelectClientFromList()
        {
            if (lstClientSuggestions.SelectedItem != null)
            {
                txtClientSearch.Text = lstClientSuggestions.SelectedItem.ToString();
                txtClientSearch.ForeColor = Color.Black;
                txtClientSearch.SelectionStart = txtClientSearch.Text.Length;
                lstClientSuggestions.Visible = false;
                txtClientSearch.Focus();
            }
        }

        private string GetDepartmentCode(string department)
        {
            switch (department)
            {
                case "Documentos / KYC": return "KY";
                case "Personas": return "PE";
                case "Riesgos Generales": return "RG";
                case "Fianzas": return "FI";
                case "Reclamaciones": return "RC";
                case "Cobros": return "CB";
                default: return "GE";
            }
        }

        private string GetShortDocTypeCode(string docType)
        {
            if (string.IsNullOrWhiteSpace(docType)) return "Doc";
            switch (docType.Trim())
            {
                case "Historial Clínico": return "HisClin";
                case "Resultado de Estudios": return "Estudios";
                case "Indicación Médica": return "IndMed";
                case "Cotización": return "Cotizacion";
                case "Póliza": return "Poliza";
                case "Reclamación": return "Reclamacion";
                case "Estado de Cuenta": return "EdoCuenta";
                default: return docType.Trim().Replace(" ", "");
            }
        }

        private string GetSubfolderForDepartment(string department)
        {
            switch (department)
            {
                case "Documentos / KYC": return "Documentos Legales";
                case "Riesgos Generales": return "Riesgos Generales";
                case "Personas":
                case "Salud":
                case "Vida": return "Personas";
                case "Fianzas":
                case "Reclamaciones": return "Fianzas y Reclamaciones";
                case "Cobros": return "Cobros";
                default: return "Polizas";
            }
        }

        private void UpdateFilenamePreview(object sender, EventArgs e)
        {
            string rawClient = txtClientSearch.Text.Trim();
            bool hasValidClient = !string.IsNullOrWhiteSpace(rawClient) && rawClient != PlaceholderText;
            string clientName = hasValidClient ? rawClient : "[Cliente]";

            string department = cmbDepartment.SelectedItem?.ToString() ?? "";
            string rawDocType = string.IsNullOrWhiteSpace(cmbDocType.Text) ? "Doc" : cmbDocType.Text;
            string docType = GetShortDocTypeCode(rawDocType);

            bool includeDay = chkIncludeDay != null && chkIncludeDay.Checked;
            string formattedDate = includeDay
                ? dtpDocDate.Value.ToString("dd_MMM_yyyy")
                : dtpDocDate.Value.ToString("MMM_yyyy");

            string deptCode = GetDepartmentCode(department);
            string fileExt = string.IsNullOrEmpty(selectedFilePath) ? ".pdf" : Path.GetExtension(selectedFilePath);

            string subfolderName = GetSubfolderForDepartment(department);
            string destinationFolder = $"Clientes/{clientName}/{subfolderName}";
            string generatedFileName = $"{deptCode}_{docType}_{formattedDate}{fileExt}";

            if (lblPreviewName != null)
            {
                lblPreviewName.Text = $"📁 Ruta: {destinationFolder}\n📄 Archivo: {generatedFileName}";
            }
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Por favor selecciona o arrastra un archivo primero.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string clientName = txtClientSearch.Text.Trim();
            if (string.IsNullOrEmpty(clientName) || clientName == PlaceholderText)
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Por favor ingresa o selecciona un cliente.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientSearch.Focus();
                return;
            }

            string clientFolderPath = Path.Combine(clientRootPath, clientName);

            if (!Directory.Exists(clientFolderPath))
            {
                PlaySound(SystemSounds.Hand);
                for (int i = 0; i < 3; i++)
                {
                    FlashWindow(this.Handle, true);
                    System.Threading.Thread.Sleep(80);
                }

                DialogResult confirmResult = MessageBox.Show(
                    $"El cliente \"{clientName}\" no existe en la carpeta raíz.\n\n" +
                    $"Ruta solicitada:\n{clientFolderPath}\n\n" +
                    "¿Desea crear esta nueva carpeta de cliente y continuar?",
                    "Cliente no encontrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                );

                if (confirmResult == DialogResult.No) return;
            }

            string department = cmbDepartment.SelectedItem?.ToString();
            string rawDocType = string.IsNullOrWhiteSpace(cmbDocType.Text) ? "Doc" : cmbDocType.Text;
            string docType = GetShortDocTypeCode(rawDocType);

            bool includeDay = chkIncludeDay != null && chkIncludeDay.Checked;
            string formattedDate = includeDay
                ? dtpDocDate.Value.ToString("dd_MMM_yyyy")
                : dtpDocDate.Value.ToString("MMM_yyyy");

            string deptCode = GetDepartmentCode(department);
            string fileExtension = Path.GetExtension(selectedFilePath);

            string subfolderName = GetSubfolderForDepartment(department);
            string targetDirectory = Path.Combine(clientFolderPath, subfolderName);

            try
            {
                if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

                string baseFileName = $"{deptCode}_{docType}_{formattedDate}";
                string newFileName = $"{baseFileName}{fileExtension}";
                string destinationPath = Path.Combine(targetDirectory, newFileName);

                if (File.Exists(destinationPath))
                {
                    int counter = 1;
                    while (File.Exists(destinationPath))
                    {
                        newFileName = $"{baseFileName} ({counter}){fileExtension}";
                        destinationPath = Path.Combine(targetDirectory, newFileName);
                        counter++;
                    }
                }

                File.Move(selectedFilePath, destinationPath);

                PlaySound(SystemSounds.Asterisk);
                MessageBox.Show($"Archivo organizado con éxito.\n\nUbicación:\n{destinationPath}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadClientFolders();
                ResetDropzone();
            }
            catch (Exception ex)
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show($"Error al mover el archivo: {ex.Message}", "Error de Operación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Reembolsos (Salud & Vida) Implementation

        // Helper reutilizable para Placeholders
        private void ApplyPlaceholder(TextBox txt, string placeholder)
        {
            if (txt == null) return;

            txt.Tag = placeholder;
            if (string.IsNullOrWhiteSpace(txt.Text) || txt.Text == placeholder)
            {
                txt.Text = placeholder;
                txt.ForeColor = Color.Gray;
            }

            txt.GotFocus += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.Black;
                }
            };

            txt.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };
        }

        private string GetCleanText(TextBox txt)
        {
            if (txt == null) return string.Empty;
            string placeholder = txt.Tag?.ToString() ?? "";
            string val = txt.Text.Trim();
            return (val == placeholder) ? string.Empty : val;
        }

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

            // Configurar DataGridView interactivo
            dgvReembolsoFiles.AutoGenerateColumns = false;
            dgvReembolsoFiles.AllowUserToAddRows = false;
            dgvReembolsoFiles.RowHeadersVisible = false;
            dgvReembolsoFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReembolsoFiles.MultiSelect = false;

            dgvReembolsoFiles.Columns.Clear();

            // Columna 1: Enlace con el nombre de archivo
            DataGridViewLinkColumn colName = new DataGridViewLinkColumn
            {
                HeaderText = "Archivo",
                DataPropertyName = "FileName",
                Name = "colName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                LinkBehavior = LinkBehavior.HoverUnderline,
                LinkColor = Color.FromArgb(0, 102, 204),
                TrackVisitedState = false
            };
            dgvReembolsoFiles.Columns.Add(colName);

            // Columna 2: Tipo de Documento (ComboBox por fila)
            DataGridViewComboBoxColumn colType = new DataGridViewComboBoxColumn
            {
                HeaderText = "Tipo de Documento",
                Name = "colType",
                Width = 200,
                FlatStyle = FlatStyle.Flat
            };
            colType.Items.AddRange(docTypeOptions);
            dgvReembolsoFiles.Columns.Add(colType);

            // Columna 3: Tamaño
            DataGridViewTextBoxColumn colSize = new DataGridViewTextBoxColumn
            {
                HeaderText = "Tamaño",
                DataPropertyName = "FileSizeFormatted",
                Name = "colSize",
                Width = 90,
                ReadOnly = true
            };
            dgvReembolsoFiles.Columns.Add(colSize);

            // Eventos del DataGridView
            dgvReembolsoFiles.CellContentClick += DgvReembolsoFiles_CellContentClick;
            dgvReembolsoFiles.CellValueChanged += DgvReembolsoFiles_CellValueChanged;
            dgvReembolsoFiles.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvReembolsoFiles.IsCurrentCellDirty)
                {
                    dgvReembolsoFiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            // Botones de acción
            btnRemoveSelectedFile.Click += BtnRemoveSelectedFile_Click;
            btnClearReembolso.Click += BtnClearReembolso_Click;
            btnProcessReembolso.Click += BtnProcessReembolso_Click;

            // Valores por defecto
            rbSaveOriginalsToFolder.Checked = true;
        }

        private void InitializeReembolsosTab()
        {
            // Cargar Aseguradoras comunes
            cmbReembolsoAseguradora.Items.Clear();
            cmbReembolsoAseguradora.Items.AddRange(new string[] {
                "Sura",
                "Humano",
                "Universal",
                "La Colonial",
                "Mapfre",
                "SeNaSa",
                "Cigna",
                "Bupa",
                "BMI",
                "One Alliance",
                "Aetna",
                "American Fidelity International",
                "Yunen",
                "WorldWide Medical",
                "Primera",
                "Aurora",
                "APS",
                "COOPSEGUROS",
                "Crecer",
                "La Monumental",
                "Seguros Reservas",
                "Otra Aseguradora"
            });
            if (cmbReembolsoAseguradora.Items.Count > 0) cmbReembolsoAseguradora.SelectedIndex = 0;

            // Búsqueda de cliente en tab Reembolsos
            txtReembolsoClientSearch.AutoCompleteMode = AutoCompleteMode.None;
            ApplyPlaceholder(txtReembolsoClientSearch, PlaceholderText);

            // Placeholders para el resto de los campos de texto
            ApplyPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            ApplyPlaceholder(txtReembolsoCedula, PlaceholderCedula);
            ApplyPlaceholder(txtReembolsoMonto, PlaceholderMonto);
            ApplyPlaceholder(txtReembolsosNotasAdicionales, PlaceholderNotas);

            txtReembolsoClientSearch.LostFocus += (s, e) =>
            {
                Timer focusTimer = new Timer { Interval = 150 };
                focusTimer.Tick += (st, et) =>
                {
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
                ofd.Filter = "Documentos PDF (*.pdf)|*.pdf";
                ofd.Multiselect = true;
                ofd.Title = "Seleccionar documentos para Reembolso";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    AddFilesToReembolsoQueue(ofd.FileNames);
                }
            }
        }

        private void AddFilesToReembolsoQueue(string[] filePaths)
        {
            int addedCount = 0;
            foreach (string path in filePaths)
            {
                if (File.Exists(path) && Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
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

        private void DgvReembolsoFiles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvReembolsoFiles.Columns["colType"].Index)
            {
                var item = dgvReembolsoFiles.Rows[e.RowIndex].Tag as ReembolsoFileItem;
                if (item != null)
                {
                    item.DocumentType = dgvReembolsoFiles.Rows[e.RowIndex].Cells["colType"].Value?.ToString() ?? "Otros";
                }
            }
        }

        private void BtnRemoveSelectedFile_Click(object sender, EventArgs e)
        {
            if (dgvReembolsoFiles.SelectedRows.Count > 0)
            {
                var selectedRow = dgvReembolsoFiles.SelectedRows[0];
                var item = selectedRow.Tag as ReembolsoFileItem;
                if (item != null)
                {
                    reembolsoItems.Remove(item);
                    RefreshReembolsosGrid();
                }
            }
            else
            {
                MessageBox.Show("Seleccione una fila para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnClearReembolso_Click(object sender, EventArgs e)
        {
            if (reembolsoItems.Count == 0 && string.IsNullOrWhiteSpace(GetCleanText(txtReembolsoPoliza))) return;

            DialogResult confirm = MessageBox.Show(
                "¿Estás seguro de que deseas limpiar la lista de documentos y todos los campos de este reembolso?",
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

            ApplyPlaceholder(txtReembolsoPoliza, PlaceholderPoliza);
            ApplyPlaceholder(txtReembolsoCedula, PlaceholderCedula);
            ApplyPlaceholder(txtReembolsoMonto, PlaceholderMonto);
            ApplyPlaceholder(txtReembolsosNotasAdicionales, PlaceholderNotas);
            ApplyPlaceholder(txtReembolsoClientSearch, PlaceholderText);

            if (lblReembolsoDropPrompt != null)
            {
                lblReembolsoDropPrompt.Text = "Arrastra todos los archivos PDF del caso aquí";
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
            }
        }

        // Ejecución Principal: Merge + Metadata + JSON Trigger
        private void BtnProcessReembolso_Click(object sender, EventArgs e)
        {
            string clientName = GetCleanText(txtReembolsoClientSearch);
            string policyNumber = GetCleanText(txtReembolsoPoliza);
            string cedula = GetCleanText(txtReembolsoCedula);
            string insurer = cmbReembolsoAseguradora.SelectedItem?.ToString() ?? "Aseguradora";
            string amount = GetCleanText(txtReembolsoMonto);
            string notes = GetCleanText(txtReembolsosNotasAdicionales);

            if (reembolsoItems.Count == 0)
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Debe agregar al menos un documento PDF al caso de reembolso.", "Faltan Archivos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(clientName))
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Por favor indique o seleccione un cliente válido.", "Cliente Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReembolsoClientSearch.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(policyNumber))
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Por favor introduzca el número de póliza del cliente.", "Póliza Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReembolsoPoliza.Focus();
                return;
            }

            string safeClient = string.Concat(clientName.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
            string safePolicy = string.Concat(policyNumber.Split(Path.GetInvalidFileNameChars())).Replace(" ", "");
            string timestamp = DateTime.Now.ToString("dd_MMM_yyyy_HHmm");

            List<string> presentDocTypes = reembolsoItems.Select(x => x.DocumentType).Distinct().ToList();

            string finalPdfName = $"REEMBOLSO_{safeClient}_{safePolicy}_{timestamp}.pdf";
            string finalJsonName = $"REEMBOLSO_{safeClient}_{safePolicy}_{timestamp}.json";

            string finalPdfLocalPath = Path.Combine(dropzoneFlowPath, finalPdfName);
            string finalJsonLocalPath = Path.Combine(dropzoneFlowPath, finalJsonName);

            try
            {
                if (!Directory.Exists(dropzoneFlowPath))
                {
                    Directory.CreateDirectory(dropzoneFlowPath);
                }

                // 1. Consolidación de PDFs y Metadata
                using (PdfSharp.Pdf.PdfDocument outputDoc = new PdfSharp.Pdf.PdfDocument())
                {
                    outputDoc.Info.Title = $"Expediente Reembolso - {clientName}";
                    outputDoc.Info.Author = "Coalcava Dropzone Suite";
                    outputDoc.Info.Subject = $"Póliza: {policyNumber} | Aseguradora: {insurer} | Cédula: {cedula}";
                    outputDoc.Info.Keywords = $"Reembolso, {clientName}, {policyNumber}, {insurer}, {string.Join(", ", presentDocTypes)}";

                    foreach (var item in reembolsoItems)
                    {
                        if (!File.Exists(item.FullPath)) continue;

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

                    outputDoc.Save(finalPdfLocalPath);
                }

                // 2. Generación del Schema JSON para Planner con notas adicionales
                var plannerPayload = new
                {
                    ClientName = clientName,
                    PolicyNumber = policyNumber,
                    Insurer = insurer,
                    Cedula = string.IsNullOrWhiteSpace(cedula) ? "No indicada" : cedula,
                    ClaimAmount = string.IsNullOrWhiteSpace(amount) ? "No especificado" : amount,
                    AdditionalNotes = string.IsNullOrWhiteSpace(notes) ? "Sin notas adicionales." : notes,
                    DocumentTypes = presentDocTypes,
                    SourceFilesCount = reembolsoItems.Count,
                    GeneratedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    PdfFileName = finalPdfName
                };

                string jsonContent = JsonSerializer.Serialize(plannerPayload, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(finalJsonLocalPath, jsonContent);

                // 3. Manejo de originales
                if (rbSaveOriginalsToFolder.Checked)
                {
                    string clientReembolsoDir = Path.Combine(clientRootPath, clientName, "Reembolsos", $"{DateTime.Now:yyyy-MM-dd}_{safePolicy}");
                    if (!Directory.Exists(clientReembolsoDir)) Directory.CreateDirectory(clientReembolsoDir);

                    foreach (var item in reembolsoItems)
                    {
                        if (File.Exists(item.FullPath))
                        {
                            string dest = Path.Combine(clientReembolsoDir, $"{item.DocumentType.Replace(" ", "_")}_{Path.GetFileName(item.FullPath)}");
                            File.Copy(item.FullPath, dest, true);
                        }
                    }

                    File.Copy(finalPdfLocalPath, Path.Combine(clientReembolsoDir, finalPdfName), true);
                }
                else if (rbDeleteOriginals.Checked)
                {
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
                }

                PlaySound(SystemSounds.Asterisk);
                MessageBox.Show(
                    $"Expediente generado exitosamente:\n\n" +
                    $"📄 PDF: {finalPdfName}\n" +
                    $"⚙️ Metadatos: {finalJsonName}\n" +
                    $"📁 Destino: {dropzoneFlowPath}\n\n" +
                    $"Power Automate procesará la tarea en Planner.",
                    "Expediente Procesado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                LoadClientFolders();
                ClearReembolsosForm();
            }
            catch (Exception ex)
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show($"Ocurrió un error al procesar el reembolso:\n{ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Validación de carpeta de Clientes
            if (!ValidateOrCreateDirectory(ref newClientPath, "Clientes")) return;

            // Validación de carpeta de Reembolsos
            if (!ValidateOrCreateDirectory(ref newReembolsoPath, "Reembolsos Flow")) return;

            clientRootPath = newClientPath;
            dropzoneFlowPath = newReembolsoPath;

            try
            {
                Properties.Settings.Default.ClientRootPath = clientRootPath;
                Properties.Settings.Default.ReembolsoRootPath = dropzoneFlowPath;

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

            if (tabControlMain != null && tabPageDropzone != null)
            {
                tabControlMain.SelectedTab = tabPageDropzone;
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

        private void rbDeleteOriginals_CheckedChanged(object sender, EventArgs e) { }
        private void rbSaveOriginalsToFolder_CheckedChanged(object sender, EventArgs e) { }
    }
}