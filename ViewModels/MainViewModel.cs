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
    private DispatcherTimer deliveryTimer; // Steine auf der Lieferlinie (läuft nur, solange welche unterwegs sind)

    private int yellowDelivered;
    private int blueDelivered;
    private int redDelivered;
    private int greenDelivered;
    private string orderInfo = "";
    private string undockInfo = "";

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

    // Bild mit den drei Feldern For Recycling / Delivery / To Customers
    public double DeliveryStripX => ViewParameters.ToRecycleLinePosition.X;
    public double DeliveryStripY => ViewParameters.ToRecycleLinePosition.Y;
    public double DeliveryStripWidth => ViewParameters.DeliveryStripWidth;
    public double DeliveryStripHeight => ViewParameters.ProductionDeliveryHeight;

    // ---------- Auslieferung ----------
    public int YellowDelivered => yellowDelivered;
    public int BlueDelivered => blueDelivered;
    public int RedDelivered => redDelivered;
    public int GreenDelivered => greenDelivered;

    // Meldung zur Auslieferung ("Brick delivered", ...)
    public string OrderInfo
    {
        get { return orderInfo; }
        private set
        {
            orderInfo = value;
            OnPropertyChanged(nameof(OrderInfo));
        }
    }

    // Rückmeldung des Undock-Buttons
    public string UndockInfo
    {
        get { return undockInfo; }
        private set
        {
            undockInfo = value;
            OnPropertyChanged(nameof(UndockInfo));
        }
    }

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

        deliveryTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Parameters.deliveryTimerInterval)
        };
        deliveryTimer.Tick += DeliveryTimer_Tick;

        factory.BrickDelivered += Factory_BrickDelivered;
        factory.BrickRecycled += Factory_BrickRecycled;
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

// ---------- GoTo-Tasten ----------
// Vorläufig: bis der Logistik-Plan die Station vorgibt, geht jeder Klick zur nächsten Station
private Parameters.PickStation pickStation = Parameters.PickStation.yellow;

public void GoToPickUp()
{
    Stop();                                    // laufende Dauerbewegung beenden
    bool ok = factory.GoToLine(pickStation);   // Roboter vor die Linie setzen

    UndockInfo = ok
        ? $"Robot at the {pickStation} line: drive forward to dock the brick."
        : "Robot cannot go there (blocked).";

    // nächste Station für den nächsten Klick
    pickStation = pickStation == Parameters.PickStation.recycle
        ? Parameters.PickStation.yellow
        : pickStation + 1;

    NotifyRobotPositionChanged();
}

public void GoToDelivery()
{
    Stop();
    bool ok = factory.GoToDelivery();

    UndockInfo = ok
        ? "Robot at the delivery line: undock the brick."
        : "Robot cannot go there (blocked).";

    NotifyRobotPositionChanged();
}
    // Undock-Taste: lässt den Stein los, aber nur auf der Lieferlinie. Das Ergebnis wird angezeigt.
    public void Undock()
    {
        Parameters.UndockResult result = factory.Undock();

        UndockInfo = result switch
        {
            Parameters.UndockResult.ToCustomer => "Undocking successful: the brick is sent to the customers.",
            Parameters.UndockResult.ToRecycle => "Undocking successful: the brick is defective and sent to recycling.",
            Parameters.UndockResult.NoBrick => "No brick available to undock.",
            Parameters.UndockResult.NotOnDeliveryLine => "Undocking is only possible on the delivery line.",
            _ => ""
        };

        if (result == Parameters.UndockResult.ToCustomer || result == Parameters.UndockResult.ToRecycle)
        {
            deliveryTimer.Start();
        }

        NotifyRobotPositionChanged();
    }

    // Steine auf der Lieferlinie fahren; wenn keiner mehr unterwegs ist, hält der Timer an
    private void DeliveryTimer_Tick(object? sender, EventArgs e)
    {
        bool aktiv = factory.LogisticsBewegen();
        NotifyLegoBricksPositionChanged();

        if (!aktiv)
        {
            deliveryTimer.Stop();
        }
    }

    private void Factory_BrickDelivered(LegoBrick brick)
    {
        switch (brick.Color)
        {
            case Parameters.LegoColor.yellow:
                yellowDelivered++;
                OnPropertyChanged(nameof(YellowDelivered));
                break;
            case Parameters.LegoColor.blue:
                blueDelivered++;
                OnPropertyChanged(nameof(BlueDelivered));
                break;
            case Parameters.LegoColor.red:
                redDelivered++;
                OnPropertyChanged(nameof(RedDelivered));
                break;
            case Parameters.LegoColor.green:
                greenDelivered++;
                OnPropertyChanged(nameof(GreenDelivered));
                break;
        }

        OrderInfo = $"Brick delivered ({brick.Color})";
    }

    private void Factory_BrickRecycled(LegoBrick brick)
    {
        OrderInfo = $"Brick ({brick.Color}) arrived at the recycling line";
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