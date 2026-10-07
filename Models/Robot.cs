using System.Drawing;


namespace MyAvaloniaApp.Models;

internal class Robot
{
    private Point location;
    private Rectangle borders;


public Point Location
    {
        get { return location;}
        set { location = value;}
    }

public Rectangle Borders { get => borders; set => borders = value;}

public Robot(Point location, Rectangle borders)
    {
        this.location = location;
        this.borders = borders;
    }
public Robot(Point location)
    {
        Location = location;
    }
    public void Bewegen(Parameters.Direction direction)
    {
        switch (direction)
        {
            case Parameters.Direction.up:
            if(location.Y - Parameters.step >= Borders.Top)
            location = new Point(Location.X, Location.Y - Parameters.step);
            break;
            
        }

    }
}

