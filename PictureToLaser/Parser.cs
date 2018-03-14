using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using CommandLine;

namespace PictureToLaser
{
    public class Options
    {
        [Option(Required = true)]
        public string FilePath { get; set; }

        [Option(Default = 100)]
        public int LaserMax { get; set; }

        [Option(Default = 0)]
        public int LaserMin { get; set; }

        [Option(Default = 1000)]
        public int FeedRate { get; set; }
        [Option(Default = 3000)]
        public int TravelRate { get; set; }

        [Option(Default = 40)]
        public double SizeY { get; set; }

        [Option(Default = 0.15)]
        public double ScanGap { get; set; }
        [Option(Default = 0.1)]
        public double ResX { get; set; }
    }

    public static class Parser
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var filePath = @"z:\photo.jpg";

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

                args = new[] { "--filepath", filePath };
            }
            CommandLine.Parser.Default.ParseArguments<Options>(args).MapResult(Parsed, Errors);
        }

        private static object Errors(IEnumerable<Error> arg)
        {
            foreach (var ar in arg)
            {
                Console.WriteLine(ar.ToString());
            }

            return null;
        }

        private static object Parsed(Options arg)
        {
            int pixelsX, pixelsY;
            double sizeX;
            float[,] arr;
            {
                using (var img = Bitmap.FromFile(arg.FilePath))
                {
                    sizeX = arg.SizeY * img.Width /
                                img.Height; //SET A HEIGHT AND CALC WIDTH (this should be customizable)
                    pixelsX = (int)(sizeX / arg.ResX);
                    pixelsY = (int)(arg.SizeY / arg.ScanGap);
                    arr = new float[pixelsX, pixelsY];
                    using (var newImg = new Bitmap(img, pixelsX, pixelsY))
                    {
                        for (int i = 0; i < pixelsX; i++)
                            for (int j = 0; j < pixelsY; j++)
                                arr[i, j] = newImg.GetPixel(i, j).GetBrightness();
                    }
                }
            }

            var sb = new StringBuilder();

            var laserMin = arg.LaserMin;
            var laserMax = arg.LaserMax;

            sb.AppendLine($";Size in pixels X={pixelsX}, Y={pixelsY}");
            sb.AppendLine($";Size in cm X={sizeX * arg.ResX}, Y={arg.SizeY * arg.ScanGap}");
            var cmdRate = (int)(arg.FeedRate / arg.ResX * 2 / 60);
            sb.AppendLine($";Speed is {arg.FeedRate} mm/min, {arg.ResX} mm/pix => {cmdRate} lines/sec");
            sb.AppendLine($";Power is {laserMin} to {laserMax} (" + laserMin / 255.0 * 100 + "%-" +
                          laserMax / 255.0 * 100 + "%)");

            sb.AppendLine("G21");
            sb.AppendLine("M5; Turn laser off");
            sb.AppendLine($"G1 F{arg.FeedRate}");

            var lineIndex = 0;
            sb.AppendLine($"G0 X0 Y0 F{arg.TravelRate}");
            for (var line = 0.0; line < arg.SizeY && lineIndex < pixelsY; line += arg.ScanGap)
            {
                var pixelIndex = 0;
                sb.AppendLine("G1 X0 Y" + line.MyRound() + $" F{arg.TravelRate}; Line {lineIndex}");
                sb.AppendLine($"G1 F{arg.FeedRate}");
                for (var pixel = 0.0; pixel < sizeX && pixelIndex < pixelsX; pixel += arg.ResX, pixelIndex++)
                {
                    sb.AppendLine("G1 X" + pixel.MyRound() + "");

                    var value = arr[pixelIndex, lineIndex];
                    var lvalue = Map(value, 1, 0, laserMin, laserMax).MyRound();
                    sb.AppendLine($"M3 S{lvalue}");

                    while (pixelIndex + 1 < pixelsX)
                    {
                        var nextValue = arr[pixelIndex + 1, lineIndex];
                        var lnextValue = Map(nextValue, 1, 0, laserMin, laserMax).MyRound();
                        if (lnextValue != lvalue)
                            break;

                        pixel += arg.ResX;
                        pixelIndex++;
                    }
                }

                sb.AppendLine("M5;");
                lineIndex++;
                line += arg.ScanGap;
                if (line > arg.SizeY || lineIndex > pixelsY)
                    break;

                pixelIndex = pixelsX - 1;
                sb.AppendLine($"G1 X{pixelIndex} Y" + line.MyRound() + $" F{arg.TravelRate}; Line {lineIndex}");
                sb.AppendLine($"G1 F{arg.FeedRate}");
                for (var pixel = sizeX; pixel > 0 && pixelIndex > 0; pixel -= arg.ResX, pixelIndex--)
                {
                    sb.AppendLine("G1 X" + pixel.MyRound() + "");

                    var value = arr[pixelIndex, lineIndex];
                    var lvalue = Map(value, 1, 0, laserMin, laserMax).MyRound();
                    sb.AppendLine($"M3 S{lvalue}");

                    while (pixelIndex - 1 > 0)
                    {
                        var nextValue = arr[pixelIndex - 1, lineIndex];
                        var lnextValue = Map(nextValue, 1, 0, laserMin, laserMax).MyRound();
                        if (lnextValue != lvalue)
                            break;

                        pixel -= arg.ResX;
                        pixelIndex--;
                    }
                }


                sb.AppendLine("M5;");
                lineIndex++;
            }

            sb.AppendLine($"M5 ;Turn laser off");
            sb.AppendLine($"G0 X0 Y0 F{arg.TravelRate} ;Go home");

            File.WriteAllText(Path.ChangeExtension(arg.FilePath, "gcode"), sb.ToString());
            return null;
        }

        static string MyRound(this double d) => Math.Round(d, 4).ToString(CultureInfo.InvariantCulture);

        static double Map(double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            var fromRange = fromHigh - fromLow;
            var toRange = toHigh - toLow;
            var scaleFactor = toRange / fromRange;

            // Re-zero the value within the from range
            var tmpValue = value - fromLow;
            // Rescale the value to the to range
            tmpValue *= scaleFactor;
            // Re-zero back to the to range
            return tmpValue + toLow;
        }
    }
}