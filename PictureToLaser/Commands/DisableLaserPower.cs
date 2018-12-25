namespace PictureToLaser
{
    internal class DisableLaserPower : AbstractCommand
    {
        public DisableLaserPower()
        {
            Comment = "Turn laser power off";
        }
        
        public override string ToString() => "M107 P1" + base.ToString();
    }
}