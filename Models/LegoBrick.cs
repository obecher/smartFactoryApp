using System;

namespace MyAvaloniaApp.Models
{
    public class LegoBrick : Beweger
    {
        public Parameters.LegoColor Color { get; set; }
        public Parameters.LegoSize Size { get; set; }

        // Wo der Stein gerade ist (Produktionslinie, getragen, auf der Lieferlinie, ...)
        public Parameters.BrickStatus Status { get; set; } = Parameters.BrickStatus.OnProductionLine;

        // true, solange der Roboter den Stein trägt
        public bool Gedockt => Status == Parameters.BrickStatus.Docked;

        // Nur Steine auf einer Produktions- oder Recyclinglinie dürfen gegriffen werden
        public bool Greifbar => Status == Parameters.BrickStatus.OnProductionLine
                             || Status == Parameters.BrickStatus.OnRecycleLine;

        // Nummer des Platzes auf der Recyclinglinie (-1 = nicht auf der Recyclinglinie)
        public int RecyclePlatz { get; set; } = -1;

        // Heimat: Produktionslinie und Position (linke obere Ecke), an der der Stein "produziert" wird
        public ProductionLine? Heimlinie { get; private set; }
        public double HeimX { get; private set; }
        public double HeimY { get; private set; }

        // pose = MITTELPUNKT des Steins und Drehwinkel (die Pose der Basisklasse
        // ist die linke obere Ecke, deshalb wird hier umgerechnet)
        public LegoBrick(Pose pose, Parameters.LegoColor color, Parameters.LegoSize size)
            : base(pose.X - WidthOf(size) / 2.0, pose.Y - HeightOf(size) / 2.0, pose.Angle, WidthOf(size), HeightOf(size))
        {
            Color = color;
            Size = size;
        }

        // Merkt sich die aktuelle Position als Heimat (wird von der ProductionLine nach dem Anlegen aufgerufen)
        public void SetzeHeimat(ProductionLine linie)
        {
            Heimlinie = linie;
            HeimX = X;
            HeimY = Y;
        }

        // Stein wurde auf der Lieferlinie losgelassen und ist in Ordnung: fährt nach rechts zu den Kunden
        public void ZumKunden(ProductionLine lieferlinie)
        {
            AufLinieSetzen(lieferlinie);
            Status = Parameters.BrickStatus.ToCustomer;
        }

        // Stein ist defekt: fährt auf der Lieferlinie nach links zum Recycling
        public void ZumRecycling(ProductionLine lieferlinie)
        {
            AufLinieSetzen(lieferlinie);
            Status = Parameters.BrickStatus.ToRecycle;
        }

        // Setzt den Stein mittig auf die Lieferlinie (waagerecht, in der Spur der Linie)
        private void AufLinieSetzen(ProductionLine linie)
        {
            double halbeBreite = Width / 2.0;
            double mitteX = Math.Clamp(X + halbeBreite, linie.X + halbeBreite, linie.X + linie.Width - halbeBreite);
            double mitteY = linie.Y + linie.Height / 2.0;
            SetzeMitte(mitteX, mitteY, 0);
            RecyclePlatz = -1;
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
            return $"{Color} {Size} bei ({X:F1}, {Y:F1}), Status: {Status}";
        }
    }
}
