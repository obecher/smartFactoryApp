using System;
using System.Drawing;

namespace MyAvaloniaApp.Models;

internal class Robot : Beweger
{
    // aktuelle Richtung der Dauerbewegung (stop = steht)
    private Parameters.Direction robotDirection = Parameters.Direction.stop;

    public Robot(Pose pose, Rectangle borders)
        : base(pose, Parameters.robotWidth, Parameters.robotHeight, borders)
    {
    }

    public Parameters.Direction RobotDirection
    {
        get { return robotDirection; }
        set { robotDirection = value; }
    }

    // "oben" heißt: in Blickrichtung des Roboters (der Greifer zeigt bei 0 Grad nach oben).
    // Dafür wird der Richtungsvektor um den Drehwinkel des Roboters gedreht.
    public override bool Bewegen(Parameters.Direction direction, double distance)
    {
        var (dx, dy) = Vektor(direction, distance);

        if (Parameters.moveRelativeToRobot)
        {
            double rad = Angle * Math.PI / 180.0;
            double gx = dx * Math.Cos(rad) - dy * Math.Sin(rad);
            double gy = dx * Math.Sin(rad) + dy * Math.Cos(rad);
            dx = gx;
            dy = gy;
        }

        return Verschieben(dx, dy);
    }

    public bool ResetRotation()
    {
        return WinkelSetzen(0);
    }

    // Nach jeder Bewegung: ist ein Stein nah genug am Greifpunkt?
    protected override void NachBewegung()
    {
        PruefeDock();
    }
}
