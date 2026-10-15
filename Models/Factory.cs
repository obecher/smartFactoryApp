using System.Collections.Generic;
using System.Drawing;

namespace MyAvaloniaApp.Models;

public class Factory
{
    private Robot robot;
    private List<LegoBrick> bricks = new List<LegoBrick>();

    // Zähler für die Hin- und Herbewegung der Legosteine
    private int tickCount = 0;

    // Properties immer groß schreiben
    public int RobotX => robot.Location.X;
    public int RobotY => robot.Location.Y;
    public int RobotAngle => robot.Angle;

    public IReadOnlyList<LegoBrick> Bricks => bricks;

    public Factory()
    {
        // Erlaubter Bereich für die linke obere Ecke: grauer Bereich minus Robotergröße
        Rectangle borders = new Rectangle(
            Parameters.arenaLeft,
            Parameters.arenaTop,
            Parameters.arenaSize - Parameters.robotWidth,
            Parameters.arenaSize - Parameters.robotHeight);

        // Start: Roboter mittig im grauen Bereich
        Point start = new Point(
            Parameters.arenaLeft + (Parameters.arenaSize - Parameters.robotWidth) / 2,
            Parameters.arenaTop + (Parameters.arenaSize - Parameters.robotHeight) / 2);

        robot = new Robot(start, borders);

        CreateBricks();
    }

    // Vier Produktionslinien, punktsymmetrisch um den Mittelpunkt des Plans
    private void CreateBricks()
    {
        int center = Parameters.arenaLeft + Parameters.arenaSize / 2;
        int offset = Parameters.lineOffset;

        // Letzter Parameter dirX: 1 = Linie läuft von links oben nach rechts unten,
        //                        -1 = Linie läuft von rechts oben nach links unten
        CreateLine(Parameters.LegoColor.red,    center - offset, center - offset, -1);
        CreateLine(Parameters.LegoColor.blue,   center + offset, center - offset,  1);
        CreateLine(Parameters.LegoColor.yellow, center - offset, center + offset,  1);
        CreateLine(Parameters.LegoColor.green,  center + offset, center + offset, -1);
    }

    private void CreateLine(Parameters.LegoColor color, int centerX, int centerY, int dirX)
    {
        // Bewegungsrichtung entlang der Linie
        Parameters.Direction forward = dirX > 0 ? Parameters.Direction.downRight : Parameters.Direction.downLeft;
        Parameters.Direction backward = dirX > 0 ? Parameters.Direction.upLeft : Parameters.Direction.upRight;

        for (int i = 0; i < Parameters.bricksPerLine; i++)
        {
            // Position relativ zur Mitte der Linie: bei 4 Steinen -1.5, -0.5, 0.5, 1.5
            double k = i - (Parameters.bricksPerLine - 1) / 2.0;
            int x = centerX + (int)(k * Parameters.brickSpacing * dirX);
            int y = centerY + (int)(k * Parameters.brickSpacing);

            // Erste Hälfte kleine, zweite Hälfte mittlere Steine
            Parameters.LegoSize size = i < Parameters.bricksPerLine / 2
                ? Parameters.LegoSize.small
                : Parameters.LegoSize.medium;

            bricks.Add(new LegoBrick(new Pose(x, y, 0), color, size, forward, backward));
        }
    }

    public void RobotBewegen(Parameters.Direction direction) => robot.Bewegen(direction);
    public void RobotDrehen(Parameters.Rotation rotation) => robot.Drehen(rotation);

    // Ein Schritt der Legosteine: 10 Ticks vorwärts, dann 10 Ticks rückwärts entlang der Linie
    public void LegoBricksBewegen()
    {
        bool forward = tickCount < 10;
        tickCount = (tickCount + 1) % 20;

        foreach (LegoBrick brick in bricks)
        {
            brick.Bewegen(forward ? brick.ForwardDirection : brick.BackwardDirection);
        }
    }
}