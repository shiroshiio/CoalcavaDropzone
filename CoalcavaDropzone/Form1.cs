using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32; // required for windows startup registry
using PdfiumViewer;

namespace CoalcavaDropzone
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        private string clientRootPath = @"C:\Users\Public\OneDrive - COALCAVA\Clientes\";
        private List<string> cachedClientList = new List<string>();
        private string selectedFilePath = string.Empty;
        private const string PlaceholderText = "Seleccionar o buscar cliente...";

        private readonly Color NormalDropzoneColor = Color.FromArgb(248, 248, 246);
        private readonly Color HoverDropzoneColor = Color.FromArgb(254, 243, 235);

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupDropzoneUI();
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
            }
            catch
            {
                // fallback if settings wrapper unconfigured
            }

            clientRootPath = clientRootPath.Replace('/', '\\');

            if (txtRootPath != null)
            {
                txtRootPath.Text = clientRootPath;
            }

            InitializeSearchAndDropdowns();
        }

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

        // --- MUTE SOUND HELPER ---
        private void PlaySound(SystemSound sound)
        {
            bool isMuted = chkMuteSound != null && chkMuteSound.Checked;
            if (!isMuted && sound != null)
            {
                sound.Play();
            }
        }

        // --- STARTUP REGISTRY HELPER ---
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
                System.Diagnostics.Debug.WriteLine($"Failed to update startup registry: {ex.Message}");
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
            catch
            {
                // fallback gracefully
            }
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

            // 1. Native image preview
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Image preview failed: {ex.Message}");
                }
            }

            // 2. Pdfium render for PDFs
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PDFium failed: {ex.Message}");
                }
            }

            // 3. OpenXML embedded thumbnail for Word
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

            // 4. Windows shell thumbnail provider
            try
            {
                Bitmap thumbnail = WindowsThumbnailProvider.GetThumbnail(filePath, 300, 400);
                if (thumbnail != null)
                {
                    picPreview.Image = thumbnail;
                    picPreview.SizeMode = PictureBoxSizeMode.Zoom;
                    picPreview.Visible = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shell thumbnail failed: {ex.Message}");
            }

            // 5. System icon fallback
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Icon extraction failed: {ex.Message}");
            }

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
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
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

        private void InitializeSearchAndDropdowns()
        {
            LoadClientFolders();

            txtClientSearch.AutoCompleteMode = AutoCompleteMode.None;

            txtClientSearch.Text = PlaceholderText;
            txtClientSearch.ForeColor = Color.Gray;

            txtClientSearch.GotFocus += (s, e) => {
                if (txtClientSearch.Text == PlaceholderText)
                {
                    txtClientSearch.Text = "";
                    txtClientSearch.ForeColor = Color.Black;
                }
            };

            txtClientSearch.LostFocus += (s, e) => {
                Timer focusTimer = new Timer { Interval = 150 };
                focusTimer.Tick += (st, et) => {
                    focusTimer.Stop();
                    if (!lstClientSuggestions.Focused && !txtClientSearch.Focused)
                    {
                        if (string.IsNullOrWhiteSpace(txtClientSearch.Text))
                        {
                            txtClientSearch.Text = PlaceholderText;
                            txtClientSearch.ForeColor = Color.Gray;
                        }

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

            // --- LOAD SAVED OPTIONS CHECKBOX STATES ---
            try
            {
                if (chkMuteSound != null)
                {
                    chkMuteSound.Checked = Properties.Settings.Default.IsMuted;
                }

                if (chkOpenOnStartup != null)
                {
                    chkOpenOnStartup.Checked = Properties.Settings.Default.OpenOnStartup;
                }
            }
            catch
            {
                // ignore unconfigured settings
            }

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
                        "Factura",
                        "Recibo",
                        "Historial Clínico",
                        "Resultado de Estudios",
                        "Indicación Médica",
                        "Póliza",
                        "Cotización"
                    });
                    break;

                case "Documentos / KYC":
                    cmbDocType.Items.AddRange(new string[] {
                        "KYC",
                        "Cédula",
                        "RNC",
                        "Mercantil",
                        "Acta"
                    });
                    break;

                case "Cobros":
                    cmbDocType.Items.AddRange(new string[] {
                        "Factura",
                        "Recibo",
                        "Estado de Cuenta",
                        "Remisión"
                    });
                    break;

                case "Reclamaciones":
                    cmbDocType.Items.AddRange(new string[] {
                        "Reclamación",
                        "Factura",
                        "Informe",
                        "Ajuste"
                    });
                    break;

                default:
                    cmbDocType.Items.AddRange(new string[] {
                        "Póliza",
                        "Cotización",
                        "Factura",
                        "Marbete",
                        "Endoso",
                        "Remisión"
                    });
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

        private void LstClientSuggestions_Click(object sender, EventArgs e)
        {
            SelectClientFromList();
        }

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
                case "Documentos / KYC":
                    return "Documentos Legales";

                case "Riesgos Generales":
                    return "Riesgos Generales";

                case "Personas":
                case "Salud":
                case "Vida":
                    return "Personas";

                case "Fianzas":
                case "Reclamaciones":
                    return "Fianzas y Reclamaciones";

                case "Cobros":
                    return "Cobros";

                default:
                    return "Polizas";
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

            // National Standard Date Format: dd_MMM_yyyy or MMM_yyyy
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
                MessageBox.Show("Por favor selecciona o arrastra un archivo primero.",
                                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string clientName = txtClientSearch.Text.Trim();
            if (string.IsNullOrEmpty(clientName) || clientName == PlaceholderText)
            {
                PlaySound(SystemSounds.Exclamation);
                MessageBox.Show("Por favor ingresa o selecciona un cliente.",
                                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    $"El cliente \"{clientName}\" No existe en la carpeta raíz.\n\n" +
                    $"Ruta solicitada:\n{clientFolderPath}\n\n" +
                    "¿Desea crear esta nueva carpeta de cliente y continuar con el guardado?",
                    "Cliente no encontrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                );

                if (confirmResult == DialogResult.No)
                {
                    return;
                }
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
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

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
                MessageBox.Show($"Archivo organizado con éxito. :)\n\nUbicación:\n{destinationPath}",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadClientFolders();
                ResetDropzone();
            }
            catch (Exception ex)
            {
                PlaySound(SystemSounds.Hand);
                MessageBox.Show($"Error al mover el archivo: {ex.Message}",
                                "Error de Operación", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnSaveSettings_Click_1(object sender, EventArgs e)
        {
            string newPath = txtRootPath.Text.Trim().Replace('/', '\\');

            if (!Directory.Exists(newPath))
            {
                var result = MessageBox.Show(
                    "La carpeta especificada no existe en este equipo. ¿Desea crearla ahora?",
                    "Confirmar Ruta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"No se pudo crear la carpeta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            if (!newPath.EndsWith(@"\")) newPath += @"\";

            clientRootPath = newPath;

            // --- SAVE PREFERENCES & APPLY STARTUP REGISTRY ---
            try
            {
                Properties.Settings.Default.ClientRootPath = clientRootPath;

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
            catch
            {
                // ignore unconfigured settings wrapper
            }

            LoadClientFolders();

            MessageBox.Show("Configuración guardada correctamente.", "Opciones", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (tabControlMain != null && tabPageDropzone != null)
            {
                tabControlMain.SelectedTab = tabPageDropzone;
            }
        }
    }
}