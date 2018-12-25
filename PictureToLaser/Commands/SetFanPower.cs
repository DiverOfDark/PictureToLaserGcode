namespace PictureToLaser
{
    class SetFanPower : AbstractCommand
    {
        public SetFanPower(double power)
        {
            Power = power;
        }

        public double Power { get; set; }
        
        public override string ToString() => $"M106 P0 S{Power.MyInt()}" + base.ToString();
    }
}