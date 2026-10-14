using System.Drawing;
using MyAvaloniaApp.Models;
using static MyAvaloniaApp.ViewParameters;
using static MyAvaloniaApp.Parameters;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

namespace MyAvaloniaApp.ViewModels;

public class Factory
{
    Robot robot;
    List <LegoBrick> bricks = new List <LegoBricks>();

    // Properties immer groß schreiben
    public int RobotX => robot.Location.X;
    public int RobotY => robot.Location.Y;
    public int RobotAngle => robot.Angle;

    public Factory()
    {

        robot = new Robot(RobotPose.Location, Borders);
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
    }

private void CreateBricks()
    {
    bricks.Add(new LegoBrick(BrickPose, LegoColor.red, legoSize.small));
    }

    public void RobotBewegen(Parameters.Direction direction) => robot.Bewegen(direction);
    public void RobotDrehen(Parameters.Rotation rotation) => robot.Drehen(rotation);

    int tickCount = 0;
    

    public void LegoBricksBewegen()
    {
        bool outward = tickCount < 10;
        tickCount = (tickCount +1) % 20;
        foreach (LegoBrick brick in Bricks)
        {
            if(outward)
            brick.Bewegen(Parameters.Direction.right);
            else
            brick.Bewegen(Parameters.Direction.left);
        }
    }
}