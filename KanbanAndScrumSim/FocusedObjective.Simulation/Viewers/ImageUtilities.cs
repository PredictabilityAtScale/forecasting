using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

namespace FocusedObjective.Simulation.Viewers
{
    internal static class ImageUtilities
    {
            internal static void SaveWindow(Window window, int dpi, string filename)
            {

                var rtb = new RenderTargetBitmap(
                    (int)window.ActualWidth, //width
                    (int)window.ActualHeight, //height
                    dpi, //dpi x
                    dpi, //dpi y 
                    PixelFormats.Pbgra32 // pixelformat
                    );
                rtb.Render(window);

                SaveRTBAsPNG(rtb, filename);

            }

            internal static void SaveCanvas(Window window, Canvas canvas, int dpi, string filename)
            {
                Size size = new Size(window.Width, window.Height);
                canvas.Measure(size);
                //canvas.Arrange(new Rect(size));

                var rtb = new RenderTargetBitmap(
                    (int)window.Width, //width
                    (int)window.Height, //height
                    dpi, //dpi x
                    dpi, //dpi y 
                    PixelFormats.Pbgra32 // pixelformat
                    );
                rtb.Render(canvas);

                SaveRTBAsPNG(rtb, filename);
            }

            private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
            {
                var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));

                using (var stm = System.IO.File.Create(filename))
                {
                    enc.Save(stm);
                }
            }
        
    }
}
