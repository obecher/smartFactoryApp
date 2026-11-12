using System;
using System.Collections.Generic;

namespace MyAvaloniaApp.Models;

public class Factory
{
    private Robot robot;
    private ProductionLine productionRed;
    private ProductionLine productionBlue;
    private ProductionLine productionYellow;
    private ProductionLine productionGreen;
    private ProductionLine productionRecycle;
    private ProductionLine productionDelivery;
    private ProductionLine toRecycleLine;       // Pfeil-Feld links neben der Lieferlinie
    private ProductionLine toCustomerLine;      // Pfeil-Feld rechts neben der Lieferlinie

    private Logistics logistics = new Logistics();

    // alle Linien (Hindernisse für den Roboter) und alle Steine (greifbar)
    private List<ProductionLine> productionLines = new List<ProductionLine>();
    private List<LegoBrick> allBricks = new List<LegoBrick>();

    // Jeder zweite Stein ist defekt: abwechselnd Kunde, Recycling, Kunde, Recycling, ...
    private Random random = new Random();

    // Ein Stein ist beim Kunden angekommen (und wurde als Ersatz neu "produziert")
    public event Action<LegoBrick>? BrickDelivered;

    // Ein defekter Stein ist auf der Recyclinglinie angekommen
    public event Action<LegoBrick>? BrickRecycled;
    public event Action<LegoBrick>? RecycleLineFull;

    // ---- Roboter ----
    public double RobotX => robot.X;
    public double RobotY => robot.Y;
    public double RobotAngle => robot.Angle;

    public Parameters.Direction RobotDirection
    {
        get { return robot.RobotDirection; }
        set { robot.RobotDirection = value; }
    }

    // ---- Produktionslinien ----
    public double ProductionRedX => productionRed.X;
    public double ProductionRedY => productionRed.Y;
    public double ProductionRedAngle => productionRed.Angle;

    public double ProductionBlueX => productionBlue.X;
    public double ProductionBlueY => productionBlue.Y;
    public double ProductionBlueAngle => productionBlue.Angle;

    public double ProductionYellowX => productionYellow.X;
    public double ProductionYellowY => productionYellow.Y;
    public double ProductionYellowAngle => productionYellow.Angle;

    public double ProductionGreenX => productionGreen.X;
    public double ProductionGreenY => productionGreen.Y;
    public double ProductionGreenAngle => productionGreen.Angle;

    public double ProductionRecycleX => productionRecycle.X;
    public double ProductionRecycleY => productionRecycle.Y;
    public double ProductionRecycleAngle => productionRecycle.Angle;

    public double ProductionDeliveryX => productionDelivery.X;
    public double ProductionDeliveryY => productionDelivery.Y;
    public double ProductionDeliveryAngle => productionDelivery.Angle;

// ---- Bestellung und Plan ----
public bool HasOrder => logistics.HasOrder;
public bool OrderActive => logistics.OrderActive;
public bool OrderComplete => logistics.OrderComplete;
public bool AllBricksSent => logistics.AllBricksSent;
public int TotalOrdered => logistics.TotalOrdered;
public int CurrentStep => logistics.CurrentStep;
public PickPlan? CurrentPlan => logistics.NextPick;

// Der Stein, den der Roboter gerade trägt (null = keiner)
public LegoBrick? DockedBrick
{
    get
    {
        foreach (LegoBrick brick in allBricks)
        {
            if (brick.Gedockt) return brick;
        }
        return null;
    }
}

public int GetOrder(Parameters.LegoColor color) => logistics.GetOrder(color);
public int GetDelivered(Parameters.LegoColor color) => logistics.GetDelivered(color);

    // ---- Legosteine ----
    public IReadOnlyList<LegoBrick> RedBricks => productionRed.Bricks;
    public IReadOnlyList<LegoBrick> BlueBricks => productionBlue.Bricks;
    public IReadOnlyList<LegoBrick> YellowBricks => productionYellow.Bricks;
    public IReadOnlyList<LegoBrick> GreenBricks => productionGreen.Bricks;
    public IReadOnlyList<LegoBrick> AlleBricks => allBricks;

