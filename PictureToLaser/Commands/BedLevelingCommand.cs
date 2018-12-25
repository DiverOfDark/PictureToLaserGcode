namespace PictureToLaser
{
    internal class BedLevelingCommand : AbstractCommand
    {
        public bool Enabled { get; }

        public BedLevelingCommand(bool enabled)
        {
            Enabled = enabled;
            Comment = enabled ? "Activate UBL" : "Deactivate UBL";
        }

        public override string ToString() => "G29 " + (Enabled ? "A" : "D") + base.ToString();
    }
}