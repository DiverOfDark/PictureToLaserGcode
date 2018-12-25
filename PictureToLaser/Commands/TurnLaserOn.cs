namespace PictureToLaser
{
    class TurnLaserOn:AbstractCommand
    {
        public override string ToString() => "M3 S0";
    }
}