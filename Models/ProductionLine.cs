using System;
using System.Collections.Generic;
using System.Drawing;

namespace MyAvaloniaApp.Models;

// Eine Produktionslinie: ein Rechteck mit Pose (x, y = linke obere Ecke des ungedrehten Rechtecks,
// angle = Drehung um die Mitte). Linien mit Farbe haben vier Legosteine, Recycling- und
// Lieferlinie haben keine Farbe und keine Steine.
public class ProductionLine
{
    private double x;
    private double y;
    private double angle;
    private int width;
    private int height;
    private List<LegoBrick> bricks = new List<LegoBrick>();
    private int tickCount;

    public Parameters.LegoColor? Color { get; }

    public double X => x;
    public double Y => y;
    public double Angle => angle;
    public int Width => width;
    public int Height => height;
    public Point Location => new Point((int)Math.Round(x), (int)Math.Round(y));

    public IReadOnlyList<LegoBrick> Bricks => bricks;

    public ProductionLine(Pose pose, int width, int height, Parameters.LegoColor? color = null)
    {
        x = pose.X;
        y = pose.Y;
        angle = pose.Angle;
        this.width = width;
        this.height = height;
        Color = color;

        // Start in der Mitte der Hin- und Herbewegung: die Steine pendeln um ihre Startposition
        tickCount = Parameters.stepsPerDirection / 2;

        if (color != null)
        {
            CreateBricks(color.Value);
        }
    }

    public ProductionLine(Pose pose, Parameters.LegoColor color)
        : this(pose, ViewParameters.ProductionLineWidth, ViewParameters.ProductionLineHeight, color)
    {
    }

    // Die Ecken des gedrehten Rechtecks (für die Kollisionsprüfung)
    public PointF[] Ecken()
    {
        return Beweger.Ecken(x, y, width, height, angle);
    }

    // Vier Steine in einer Reihe am oberen Rand der UNGEDREHTEN Linie. Die Position wird
    // in Linien-Koordinaten (Ursprung = Linienmitte) berechnet und dann mit dem
    // Linienwinkel auf den Plan gedreht. So stimmen alle vier Ecken automatisch.
    private void CreateBricks(Parameters.LegoColor color)
    {
        Parameters.LegoSize size = Parameters.LegoSize.medium;
        double rad = angle * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        double centerX = x + width / 2.0;
        double centerY = y + height / 2.0;

        for (int i = 0; i < Parameters.bricksPerLine; i++)
        {
            // lokal: gleichmäßig verteilt, bündig mit der oberen Kante
            double lx = (i - (Parameters.bricksPerLine - 1) / 2.0) * Parameters.brickSpacing;
            double ly = -height / 2.0 + LegoBrick.HeightOf(size) / 2.0;

            double gx = centerX + lx * cos - ly * sin;
            double gy = centerY + lx * sin + ly * cos;

            bricks.Add(new LegoBrick(new Pose(gx, gy, angle), color, size));
        }
    }

    // Ein Schritt der Pendelbewegung entlang der Längsachse der Linie
    public void Bewegen()
    {
        if (bricks.Count == 0) return;

        bool forward = tickCount < Parameters.stepsPerDirection;
        tickCount = (tickCount + 1) % (2 * Parameters.stepsPerDirection);

        Parameters.Direction direction = DirectionAlongLine(forward);
        foreach (LegoBrick brick in bricks)
        {
            // gegriffene Steine gehören nicht mehr zur Linie
            if (!brick.AufProduktionslinie) continue;
            brick.Bewegen(direction);
        }
    }

    // Längsachse der Linie als Richtung (0 Grad -> rechts, 90 -> unten, 270 -> oben)
    private Parameters.Direction DirectionAlongLine(bool forward)
    {
        double rad = angle * Math.PI / 180.0;
        int dx = Math.Sign((int)Math.Round(Math.Cos(rad)));
        int dy = Math.Sign((int)Math.Round(Math.Sin(rad)));

        if (!forward)
        {
            dx = -dx;
            dy = -dy;
        }

        return (dx, dy) switch
        {
            (0, -1) => Parameters.Direction.up,
            (0, 1) => Parameters.Direction.down,
            (-1, 0) => Parameters.Direction.left,
            (1, 0) => Parameters.Direction.right,
            (-1, -1) => Parameters.Direction.upLeft,
            (1, -1) => Parameters.Direction.upRight,
            (-1, 1) => Parameters.Direction.downLeft,
            (1, 1) => Parameters.Direction.downRight,
            _ => Parameters.Direction.stop
        };
    }
}
