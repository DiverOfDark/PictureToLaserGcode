namespace PictureToLaser
{
    internal class Pause : AbstractCommand
    {
        public Pause()
        {
            Comment = "Pause";
        }
        
        public override string ToString() => "M0" + base.ToString();
    }
}