using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace COALCAVA_Dropzone
{
    public static class WindowsThumbnailProvider
    {
        [Flags]
        private enum ThumbnailOptions
        {
            None = 0x00,
            BiggerSizeOk = 0x01,
            InMemoryOnly = 0x02,
            IconOnly = 0x04,
            ThumbnailOnly = 0x08,
            InCacheOnly = 0x10
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct SIZE
        {
            public int cx;
            public int cy;
            public SIZE(int cx, int cy) { this.cx = cx; this.cy = cy; }
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("bcc18b79-5564-4678-8370-37191780c2e2")]
        private interface IShellItemImageFactory
        {
            void GetImage([In, MarshalAs(UnmanagedType.Struct)] SIZE size, [In] ThumbnailOptions flags, [Out] out IntPtr phbm);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, [In] ref Guid riid, [Out] out IShellItemImageFactory ppv);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static Bitmap GetThumbnail(string fileName, int width, int height)
        {
            IntPtr hBitmap = IntPtr.Zero;
            try
            {
                Guid uuid = new Guid("bcc18b79-5564-4678-8370-37191780c2e2");
                int hr = SHCreateItemFromParsingName(fileName, IntPtr.Zero, ref uuid, out IShellItemImageFactory factory);

                if (hr == 0 && factory != null)
                {
                    factory.GetImage(new SIZE(width, height), ThumbnailOptions.BiggerSizeOk, out hBitmap);
                    if (hBitmap != IntPtr.Zero)
                    {
                        Bitmap result = Image.FromHbitmap(hBitmap);
                        return result;
                    }
                }
            }
            catch { }
            finally
            {
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
            }

            return null;
        }
    }
}