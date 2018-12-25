namespace PictureToLaser
{
    class TurnLaserOff : AbstractCommand
    {
        public override string ToString() => "M5 S0" + base.ToString();
    }
}