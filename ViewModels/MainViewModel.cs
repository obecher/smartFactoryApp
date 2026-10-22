using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Factory factory;
    private DispatcherTimer timer;        // Pendelbewegung der Legosteine (Start/Stop)
    private DispatcherTimer robotTimer;   // Dauerbewegung des Roboters (Move-Modus)

    private bool isStepMode = true;
    private Parameters.Rotation? rotation;   // aktuelle Dauerdrehung, null = keine

    // ---------- Roboter ----------
    public double RobotX => factory.RobotX;
    public double RobotY => factory.RobotY;
    public double RobotAngle => factory.RobotAngle;
    public double RobotWidth => Parameters.robotWidth;
    public double RobotHeight => Parameters.robotHeight;

    // Rote Anzeige der aktuellen Roboter-Pose (Format passend zur Sprache des Systems)
    public string PositionText => $"X: {factory.RobotX:F1}\nY: {factory.RobotY:F1}\nAngle: {factory.RobotAngle:F1}°";

    // true = Step (ein Schritt pro Klick), false = Move (Dauerbewegung bis Stop)
    public bool IsStepMode
    {
        get { return isStepMode; }
        set
        {
            if (isStepMode == value) return;
            isStepMode = value;
            OnPropertyChanged(nameof(IsStepMode));
            Stop();
        }
    }

    // ---------- Produktionslinien ----------
    public double ProductionLineWidth => ViewParameters.ProductionLineWidth;
    public double ProductionLineHeight => ViewParameters.ProductionLineHeight;
    public double ProductionDeliveryWidth => ViewParameters.ProductionDeliveryWidth;
    public double ProductionDeliveryHeight => ViewParameters.ProductionDeliveryHeight;

    public double ProductionRedX => factory.ProductionRedX;
    public double ProductionRedY => factory.ProductionRedY;
    public double ProductionRedAngle => factory.ProductionRedAngle;

    public double ProductionBlueX => factory.ProductionBlueX;
    public double ProductionBlueY => factory.ProductionBlueY;
    public double ProductionBlueAngle => factory.ProductionBlueAngle;

    public double ProductionYellowX => factory.ProductionYellowX;
    public double ProductionYellowY => factory.ProductionYellowY;
    public double ProductionYellowAngle => factory.ProductionYellowAngle;

    public double ProductionGreenX => factory.ProductionGreenX;
    public double ProductionGreenY => factory.ProductionGreenY;
    public double ProductionGreenAngle => factory.ProductionGreenAngle;

    public double ProductionRecycleX => factory.ProductionRecycleX;
    public double ProductionRecycleY => factory.ProductionRecycleY;
    public double ProductionRecycleAngle => factory.ProductionRecycleAngle;

    public double ProductionDeliveryX => factory.ProductionDeliveryX;
    public double ProductionDeliveryY => factory.ProductionDeliveryY;
    public double ProductionDeliveryAngle => factory.ProductionDeliveryAngle;

    // ---------- Legosteine ----------
    // Alle Steine für die Anzeige
    public ObservableCollection<LegoBrickViewModel> Bricks { get; } = new ObservableCollection<LegoBrickViewModel>();

    public MainViewModel()
    {
        factory = new Factory();

        foreach (var brick in factory.RedBricks) Bricks.Add(new LegoBrickViewModel(brick));
        foreach (var brick in factory.BlueBricks) Bricks.Add(new LegoBrickViewModel(brick));
        foreach (var brick in factory.YellowBricks) Bricks.Add(new LegoBrickViewModel(brick));
        foreach (var brick in factory.GreenBricks) Bricks.Add(new LegoBrickViewModel(brick));

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Parameters.timerInterval)
        };
        timer.Tick += Timer_Tick;

        robotTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Parameters.robotTimerInterval)
        };
        robotTimer.Tick += RobotTimer_Tick;
    }

    // ---------- Legosteine: Start / Stop ----------
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

    // ---------- Roboter: Richtungstasten ----------
    public void MoveUp() => Move(Parameters.Direction.up);
    public void MoveDown() => Move(Parameters.Direction.down);
    public void MoveLeft() => Move(Parameters.Direction.left);
    public void MoveRight() => Move(Parameters.Direction.right);
    public void MoveUpLeft() => Move(Parameters.Direction.upLeft);
    public void MoveUpRight() => Move(Parameters.Direction.upRight);
    public void MoveDownLeft() => Move(Parameters.Direction.downLeft);
    public void MoveDownRight() => Move(Parameters.Direction.downRight);

    public void MoveRotateLeft() => Rotate(Parameters.Rotation.left);
    public void MoveRotateRight() => Rotate(Parameters.Rotation.right);

    // Step: genau ein Schritt. Move: Richtung merken, der Timer fährt weiter bis Stop.
    private void Move(Parameters.Direction direction)
    {
        if (isStepMode)
        {
            factory.RobotBewegen(direction, Parameters.stepMove);
            NotifyRobotPositionChanged();
        }
        else
        {
            factory.RobotDirection = direction;
            robotTimer.Start();
        }
    }

    private void Rotate(Parameters.Rotation newRotation)
    {
        if (isStepMode)
        {
            factory.RobotDrehen(newRotation, Parameters.stepRotation);
            NotifyRobotPositionChanged();
        }
        else
        {
            rotation = newRotation;
            robotTimer.Start();
        }
    }

    // Stop-Taste (■): beendet Dauerbewegung und Dauerdrehung
    public void Stop()
    {
        factory.RobotDirection = Parameters.Direction.stop;
        rotation = null;
        robotTimer.Stop();
    }

    public void ResetRotation()
    {
        factory.RobotWinkelZuruecksetzen();
        NotifyRobotPositionChanged();
    }

    // Lässt den getragenen Stein los
    public void Undock()
    {
        factory.Undock();
        NotifyRobotPositionChanged();
    }

    private void RobotTimer_Tick(object? sender, EventArgs e)
    {
        if (factory.RobotDirection != Parameters.Direction.stop)
        {
            factory.RobotBewegen(factory.RobotDirection, Parameters.step);
        }

        if (rotation != null)
        {
            factory.RobotDrehen(rotation.Value, Parameters.rotationStep);
        }

        NotifyRobotPositionChanged();
    }

    // Roboter und (getragene) Steine neu zeichnen
    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
        OnPropertyChanged(nameof(PositionText));
        NotifyLegoBricksPositionChanged();
    }
}
