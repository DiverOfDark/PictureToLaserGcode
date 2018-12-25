using System;

namespace PictureToLaser
{
    class AbstractCommand
    {
        public string Comment { get; set; }

        public override string ToString() => String.IsNullOrEmpty(Comment) ? "" : (" ; " + Comment);
    }
}