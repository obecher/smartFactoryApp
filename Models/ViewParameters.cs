using System.Drawing;

namespace MyAvaloniaApp.Models
{
    // Position (x, y = linke obere Ecke des NOCH NICHT GEDREHTEN Rechtecks) und Drehwinkel in Grad
    // im Uhrzeigersinn. Gedreht wird um die Mitte des Rechtecks.
    public struct Pose
    {
        public double X {get; }
        public double Y {get; }
        public double Angle {get; }
        public Pose(double x, double y, double angle)
        {
            X = x;
            Y = y;
            Angle = angle;
        }
        public Point Location => new Point((int)System.Math.Round(X), (int)System.Math.Round(Y));
    }

    public static class ViewParameters
    {
        // Spielfeld (grauer Bereich)
        public static Rectangle Borders = new Rectangle(
            Parameters.arenaLeft, Parameters.arenaTop, Parameters.arenaSize, Parameters.arenaSize);

        // Startpose des Roboters
        public static Pose RobotPose = new Pose(285, 200, 0);

        // Größe der Produktionslinien (ungedreht: breit und flach)
        public const int ProductionLineWidth = 160;
        public const int ProductionLineHeight = 60;
        public const int ProductionDeliveryWidth = 186;
        public const int ProductionDeliveryHeight = 56;

        // Die Legosteine liegen in der oberen Streifen-Hälfte der ungedrehten Linie.
        // 90 Grad: Streifen zeigt nach rechts (rote Linie links), 270 Grad: nach links (blaue Linie rechts).
        // Pose = linke obere Ecke des ungedrehten Rechtecks = Mittelpunkt - (Breite/2, Höhe/2)
        public static Pose RectRedPose      = new Pose(140 - ProductionLineWidth / 2, 212 - ProductionLineHeight / 2, 90);
        public static Pose RectBluePose     = new Pose(460 - ProductionLineWidth / 2, 212 - ProductionLineHeight / 2, 270);
        public static Pose RectYellowPose   = new Pose(170 - ProductionLineWidth / 2, 462 - ProductionLineHeight / 2, 0);
        public static Pose RectGreenPose    = new Pose(430 - ProductionLineWidth / 2, 462 - ProductionLineHeight / 2, 0);
        public static Pose RectRecyclePose  = new Pose(300 - ProductionLineWidth / 2, 322 - ProductionLineHeight / 2, 0);
        public static Pose RectDeliveryPose = new Pose(220, 40, 0);
    }
}
