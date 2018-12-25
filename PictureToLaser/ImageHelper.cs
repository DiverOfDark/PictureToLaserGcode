using System.Drawing;
using System.IO;

namespace PictureToLaser
{
    public static class ImageHelper
    {
        public static void CreateSampleImageIfNotExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                var b = new Bitmap(255, 10);
                var g = Graphics.FromImage(b);

                for (int i = 0; i < 255; i++)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(i, i, i)), new Rectangle(i, 0, 2, 50));
                }

                g.Flush();
                b.Save(filePath);
            }            
        }

        public static void GetPixelsForEngraving(string argFilePath, double sizeY, double resX, double scanGap, out int pixelsX, out int pixelsY, out double sizeX,
            out float[,] arr)
        {
            using (var img = Bitmap.FromFile(argFilePath))
            {
                sizeX = sizeY * img.Width /
                        img.Height; //SET A HEIGHT AND CALC WIDTH (this should be customizable)
                pixelsX = (int) (sizeX / resX);
                pixelsY = (int) (sizeY / scanGap);
                arr = new float[pixelsX, pixelsY];
                using (var newImg2 = new Bitmap(img, pixelsX, pixelsY))
                using (var newImg = Transparent2Color(newImg2, Color.White))
                {
                    for (int i = 0; i < pixelsX; i++)
                    for (int j = 0; j < pixelsY; j++)
                    {
                        var pixel = newImg.GetPixel(i, j);

                        var brightness = pixel.GetBrightness();
                        arr[i, j] = brightness;
                        int baseColor = (int) (brightness * 255);
                        newImg.SetPixel(i, j, Color.FromArgb(baseColor, baseColor, baseColor));
                    }
                }
            }
        }
        
        private static Bitmap Transparent2Color(Bitmap bmp1, Color target)
        {
            Bitmap bmp2 = new Bitmap(bmp1.Width, bmp1.Height);
            Rectangle rect = new Rectangle(Point.Empty, bmp1.Size);
            using (Graphics G = Graphics.FromImage(bmp2) )
            {
                G.Clear(target);
                G.DrawImageUnscaledAndClipped(bmp1, rect);
            }
            return bmp2;
        }
    }
}