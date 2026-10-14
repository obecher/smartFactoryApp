using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Factory factory;

    public double RobotX => factory.RobotX;
    public double RobotY => factory.RobotY;
    public double RobotAngle => factory.RobotAngle;

    public double RobotWidth => Parameters.robotWidth;
    public double RobotHeight => Parameters.robotHeight;

    // Rote Anzeige der aktuellen Roboter-Koordinate
    public string PositionText => $"X: {factory.RobotX}  Y: {factory.RobotY}";

    public MainViewModel()
    {
        factory = new Factory();
    }

    public void MoveUp() => Move(Parameters.Direction.up);
    public void MoveDown() => Move(Parameters.Direction.down);
    public void MoveLeft() => Move(Parameters.Direction.left);
    public void MoveRight() => Move(Parameters.Direction.right);
    public void MoveUpLeft() => Move(Parameters.Direction.upLeft);
    public void MoveUpRight() => Move(Parameters.Direction.upRight);
    public void MoveDownLeft() => Move(Parameters.Direction.downLeft);
    public void MoveDownRight() => Move(Parameters.Direction.downRight);

    public void RotateLeft() => Rotate(Parameters.Rotation.left);
    public void RotateRight() => Rotate(Parameters.Rotation.right);

    private void Move(Parameters.Direction direction)
    {
        factory.RobotBewegen(direction);
        NotifyRobotPositionChanged();
    }

    private void Rotate(Parameters.Rotation rotation)
    {
        factory.RobotDrehen(rotation);
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