    public Factory()
    {
        productionRed = new ProductionLine(ViewParameters.RectRedPose, Parameters.LegoColor.red);
        productionBlue = new ProductionLine(ViewParameters.RectBluePose, Parameters.LegoColor.blue);
        productionYellow = new ProductionLine(ViewParameters.RectYellowPose, Parameters.LegoColor.yellow);
        productionGreen = new ProductionLine(ViewParameters.RectGreenPose, Parameters.LegoColor.green);

        // Recycling-, Liefer- und Pfeil-Felder haben keine Farbe und anfangs keine Steine
        productionRecycle = new ProductionLine(ViewParameters.RectRecyclePose,
            ViewParameters.ProductionLineWidth, ViewParameters.ProductionLineHeight);
        productionDelivery = new ProductionLine(ViewParameters.RectDeliveryPose,
            ViewParameters.ProductionDeliveryWidth, ViewParameters.ProductionDeliveryHeight);
        toRecycleLine = new ProductionLine(ViewParameters.ToRecycleLinePosition,
            ViewParameters.ToRecycleLineWidth, ViewParameters.ToRecycleLineHeight);
        toCustomerLine = new ProductionLine(ViewParameters.ToCustomerLinePosition,
            ViewParameters.ToCustomerLineWidth, ViewParameters.ToCustomerLineHeight);

        productionLines.Add(productionRed);
        productionLines.Add(productionBlue);
        productionLines.Add(productionYellow);
        productionLines.Add(productionGreen);
        productionLines.Add(productionRecycle);
        productionLines.Add(productionDelivery);
        productionLines.Add(toRecycleLine);
        productionLines.Add(toCustomerLine);

        robot = new Robot(ViewParameters.RobotPose, ViewParameters.Borders);
        robot.ProductionLines = productionLines;
        robot.Bricks = allBricks;
        robot.DeliveryLine = productionDelivery;
    }
    // Button "Order": würfelt eine Bestellung und plant gleich den ersten Stein
public bool CreateOrder()
{
    bool neu = logistics.CreateRandomOrder();
    if (neu) logistics.PlanNextStep(allBricks);
    return neu;
}

public PickPlan? PlanNextStep() => logistics.PlanNextStep(allBricks);
public PickPlan? EnsurePlan() => logistics.EnsurePlan(allBricks);

public void SetupProductionLines()
{
    if (allBricks.Count > 0) return;   // nur einmal

    productionRed.CreateBricks();
    productionBlue.CreateBricks();
    productionYellow.CreateBricks();
    productionGreen.CreateBricks();

    allBricks.AddRange(productionRed.Bricks);
    allBricks.AddRange(productionBlue.Bricks);
    allBricks.AddRange(productionYellow.Bricks);
    allBricks.AddRange(productionGreen.Bricks);
}
    public void RobotBewegen(Parameters.Direction direction, double distance) => robot.Bewegen(direction, distance);
    public void RobotDrehen(Parameters.Rotation rotation, double degrees) => robot.Drehen(rotation, degrees);
    public void RobotWinkelZuruecksetzen() => robot.ResetRotation();

    // Alle vier Produktionslinien machen einen Schritt (Pendelbewegung der Steine)
    public void LegoBricksBewegen()
    {
        productionRed.Bewegen();
        productionBlue.Bewegen();
        productionYellow.Bewegen();
        productionGreen.Bewegen();
    }

    // ---------- Undock und Logistik ----------

    // Undock-Taste: Der Stein darf NUR auf der Lieferlinie losgelassen werden.
    // Danach entscheidet die Qualität: jeder zweite Stein ist defekt und geht zum Recycling.
    public Parameters.UndockResult Undock()
    {
        if (!robot.HatDockobjekt)
        {
            return Parameters.UndockResult.NoBrick;
        }

        if (!robot.AufDeliveryLinie())
        {
            return Parameters.UndockResult.NotOnDeliveryLine;
        }

// Ohne aktive Bestellung gibt es nichts zu liefern. Der Stein bleibt am Greifer.
if (!logistics.OrderActive)
{
    return Parameters.UndockResult.NoOrder;
}

LegoBrick brick = robot.Undock()!;

// 1. Wird die Farbe gebraucht? Wenn nicht: Recycling (als Reserve)
if (!logistics.IsNeeded(brick.Color))
{
    brick.ZumRecycling(productionDelivery);
    return Parameters.UndockResult.NotNeeded;
}

// 2. Qualitätskontrolle: 50 % defekt
bool defekt = random.Next(2) == 0;
if (defekt)
{
    brick.ZumRecycling(productionDelivery);
    return Parameters.UndockResult.ToRecycle;
}

// 3. In Ordnung: zum Kunden. Ab jetzt zählt der Stein als "unterwegs".
logistics.BrickSent(brick.Color);
brick.ZumKunden(productionDelivery);
return Parameters.UndockResult.ToCustomer;
    }

