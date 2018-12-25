namespace PictureToLaser
{
    internal class MovementDetails
    {
        public double TravelDistance { get; set; } = 0;
        public double MinX { get; set; } = double.MaxValue;
        public double MaxX { get; set; } = double.MinValue;

        public double MinY { get; set; } = double.MaxValue;
        public double MaxY { get; set; } = double.MinValue;

        public int LaserMin { get; set; } = int.MaxValue;
        public int LaserMax { get; set; } = int.MinValue;
    }
}