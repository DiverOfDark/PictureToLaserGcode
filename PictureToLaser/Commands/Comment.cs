namespace PictureToLaser
{
    internal class Comment : AbstractCommand
    {
        public Comment(string comment)
        {
            Comment = comment;
        }

        public override string ToString() => base.ToString().Trim();
    }
}