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

        [Option(Default = 0.15)]
        public double ScanGap { get; set; }
        [Option(Default = 0.15)]
        public double ResX { get; set; }
    }
}