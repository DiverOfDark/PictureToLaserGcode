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

        [Option(Default = 800)]
        public int TravelRate { get; set; }

        [Option(Default = 140.0)]
        public double SizeY { get; set; }

        [Option(Default = 0.15)]
        public double ScanGap { get; set; }
        [Option(Default = 0.15)]
        public double ResX { get; set; }
    }
}