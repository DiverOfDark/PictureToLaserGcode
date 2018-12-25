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
    public static class Parser
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var filePath = @"f:\laserable\cala.nc";
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
            object exitCode = 0;

            Queue<AbstractCommand> result = null;
            
            switch (Path.GetExtension(arg.FilePath))
            {   
                case ".jpg":
                case ".png":
                case ".bmp":
                    exitCode = new ImageToGcode(arg).Process(out result);
                    break;
                case ".nc":
                    exitCode = new NcToGcode(arg).Process(out result);
                    break;
                default:
                    Console.Error.WriteLine("Unsupported extension: " + Path.GetExtension(arg.FilePath));
                    exitCode = 1;
                    break;
            }

            if (result != null)
            {
                result = QueueOptimizer.Optimize(result);

                var sb = new StringBuilder();
                while (result.Count > 0)
                {
                    sb.AppendLine(result.Dequeue().ToString());
                }
            
                var contents = sb.ToString();
                File.WriteAllText(Path.ChangeExtension(arg.FilePath, "gcode"), contents);
                Console.WriteLine("Saved G-Code.");
            }
            
            return exitCode;
        }
    }
}