namespace PictureToLaser
{
    internal class MillimeterUnitsCommand : AbstractCommand
    {
        public MillimeterUnitsCommand()
        {
            Comment = "Switch to millimeters";
        }
        
        public override string ToString() => "G21" + base.ToString();
    }
}