using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CommandLine;

namespace PictureToLaser
{
    public class Options
    {
        [Option(Required = true)]
        public string FilePath { get; set; }

        [Option(Default = 255)]
        public int LaserMax { get; set; }

        [Option(Default = 0)]
        public int LaserMin { get; set; }

        [Option(Default = 2000)]
        public int TravelRate { get; set; }

        [Option(Default = 50.0)]
        public double SizeY { get; set; }

        [Option(Default = 0.10)]
        public double ScanGap { get; set; }
        [Option(Default = 0.10)]
        public double ResX { get; set; }
    }

    public static class Parser
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var filePath = @"f:\laserable\octocat.png";
                ImageHelper.CreateSampleImageIfNotExists(filePath);
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
            ImageHelper.GetPixelsForEngraving(arg.FilePath, arg.SizeY, arg.ResX, arg.ScanGap, out pixelsX, out pixelsY, out sizeX, out arr);
            
            Console.WriteLine("Downscaled.");

            var widthCm = sizeX * arg.ResX;
            var heightCm = arg.SizeY * arg.ScanGap;
            
            var laserMin = arg.LaserMin;
            var laserMax = arg.LaserMax;

            var cmdRate = (int)(arg.TravelRate / arg.ResX * 2 / 60);

            var commands = new Queue<AbstractCommand>();
            commands.Enqueue(new Comment($"Size in pixels X={pixelsX}, Y={pixelsY}"));            
            commands.Enqueue(new Comment($"Size in cm X={widthCm}, Y={heightCm}"));
            
            commands.Enqueue(
                new Comment($"Speed is {arg.TravelRate} mm/min, {arg.ResX} mm/pix => {cmdRate} lines/sec"));

            var totalTime = new TimeSpan(0, 0, (int) (100 * widthCm * arg.SizeY / arg.TravelRate * 60));
            
            commands.Enqueue(new Comment($"Estimated engraving time: {totalTime}"));
            
            commands.Enqueue(new Comment($"Power is {laserMin} to {laserMax} (" + laserMin / 255.0 * 100 + "%-" +
                                         laserMax / 255.0 * 100 + "%)"));

            Console.WriteLine(String.Join("\n", commands));
            
            if (widthCm > 22 || heightCm > 22)
            {
                Console.Error.WriteLine($"Sizes are out of possible: {widthCm} / {heightCm}");
                return 1;
            }

            commands.Enqueue(new MillimeterUnitsCommand());
            
            commands.Enqueue(new BedLevelingCommand(false));
            commands.Enqueue(new SetLaserPower(0) {Comment = "Turn laser pwm off"});
            commands.Enqueue(new TurnLaserOn());

            var lineIndex = 0;
            commands.Enqueue(new Move {NewX = 0, NewY = 0, Rate = arg.TravelRate});
            
            // Draw rect around area;
            commands.Enqueue(new SetLaserPower(1));
            commands.Enqueue(new Move {NewX = sizeX});
            commands.Enqueue(new Move{NewY = arg.SizeY});
            commands.Enqueue(new Move{NewX = 0});
            commands.Enqueue(new Move{NewY = 0});

            commands.Enqueue(new Pause());

            commands.Enqueue(new SetLaserPower(0));
            for (var line = 0.0; line < arg.SizeY && lineIndex < pixelsY; line += arg.ScanGap)
            {
                commands.Enqueue(new Move {NewY = line, Comment = $"Line {lineIndex}"});
                commands.Enqueue(new Status($"Line {lineIndex}/{pixelsY}..."));

                var minX = 0;
                var maxX = pixelsX;
                Extensions.AdjustMinMaxPixels(arg, arr, lineIndex, ref minX, ref maxX);
                
                for (var pixelIndex = minX; pixelIndex < maxX; pixelIndex++)
                {
                    commands.Enqueue(new Move {NewX = arg.ResX * pixelIndex});
                    var lvalue = Extensions.Map(arr[pixelIndex, lineIndex], 1, 0, laserMin, laserMax);
                    commands.Enqueue(new SetLaserPower(lvalue));
                }

                commands.Enqueue(new SetLaserPower(0));
                lineIndex++;
                line += arg.ScanGap;
                if (line >= arg.SizeY || lineIndex >= pixelsY)
                    break;

                minX = 0;
                maxX = pixelsX;
                Extensions.AdjustMinMaxPixels(arg, arr, lineIndex, ref minX, ref maxX);
                
                commands.Enqueue(new Status($"Line {lineIndex}/{pixelsY}..."));

                commands.Enqueue(new Move {NewY = line});
                for (var pixelIndex = maxX - 1; pixelIndex > minX; pixelIndex--)
                {
                    commands.Enqueue(new Move {NewX = arg.ResX * pixelIndex});
                    var lvalue = Extensions.Map(arr[pixelIndex, lineIndex], 1, 0, laserMin, laserMax);
                    commands.Enqueue(new SetLaserPower(lvalue));
                }

                commands.Enqueue(new SetLaserPower(0));
                lineIndex++;
            }
            
            commands.Enqueue(new DisableLaserPower());
            commands.Enqueue(new BedLevelingCommand(true));
            commands.Enqueue(new TurnLaserOff {Comment = "Turn laser power off"});
            commands.Enqueue(new Move {NewX = 0, NewY = 0, Rate = arg.TravelRate, Comment = "Go home"});

            commands = QueueOptimizer.Optimize(commands);
            
            var contents = string.Join("\n", commands.Select(s => s.ToString()));
            File.WriteAllText(Path.ChangeExtension(arg.FilePath, "gcode"), contents);
            Console.WriteLine("Saved G-Code.");
            return null;
        }


        
    }
}