    // Nummer des ersten freien Platzes auf der Recyclinglinie, -1 wenn alle belegt sind
    public int FreierRecyclePlatz()
    {
        for (int platz = 0; platz < Parameters.bricksPerLine; platz++)
        {
            bool belegt = false;
            foreach (LegoBrick brick in allBricks)
            {
                if (brick.Status == Parameters.BrickStatus.OnRecycleLine && brick.RecyclePlatz == platz)
                {
                    belegt = true;
                    break;
                }
            }

            if (!belegt) return platz;
        }
        return -1;
    }

    // Mittelpunkt und Winkel eines Platzes auf der Recyclinglinie
    public Pose RecyclePlatzPose(int platz, Parameters.LegoSize size)
    {
        return productionRecycle.SlotPose(platz, size);
    }

    // Ein Schritt für alle Steine auf der Lieferlinie. Rückgabe: true, solange noch Steine unterwegs sind.
    public bool LogisticsBewegen()
    {
        bool aktiv = false;

        foreach (LegoBrick brick in allBricks)
        {
            if (brick.Status == Parameters.BrickStatus.ToCustomer)
            {
                aktiv = true;
                brick.SetzePose(brick.X + Parameters.deliveryStep, brick.Y, 0);

                // Mitte hat das rechte Ende des Kunden-Pfeils erreicht: der Stein verschwindet und
                // taucht als "neu produzierter" Stein wieder auf seinem alten Platz auf
                if (brick.X + brick.Width / 2.0 >= toCustomerLine.X + toCustomerLine.Width)
                    {
                        logistics.BrickDelivered(brick.Color);   // NEU: zählt Delivered +1, Underway -1
                             brick.Heimlinie!.Heimkehr(brick);
                            BrickDelivered?.Invoke(brick);
                        }
            }
            else if (brick.Status == Parameters.BrickStatus.ToRecycle)
            {
                aktiv = true;
                brick.SetzePose(brick.X - Parameters.deliveryStep, brick.Y, 0);

                // Mitte hat das linke Ende des Recycling-Pfeils erreicht: weiter zur Recyclinglinie.
                // Es wird KEIN neuer Stein produziert.
                if (brick.X + brick.Width / 2.0 <= toRecycleLine.X)
                {
                    int platz = FreierRecyclePlatz();
                    if (platz >= 0)
                    {
                        Pose pose = RecyclePlatzPose(platz, brick.Size);
                        brick.SetzeMitte(pose.X, pose.Y, pose.Angle);
                        brick.Status = Parameters.BrickStatus.OnRecycleLine;
                        brick.RecyclePlatz = platz;
                        brick.WartetAufRecycling = false; 
                        BrickRecycled?.Invoke(brick);
                    }
                    else
                    {
                    brick.SetzeMitte(toRecycleLine.X, brick.Y + brick.Height / 2.0, 0);

                    if (!brick.WartetAufRecycling)
                        {
                        brick.WartetAufRecycling = true;
                        RecycleLineFull?.Invoke(brick);   // nur einmal melden, nicht bei jedem Tick
                        }
                    }
                }
            }
        }

        return aktiv;
    }

    public bool GoToLine(Parameters.PickStation station)
{
    Pose pose;
    switch (station)
    {
        case Parameters.PickStation.yellow:
            pose = new Pose(155, 344, ViewParameters.GoToYellowAngle);
            break;

        case Parameters.PickStation.blue:
        pose = new Pose(358.5, 180.5, ViewParameters.GoToBlueAngle);
        break;

         case Parameters.PickStation.red:
        pose = new Pose(211.5, 180.5, ViewParameters.GoToRedAngle);
        break; 

        case Parameters.PickStation.green:
        pose = new Pose(415, 344, ViewParameters.GoToGreenAngle);
        break; 

        case Parameters.PickStation.recycle:
        pose = new Pose(285, 204, ViewParameters.GoToRecycleAngle);
        break; 

        
        default:
            return false;
    }
    return robot.GeheZuLinie(pose);
}

public bool GoToDelivery()
{
    return robot.GeheZuLinie(new Pose(300, 97, ViewParameters.GoToDeliveryAngle));
}

}
