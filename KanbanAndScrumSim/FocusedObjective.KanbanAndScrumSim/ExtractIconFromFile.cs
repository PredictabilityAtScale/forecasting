using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace FocusedObjective.KanbanSim
{

    public static class ExtractIconFromFile
    {
        // Constants that we need in the function call
        private const int SHGFI_ICON = 0x100;
        private const int SHGFI_SMALLICON = 0x1;
        private const int SHGFI_LARGEICON = 0x0;

        // This structure will contain information about the file
        public struct SHFILEINFO
        {
            // Handle to the icon representing the file
            public IntPtr hIcon;
            // Index of the icon within the image list
            public int iIcon;
            // Various attributes of the file
            public uint dwAttributes;
            // Path to the file
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szDisplayName;
            // File type
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        // The signature of SHGetFileInfo (located in Shell32.dll)
        [DllImport("Shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, uint uFlags);

        public static Bitmap ExtractLargeBitmap(string fileName)
        {
            // Will store a handle to the large icon
            IntPtr hImgLarge;

            SHFILEINFO shinfo = new SHFILEINFO();

            // Sore the icon in this myIcon object
            System.Drawing.Icon myIcon;

            // Get a handle to the small icon
            hImgLarge = SHGetFileInfo(fileName, 0, ref shinfo, Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);

            // Get the large icon from the handle
            myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);

            // Display the large icon
            return myIcon.ToBitmap();
        }

        public static Bitmap ExtractSmallBitmap(string fileName)
        {
            // Will store a handle to the small icon
            IntPtr hImgSmall;

            SHFILEINFO shinfo = new SHFILEINFO();

            // Sore the icon in this myIcon object
            System.Drawing.Icon myIcon;

            // Get a handle to the small icon
            hImgSmall = SHGetFileInfo(fileName, 0, ref shinfo, Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);

            // Get the small icon from the handle
            myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);

            // Display the small icon
            return myIcon.ToBitmap();
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bmpImage = new BitmapImage();

            try
            {
                MemoryStream strm = new MemoryStream();
                bitmap.Save(strm, System.Drawing.Imaging.ImageFormat.Png);

                bmpImage.BeginInit();
                strm.Seek(0, SeekOrigin.Begin);
                bmpImage.StreamSource = strm;
                bmpImage.EndInit();
            }
            catch
            { }

            return bmpImage;
        }
    }
}
