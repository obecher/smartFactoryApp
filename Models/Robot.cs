using System;
using System.Drawing;

namespace MyAvaloniaApp.Models;

internal class Robot
{
    private Point location;
    private Rectangle borders;
    private int angle;

    public Point Location
    {
        get { return location; }
        set { location = value; }
    }

    // Erlaubter Bereich für die linke obere Ecke des Roboters.
    // Ein leeres Rectangle bedeutet: keine Begrenzung.
    public Rectangle Borders
    {
        get { return borders; }
        set { borders = value; }
    }

    // Drehwinkel in Grad (0..359)
    public int Angle
    {
        get { return angle; }
    }

    public Robot(Point location, Rectangle borders)
    {
        this.location = location;
        this.borders = borders;
        this.angle = 0;
    }

    public Robot(Point location)
    {
        this.location = location;
        this.borders = Rectangle.Empty;
        this.angle = 0;
    }

    public void Bewegen(Parameters.Direction direction)
    {
        int dx = 0;
        int dy = 0;

        switch (direction)
        {
            case Parameters.Direction.up:
                dy = -Parameters.step;
                break;
            case Parameters.Direction.down:
                dy = Parameters.step;
                break;
            case Parameters.Direction.left:
                dx = -Parameters.step;
                break;
            case Parameters.Direction.right:
                dx = Parameters.step;
                break;
            case Parameters.Direction.upLeft:
                dx = -Parameters.step;
                dy = -Parameters.step;
                break;
            case Parameters.Direction.upRight:
                dx = Parameters.step;
                dy = -Parameters.step;
                break;
            case Parameters.Direction.downLeft:
                dx = -Parameters.step;
                dy = Parameters.step;
                break;
            case Parameters.Direction.downRight:
                dx = Parameters.step;
                dy = Parameters.step;
                break;
            default:
                break;
        }

        int newX = location.X + dx;
        int newY = location.Y + dy;

        if (!borders.IsEmpty)
        {
            newX = Math.Clamp(newX, borders.Left, borders.Right);
            newY = Math.Clamp(newY, borders.Top, borders.Bottom);
        }

        location = new Point(newX, newY);
    }

    public void Drehen(Parameters.Rotation rotation)
    {
        int delta = Parameters.rotationStep;
        if (rotation == Parameters.Rotation.left)
        {
            delta = -Parameters.rotationStep;
        }

        angle = (angle + delta + 360) % 360;
    }
}