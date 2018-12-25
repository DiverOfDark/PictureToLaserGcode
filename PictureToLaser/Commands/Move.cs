namespace PictureToLaser
{
    class Move : AbstractCommand
    {
        public double? NewX { get; set; }
        public double? NewY { get; set; }
        public int? Rate { get; set; }

        public override string ToString()
        {
            var res = "G1";
            if (NewX.HasValue)
            {
                res += " X" + NewX.Value.MyRound();
            }

            if (NewY.HasValue)
            {
                res += " Y" + NewY.Value.MyRound();
            }

            if (Rate.HasValue)
            {
                res += " F" + Rate.Value;
            }

            res += base.ToString();
            
            return res;
        }
        
        
    }
}