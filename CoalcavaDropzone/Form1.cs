using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PdfiumViewer;

namespace CoalcavaDropzone
{
    public partial class Form1 : Form
    {
        // win32 api for flashing window frame on safety warning
        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        private string clientRootPath = @"C:\Users\Public\OneDrive - COALCAVA\Clientes\";
        private List<string> cachedClientList = new List<string>();
        private string selectedFilePath = string.Empty;
        private const string PlaceholderText = "Seleccionar o buscar cliente...";

        // theme color palette
        private readonly Color NormalDropzoneColor = Color.FromArgb(248, 248, 246);
        private readonly Color HoverDropzoneColor = Color.FromArgb(254, 243, 235);

        public Form1()
        {
            InitializeComponent();
            SetupDropzoneUI();
            ConfigureLabelTextSafety();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // load saved path from user settings if available
            try
            {
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ClientRootPath))
                {
                    clientRootPath = Properties.Settings.Default.ClientRootPath;
                }
            }
            catch
            {
                // fallback to default if settings wrapper is unconfigured
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
            // enable drag and drop on dropzone panel, resting controls, AND active preview picturebox
            pnlDropzone.AllowDrop = true;
            pbDownloadIcon.AllowDrop = true;
            picPreview.AllowDrop = true; // allows dropping files directly over the active thumbnail

            // route drag & drop events
            pnlDropzone.DragEnter += Dropzone_DragEnter;
            pnlDropzone.DragLeave += Dropzone_DragLeave;
            pnlDropzone.DragDrop += Dropzone_DragDrop;

            pbDownloadIcon.DragEnter += Dropzone_DragEnter;
            pbDownloadIcon.DragLeave += Dropzone_DragLeave;
            pbDownloadIcon.DragDrop += Dropzone_DragDrop;

            picPreview.DragEnter += Dropzone_DragEnter;
            picPreview.DragLeave += Dropzone_DragLeave;
            picPreview.DragDrop += Dropzone_DragDrop;

            // route clicks to file browser (clicking picpreview re-opens file browser)
            pnlDropzone.Click += Dropzone_Click;
            pbDownloadIcon.Click += Dropzone_Click;
            picPreview.Click += Dropzone_Click; // click'-'through behavior to browse new file

            // enable form keypreview for ctrl+v clipboard paste
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // transparent background on resting elements
            pbDownloadIcon.BackColor = Color.Transparent;
            picPreview.BackColor = Color.Transparent;

            ResetDropzone();
        }

