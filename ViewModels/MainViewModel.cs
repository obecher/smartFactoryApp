using System.Drawing;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Robot robot;

    public double RobotX
    {
        get { return robot.Location.X; }
    }

    public double RobotY
    {
        get { return robot.Location.Y; }
    }

    public double RobotAngle
    {
        get { return robot.Angle; }
    }

    public double RobotWidth
    {
        get { return Parameters.robotWidth; }
    }

    public double RobotHeight
    {
        get { return Parameters.robotHeight; }
    }

    // Rote Anzeige der aktuellen Roboter-Koordinate
    public string PositionText
    {
        get { return $"X: {robot.Location.X}  Y: {robot.Location.Y}"; }
    }

    public MainViewModel()
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
    }

    public void MoveUp()
    {
        robot.Bewegen(Parameters.Direction.up);
        NotifyRobotPositionChanged();
    }

    public void MoveDown()
    {
        robot.Bewegen(Parameters.Direction.down);
        NotifyRobotPositionChanged();
    }

    public void MoveLeft()
    {
        robot.Bewegen(Parameters.Direction.left);
        NotifyRobotPositionChanged();
    }

    public void MoveRight()
    {
        robot.Bewegen(Parameters.Direction.right);
        NotifyRobotPositionChanged();
    }

    public void MoveUpLeft()
    {
        robot.Bewegen(Parameters.Direction.upLeft);
        NotifyRobotPositionChanged();
    }

    public void MoveUpRight()
    {
        robot.Bewegen(Parameters.Direction.upRight);
        NotifyRobotPositionChanged();
    }

    public void MoveDownLeft()
    {
        robot.Bewegen(Parameters.Direction.downLeft);
        NotifyRobotPositionChanged();
    }

    public void MoveDownRight()
    {
        robot.Bewegen(Parameters.Direction.downRight);
        NotifyRobotPositionChanged();
    }

    public void RotateLeft()
    {
        robot.Drehen(Parameters.Rotation.left);
        NotifyRobotPositionChanged();
    }

    public void RotateRight()
    {
        robot.Drehen(Parameters.Rotation.right);
        NotifyRobotPositionChanged();
    }

    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
        OnPropertyChanged(nameof(PositionText));
    }
}