namespace PictureToLaser
{
    class Move : AbstractCommand
    {
        public double? NewX { get; set; }
        public double? NewY { get; set; }
        public int? Rate { get; set; }
        
        public double? NewI { get; set; }
        public double? NewJ { get; set; }
        
        public double? Power { get; set; }
        
        public bool ArcClockwise { get; set; } // G2
        public bool ArcCounterClockwise { get; set; } //G3
        public bool Linear { get; set; }

        public override string ToString()
        {
            var res = ArcClockwise ? "G2" :
                ArcCounterClockwise ? "G3" :
                Linear ? "G0" : 
                "G1";
            
            if (NewX.HasValue)
            {
                res += " X" + NewX.Value.MyRound();
            }

            if (NewY.HasValue)
            {
                res += " Y" + NewY.Value.MyRound();
            }

            if (NewI.HasValue)
            {
                res += " I" + NewI.Value.MyRound();
            }

            if (NewJ.HasValue)
            {
                res += " J" + NewJ.Value.MyRound();
            }
            
            if (Rate.HasValue)
            {
                res += " F" + Rate.Value;
            }

            if (Power.HasValue)
            {
                res += " S" + Power.Value.MyRound();
            }

            res += base.ToString();
            
            return res;
        }
        
        
    }
}