        private void ConfigureLabelTextSafety()
        {
            if (lblPreviewName != null)
            {
                // 1. Disable AutoEllipsis so text wraps instead of ending in "..."
                lblPreviewName.AutoEllipsis = false;
                lblPreviewName.AutoSize = false;

                // 2. Set explicit width and enough height for 3-4 wrapped lines
                // (Adjust 320 to match the width of your right-side dark panel)
                lblPreviewName.Width = 385;
                lblPreviewName.Height = 65;

                // 3. Crisp white text against the dark gray panel
                lblPreviewName.ForeColor = Color.White;

                // 4. Ensure it sits on top of panel layers
                lblPreviewName.BringToFront();
            }

            if (lblFileName != null)
            {
                lblFileName.AutoSize = false;
                lblFileName.AutoEllipsis = true; // Keep ellipsis on file name so it stays clean
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
                SystemSounds.Exclamation.Play();
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

        // generates shell thumbnail for pdf, docx, xlsx, png, jpg or extracts system icon fallback
        private void GeneratePreview(string filePath)
        {
            try
            {
                // 1. Dispose old image to free up memory/file locks
                if (picPreview.Image != null)
                {
                    var oldImg = picPreview.Image;
                    picPreview.Image = null;
                    oldImg.Dispose();
                }

                string extension = Path.GetExtension(filePath).ToLower();

                // 2. HIGH-RES RENDER FOR PDFS VIA PDFIUM
                if (extension == ".pdf")
                {
                    using (var pdfDocument = PdfiumViewer.PdfDocument.Load(filePath))
                    {
                        // Render the first page at 300 DPI for sharp text
                        // Adjust render dimensions to match your PictureBox box ratio
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

                // 3. FALLBACK FOR IMAGES & OTHER DOCUMENTS (DOCX, XLSX, PNG, JPG)
                Bitmap thumbnail = WindowsThumbnailProvider.GetThumbnail(filePath, 300, 400);

                if (thumbnail != null)
                {
                    picPreview.Image = thumbnail;
                    picPreview.SizeMode = PictureBoxSizeMode.Zoom;
                    picPreview.Visible = true;
                }
                else
                {
                    // 4. FALLBACK TO SYSTEM ASSOCIATED FILE ICON
                    using (Icon fileIcon = Icon.ExtractAssociatedIcon(filePath))
                    {
                        if (fileIcon != null)
                        {
                            picPreview.Image = fileIcon.ToBitmap();
                            picPreview.SizeMode = PictureBoxSizeMode.CenterImage;
                            picPreview.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If PDFium or thumbnail fails, hide preview and restore default download icon
                System.Diagnostics.Debug.WriteLine($"Preview error: {ex.Message}");
                picPreview.Visible = false;
                pbDownloadIcon.Visible = true;
            }
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

            // set placeholder behavior with text to indicate instructions
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

            // dynamic listbox search listeners
            txtClientSearch.TextChanged += TxtClientSearch_TextChanged;
            txtClientSearch.KeyDown += TxtClientSearch_KeyDown;
            lstClientSuggestions.Click += LstClientSuggestions_Click;
            lstClientSuggestions.KeyDown += LstClientSuggestions_KeyDown;

            // populate departments
            cmbDepartment.Items.Clear();
            cmbDepartment.Items.AddRange(new string[] {
                "Documentos / KYC",
                "Personas",
                "Riesgos Generales",
                "Fianzas",
                "Reclamaciones",
                "Cobros"
            });

            // wire dynamic update of document types based on selected department
            cmbDepartment.SelectedIndexChanged += cmbDepartment_SelectedIndexChanged;

            if (cmbDepartment.Items.Count > 0) cmbDepartment.SelectedIndex = 0;

            dtpDocDate.Value = DateTime.Now;

            txtClientSearch.TextChanged += UpdateFilenamePreview;
            cmbDocType.SelectedIndexChanged += UpdateFilenamePreview;
            cmbDocType.TextChanged += UpdateFilenamePreview; // catches typed input
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

        // document types switch based on department
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

                default: // riesgos generales, fianzas, fallback
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

            // substring matching: prefix first then inner match
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

        // safe short doc code formatting (handles nulls and custom user text)
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

            // safely handle typed or selected combobox input
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

            lblPreviewName.Text = $"📁 Ruta: {destinationFolder}\n📄 Archivo: {generatedFileName}";
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Por favor selecciona o arrastra un archivo primero.",
                                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string clientName = txtClientSearch.Text.Trim();
            if (string.IsNullOrEmpty(clientName) || clientName == PlaceholderText)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Por favor ingresa o selecciona un cliente.",
                                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientSearch.Focus();
                return;
            }

            // security check: client folder existence with sound and flash
            string clientFolderPath = Path.Combine(clientRootPath, clientName);

            if (!Directory.Exists(clientFolderPath))
            {
                SystemSounds.Hand.Play();

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

            // safely read typed or selected document type
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

                // base filename structure: DEPT_DocType_Date.ext
                string baseFileName = $"{deptCode}_{docType}_{formattedDate}";
                string newFileName = $"{baseFileName}{fileExtension}";
                string destinationPath = Path.Combine(targetDirectory, newFileName);

                // clean incremental collision handler: (1), (2), (3)...
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

                SystemSounds.Asterisk.Play();
                MessageBox.Show($"Archivo organizado con éxito. :)\n\nUbicación:\n{destinationPath}",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadClientFolders();
                ResetDropzone();
            }
            catch (Exception ex)
            {
                SystemSounds.Hand.Play();
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

            try
            {
                Properties.Settings.Default.ClientRootPath = clientRootPath;
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