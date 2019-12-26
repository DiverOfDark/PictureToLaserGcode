using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PictureToLaser
{
    internal class ImageToGcode : GCodeConverter
    {
        private readonly Options _arg;

        public ImageToGcode(Options arg) : base(arg)
        {
            _arg = arg;
        }

        public object Process(out Queue<AbstractCommand> result)
        {
            int pixelsX, pixelsY;
            double sizeX;
            float[,] arr;
            ImageHelper.GetPixelsForEngraving(_arg.FilePath, _arg.SizeY, _arg.ResX, _arg.ScanGap, out pixelsX, out pixelsY, out sizeX, out arr);
            
            var widthCm = sizeX * _arg.ResX;
            var heightCm = _arg.SizeY * _arg.ScanGap;

            if (widthCm > 22 || heightCm > 22)
            {
                Console.Error.WriteLine($"Sizes are out of possible: {widthCm} / {heightCm}");
                result = null;
                return 1;
            }
            
            var laserMin = _arg.LaserMin;
            var laserMax = _arg.LaserMax;

            var imageToCommands = ImageToCommands(_arg, sizeX, pixelsY, pixelsX, arr, laserMin, laserMax);
            
            var commands = new Queue<AbstractCommand>();
            
            Header(imageToCommands).Requeue(commands);
            imageToCommands.Requeue(commands);
            Footer().Requeue(commands);

            result = commands;
            return 0;
        }

        private static Queue<AbstractCommand> ImageToCommands(Options arg, double sizeX, int pixelsY, int pixelsX, float[,] arr,
            int laserMin, int laserMax)
        {
            var commands = new Queue<AbstractCommand>();
            
            var lineIndex = 0;

            for (var line = 0.0; line < arg.SizeY && lineIndex < pixelsY; line += arg.ScanGap, lineIndex++)
            {
                var minX = 0;
                var maxX = pixelsX;
                Extensions.AdjustMinMaxPixels(arg, arr, lineIndex, ref minX, ref maxX);

                commands.Enqueue(new Move {NewY = line, Comment = $"Line {lineIndex}"});
                commands.Enqueue(new Status($"Line {lineIndex}/{pixelsY}..."));

                bool even = lineIndex % 2 == 0;

                for (var pixelIndex = even ? minX : maxX - 1;
                    even ? pixelIndex < maxX : pixelIndex > minX;
                    pixelIndex += even ? 1 : -1)
                {
                    var lvalue = Extensions.Map(arr[pixelIndex, lineIndex], 1, 0, laserMin, laserMax);
                    commands.Enqueue(new Move {NewX = arg.ResX * pixelIndex, Power = lvalue});
                }
            }

            return commands;
        }
    }
}