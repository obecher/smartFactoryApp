using System;
using System.Collections.Generic;
using System.Drawing;

namespace MyAvaloniaApp.Models;

// Eine Produktionslinie: ein Rechteck mit Pose (x, y = linke obere Ecke des ungedrehten Rechtecks,
// angle = Drehung um die Mitte). Linien mit Farbe haben vier Legosteine. Recycling-, Liefer- und
// die beiden Pfeil-Felder haben keine Farbe und anfangs keine Steine.
public class ProductionLine
{
    private double x;
    private double y;
    private double angle;
    private int width;
    private int height;
    private List<LegoBrick> bricks = new List<LegoBrick>();
    private int tickCount;
    private double versatz;     // Wie weit sind die Steine durch das Pendeln gerade aus der Grundstellung?

    public Parameters.LegoColor? Color { get; }

    public double X => x;
    public double Y => y;
    public double Angle => angle;
    public int Width => width;
    public int Height => height;
    public Point Location => new Point((int)Math.Round(x), (int)Math.Round(y));

    // Verschiebung der pendelnden Steine entlang der Längsachse der Linie (in Pixeln, vorwärts = positiv)
    public double Versatz => versatz;

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

    // Abstand eines Punktes zur Linie (0 = Punkt liegt auf der Linie)
    public double AbstandZu(PointF punkt)
    {
        return Beweger.AbstandPunktRechteck(punkt, x, y, width, height, angle);
    }

    // Mittelpunkt und Winkel von Platz Nummer index (0 bis bricksPerLine-1) auf dieser Linie.
    // Die Plätze liegen in einer Reihe am oberen Rand der UNGEDREHTEN Linie. Berechnet wird in
    // Linien-Koordinaten (Ursprung = Linienmitte) und dann mit dem Linienwinkel auf den Plan gedreht.
    public Pose SlotPose(int index, Parameters.LegoSize size)
    {
        double rad = angle * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        double centerX = x + width / 2.0;
        double centerY = y + height / 2.0;

        // lokal: gleichmäßig verteilt, bündig mit der oberen Kante
        double lx = (index - (Parameters.bricksPerLine - 1) / 2.0) * Parameters.brickSpacing;
        double ly = -height / 2.0 + LegoBrick.HeightOf(size) / 2.0;

        return new Pose(centerX + lx * cos - ly * sin, centerY + lx * sin + ly * cos, angle);
    }

    private void CreateBricks(Parameters.LegoColor color)
    {
        Parameters.LegoSize size = Parameters.LegoSize.medium;

        for (int i = 0; i < Parameters.bricksPerLine; i++)
        {
            LegoBrick brick = new LegoBrick(SlotPose(i, size), color, size);
            brick.SetzeHeimat(this);
            bricks.Add(brick);
        }
    }

    // Ein Schritt der Pendelbewegung entlang der Längsachse der Linie
    public void Bewegen()
    {
        if (bricks.Count == 0) return;

        bool forward = tickCount < Parameters.stepsPerDirection;
        tickCount = (tickCount + 1) % (2 * Parameters.stepsPerDirection);

        // Jeder Schritt verschiebt die Steine um 'step' Pixel; wir merken uns die Summe
        versatz += forward ? Parameters.step : -Parameters.step;

        Parameters.Direction direction = DirectionAlongLine(forward);
        foreach (LegoBrick brick in bricks)
        {
            // Nur Steine, die wirklich auf der Linie liegen, pendeln mit
            // (gegriffene, gelieferte oder recycelte Steine gehören gerade nicht dazu)
            if (brick.Status != Parameters.BrickStatus.OnProductionLine) continue;
            brick.Bewegen(direction);
        }
    }

    // Ein ausgeliefertes Stein kommt "neu produziert" auf seinen alten Platz zurück.
    // Die anderen Steine pendeln gerade um 'versatz' aus der Grundstellung. Der Ersatzstein wird deshalb
    // auf Grundplatz + versatz gesetzt, dann hat er denselben Abstand zu seinen Nachbarn wie vorher.
    public void Heimkehr(LegoBrick brick)
    {
        int ux = Math.Sign((int)Math.Round(Math.Cos(angle * Math.PI / 180.0)));
        int uy = Math.Sign((int)Math.Round(Math.Sin(angle * Math.PI / 180.0)));

        brick.SetzePose(brick.HeimX + ux * versatz, brick.HeimY + uy * versatz, angle);
        brick.Status = Parameters.BrickStatus.OnProductionLine;
        brick.RecyclePlatz = -1;
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
