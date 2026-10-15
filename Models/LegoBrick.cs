using System.Drawing;

namespace MyAvaloniaApp.Models
{
    public class LegoBrick : Beweger
    {
        public Parameters.LegoColor Color { get; set; }
        public Parameters.LegoSize Size { get; set; }

        // Richtung, in der sich der Stein entlang seiner Linie hin und her bewegt
        public Parameters.Direction ForwardDirection { get; }
        public Parameters.Direction BackwardDirection { get; }

        public LegoBrick(Pose pose, Parameters.LegoColor color, Parameters.LegoSize size,
                         Parameters.Direction forward, Parameters.Direction backward)
            : base(pose.Location)
        {
            Color = color;
            Size = size;
            ForwardDirection = forward;
            BackwardDirection = backward;
        }

        public override string ToString()
        {
            return $"{Color} {Size} bei ({Location.X}, {Location.Y})";
        }
    }
}