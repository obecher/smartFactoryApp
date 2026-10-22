namespace MyAvaloniaApp.Models
{
    public class LegoBrick : Beweger
    {
        private bool gedockt;

        public Parameters.LegoColor Color { get; set; }
        public Parameters.LegoSize Size { get; set; }

        // true, solange der Roboter den Stein trägt
        public bool Gedockt
        {
            get { return gedockt; }
            set { gedockt = value; }
        }

        // true, solange der Stein noch auf seiner Produktionslinie pendelt.
        // Wird false, sobald der Roboter ihn einmal gegriffen hat.
        public bool AufProduktionslinie { get; set; } = true;

        // pose = MITTELPUNKT des Steins und Drehwinkel (die Pose der Basisklasse
        // ist die linke obere Ecke, deshalb wird hier umgerechnet)
        public LegoBrick(Pose pose, Parameters.LegoColor color, Parameters.LegoSize size)
            : base(pose.X - WidthOf(size) / 2.0, pose.Y - HeightOf(size) / 2.0, pose.Angle, WidthOf(size), HeightOf(size))
        {
            Color = color;
            Size = size;
        }

        public static int WidthOf(Parameters.LegoSize size) => size switch
        {
            Parameters.LegoSize.small => Parameters.brickSmallWidth,
            Parameters.LegoSize.medium => Parameters.brickMediumWidth,
            _ => Parameters.brickLargeWidth
        };

        public static int HeightOf(Parameters.LegoSize size) => size switch
        {
            Parameters.LegoSize.small => Parameters.brickSmallHeight,
            Parameters.LegoSize.medium => Parameters.brickMediumHeight,
            _ => Parameters.brickLargeHeight
        };

        public override string ToString()
        {
            return $"{Color} {Size} bei ({X:F1}, {Y:F1}), gedockt: {Gedockt}";
        }
    }
}
