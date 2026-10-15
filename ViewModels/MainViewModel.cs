using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Factory factory;
    private DispatcherTimer timer;

    public double RobotX => factory.RobotX;
    public double RobotY => factory.RobotY;
    public double RobotAngle => factory.RobotAngle;

    public double RobotWidth => Parameters.robotWidth;
    public double RobotHeight => Parameters.robotHeight;

    // Rote Anzeige der aktuellen Roboter-Koordinate
    public string PositionText => $"X: {factory.RobotX}  Y: {factory.RobotY}";

    // Alle Legosteine für die Anzeige
    public ObservableCollection<LegoBrickViewModel> Bricks { get; } = new ObservableCollection<LegoBrickViewModel>();

    public MainViewModel()
    {
        factory = new Factory();

        foreach (var brick in factory.Bricks)
        {
            Bricks.Add(new LegoBrickViewModel(brick));
        }

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Parameters.timerInterval)
        };
        timer.Tick += Timer_Tick;
    }

    public void StartTimer() => timer.Start();
    public void StopTimer() => timer.Stop();

    private void Timer_Tick(object? sender, EventArgs e)
    {
        factory.LegoBricksBewegen();
        NotifyLegoBricksPositionChanged();
    }

    private void NotifyLegoBricksPositionChanged()
    {
        foreach (var brick in Bricks)
        {
            brick.Refresh();
        }
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