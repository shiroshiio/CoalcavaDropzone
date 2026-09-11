using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CoalcavaDropzone
{
    internal static class Program
    {
        // Windows 10 Creators Update (1703) and Windows 11
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        // Windows 8.1 / Windows 10
        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int awareness);

        // Windows Vista / 7 fallback
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        [STAThread]
        static void Main()
        {
            // 1. Force the OS to disable bitmap stretching before message loops start
            try
            {
                if (!SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                {
                    SetProcessDpiAwareness(2); // PROCESS_PER_MONITOR_DPI_AWARE
                }
            }
            catch
            {
                try { SetProcessDPIAware(); } catch { }
            }

            // 2. ClearType text rendering & Windows visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }
    }
}