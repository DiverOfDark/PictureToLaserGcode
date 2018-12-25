namespace PictureToLaser
{
    internal class Status : AbstractCommand
    {
        public string Text { get; set; }

        public Status(string status)
        {
            Text = status;
        }

        public override string ToString() => "M117 " + Text;
    }
}