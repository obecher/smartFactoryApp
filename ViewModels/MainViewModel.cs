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

    private bool isStepMode = true;
    private Parameters.Rotation? rotation;   // aktuelle Dauerdrehung, null = keine

    private string orderInfo = "No order yet: click \"Order\"";
    private string undockInfo = "";

    // ---------- Roboter ----------
    public double RobotX => factory.RobotX;
    public double RobotY => factory.RobotY;
    public double RobotAngle => factory.RobotAngle;
    public double RobotWidth => Parameters.robotWidth;
    public double RobotHeight => Parameters.robotHeight;

    public string PositionText => $"X: {factory.RobotX:F1}\nY: {factory.RobotY:F1}\nAngle: {factory.RobotAngle:F1}°";

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

    public double DeliveryStripX => ViewParameters.ToRecycleLinePosition.X;
    public double DeliveryStripY => ViewParameters.ToRecycleLinePosition.Y;
    public double DeliveryStripWidth => ViewParameters.DeliveryStripWidth;
    public double DeliveryStripHeight => ViewParameters.ProductionDeliveryHeight;

    // ---------- Bestellung (Order) und Auslieferung (Delivered) ----------
    public int YellowOrder => factory.GetOrder(Parameters.LegoColor.yellow);
    public int BlueOrder => factory.GetOrder(Parameters.LegoColor.blue);
    public int RedOrder => factory.GetOrder(Parameters.LegoColor.red);
    public int GreenOrder => factory.GetOrder(Parameters.LegoColor.green);

    public int YellowDelivered => factory.GetDelivered(Parameters.LegoColor.yellow);
    public int BlueDelivered => factory.GetDelivered(Parameters.LegoColor.blue);
    public int RedDelivered => factory.GetDelivered(Parameters.LegoColor.red);
    public int GreenDelivered => factory.GetDelivered(Parameters.LegoColor.green);

    // Der Plan der Logistik (blauer Text): "Step 4/8: Pick blue brick from ..."
    public string OrderInfo
    {
        get { return orderInfo; }
        private set
        {
            orderInfo = value;
            OnPropertyChanged(nameof(OrderInfo));
        }
    }

    // Rückmeldungen des Roboters (schwarzer Text): Undock, GoTo, Recycling, ...
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
    public ObservableCollection<LegoBrickViewModel> Bricks { get; } = new ObservableCollection<LegoBrickViewModel>();

    public MainViewModel()
    {
        factory = new Factory();

        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Parameters.timerInterval) };
        timer.Tick += Timer_Tick;

        robotTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Parameters.robotTimerInterval) };
        robotTimer.Tick += RobotTimer_Tick;

        deliveryTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Parameters.deliveryTimerInterval) };
        deliveryTimer.Tick += DeliveryTimer_Tick;

        factory.BrickDelivered += Factory_BrickDelivered;
        factory.BrickRecycled += Factory_BrickRecycled;
        factory.RecycleLineFull += Factory_RecycleLineFull;
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

    // ---------- Setup Production Lines ----------
    private bool productionLinesSetUp = false;

    public bool CanSetupProductionLines => !productionLinesSetUp;

    public void SetupProductionLines()
    {
        if (productionLinesSetUp) return;

        factory.SetupProductionLines();

        foreach (var brick in factory.AlleBricks)
        {
            Bricks.Add(new LegoBrickViewModel(brick));
        }

        productionLinesSetUp = true;
        OnPropertyChanged(nameof(CanSetupProductionLines));

        if (factory.OrderActive)
        {
            RefreshPlan();
        }
    }

    // ---------- Order und Plan ----------
    public void CreateOrder()
    {
        if (!factory.CreateOrder())
        {
            UndockInfo = "The current order is not finished yet.";
            return;
        }

        NotifyOrderChanged();
        UpdateOrderInfo();
        UndockInfo = "New order created.";
    }

    private void RefreshPlan()
    {
        factory.PlanNextStep();
        NotifyOrderChanged();
        UpdateOrderInfo();
    }

    private void NotifyOrderChanged()
    {
        OnPropertyChanged(nameof(YellowOrder));
        OnPropertyChanged(nameof(BlueOrder));
        OnPropertyChanged(nameof(RedOrder));
        OnPropertyChanged(nameof(GreenOrder));
        OnPropertyChanged(nameof(YellowDelivered));
        OnPropertyChanged(nameof(BlueDelivered));
        OnPropertyChanged(nameof(RedDelivered));
        OnPropertyChanged(nameof(GreenDelivered));
    }

    private void UpdateOrderInfo()
    {
        if (!factory.HasOrder)
        {
            OrderInfo = "No order yet: click \"Order\"";
        }
        else if (factory.OrderComplete)
        {
            OrderInfo = "Order complete! Click \"Order\" for a new order.";
        }
        else if (factory.AllBricksSent)
        {
            OrderInfo = "All bricks of the order are on their way to the customers.";
        }
        else if (factory.AlleBricks.Count == 0)
        {
            OrderInfo = "Click \"Setup Production Lines\" to create the bricks.";
        }
        else if (factory.CurrentPlan is PickPlan plan)
        {
            OrderInfo = $"Step {factory.CurrentStep}/{factory.TotalOrdered}: Pick {plan.Color} brick from {PlaceText(plan.Station)} and bring to delivery";
        }
        else
        {
            OrderInfo = $"Step {factory.CurrentStep}/{factory.TotalOrdered}: No brick of a needed color is available right now.";
        }
    }

    private static string PlaceText(Parameters.PickStation station)
    {
        return station == Parameters.PickStation.recycle
            ? "the recycle line"
            : $"{station} production line";
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
    public void GoToPickUp()
    {
        Stop();

        PickPlan? plan = factory.EnsurePlan();
        UpdateOrderInfo();

        if (plan == null)
        {
            UndockInfo = factory.HasOrder
                ? "There is nothing to pick up right now."
                : "No order yet: click \"Order\" first.";
            return;
        }

        bool ok = factory.GoToLine(plan.Value.Station);

        UndockInfo = ok
            ? $"Robot at the {plan.Value.Station} line: drive forward to dock the brick."
            : "Robot cannot go there (blocked).";

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

    // ---------- Undock ----------
    public void Undock()
    {
        Parameters.UndockResult result = factory.Undock();

        UndockInfo = result switch
        {
            Parameters.UndockResult.ToCustomer => "Undocking successful: the brick is sent to the customers.",
            Parameters.UndockResult.ToRecycle => "Undocking successful: the brick is defective and sent to recycling.",
            Parameters.UndockResult.NotNeeded => "This color is not needed for the order. The brick is sent to recycling.",
            Parameters.UndockResult.NoOrder => "There is no active order. Click \"Order\" first.",
            Parameters.UndockResult.NoBrick => "No brick available to undock.",
            Parameters.UndockResult.NotOnDeliveryLine => "Undocking is only possible on the delivery line.",
            _ => ""
        };

        bool brickLosgelassen = result == Parameters.UndockResult.ToCustomer
                             || result == Parameters.UndockResult.ToRecycle
                             || result == Parameters.UndockResult.NotNeeded;

        if (brickLosgelassen)
        {
            deliveryTimer.Start();
            RefreshPlan();
        }

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

    private void DeliveryTimer_Tick(object? sender, EventArgs e)
    {
        bool aktiv = factory.LogisticsBewegen();
        NotifyLegoBricksPositionChanged();

        if (!aktiv)
        {
            deliveryTimer.Stop();
        }
    }

    // ---------- Ereignisse der Factory ----------
    private void Factory_BrickDelivered(Parameters.LegoColor color)
    {
        UndockInfo = $"Brick delivered ({color}).";
        RefreshPlan();
    }

    private void Factory_BrickRecycled(LegoBrick brick)
    {
        UndockInfo = $"Brick ({brick.Color}) arrived at the recycling line.";
        RefreshPlan();
    }

    private void Factory_RecycleLineFull(LegoBrick brick)
    {
        UndockInfo = $"Recycling line is full ({Parameters.bricksPerLine} of {Parameters.bricksPerLine}). The brick waits until a place is free.";
    }

    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
        OnPropertyChanged(nameof(PositionText));
        NotifyLegoBricksPositionChanged();
    }
}
