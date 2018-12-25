namespace PictureToLaser
{
    class SetLaserPower : AbstractCommand
    {
        public SetLaserPower(double power)
        {
            Power = power;
        }

        public double Power { get; set; }
        
        public override string ToString() => $"M106 P1 S{Power.MyInt()}" + base.ToString();
    }
}