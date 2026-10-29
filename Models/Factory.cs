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

    // alle Linien (Hindernisse für den Roboter) und alle Steine (greifbar)
    private List<ProductionLine> productionLines = new List<ProductionLine>();
    private List<LegoBrick> allBricks = new List<LegoBrick>();

    // Jeder zweite Stein ist defekt: abwechselnd Kunde, Recycling, Kunde, Recycling, ...
    private bool naechsterZumRecycling = false;

    // Ein Stein ist beim Kunden angekommen (und wurde als Ersatz neu "produziert")
    public event Action<LegoBrick>? BrickDelivered;

    // Ein defekter Stein ist auf der Recyclinglinie angekommen
    public event Action<LegoBrick>? BrickRecycled;

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

        allBricks.AddRange(productionRed.Bricks);
        allBricks.AddRange(productionBlue.Bricks);
        allBricks.AddRange(productionYellow.Bricks);
        allBricks.AddRange(productionGreen.Bricks);

        robot = new Robot(ViewParameters.RobotPose, ViewParameters.Borders);
        robot.ProductionLines = productionLines;
        robot.Bricks = allBricks;
        robot.DeliveryLine = productionDelivery;
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

        LegoBrick brick = robot.Undock()!;

        Parameters.UndockResult result;
        if (naechsterZumRecycling)
        {
            brick.ZumRecycling(productionDelivery);
            result = Parameters.UndockResult.ToRecycle;
        }
        else
        {
            brick.ZumKunden(productionDelivery);
            result = Parameters.UndockResult.ToCustomer;
        }

        naechsterZumRecycling = !naechsterZumRecycling;
        return result;
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
                        BrickRecycled?.Invoke(brick);
                    }
                    else
                    {
                        // Kein Platz frei: der Stein wartet am Ende des Pfeils
                        brick.SetzeMitte(toRecycleLine.X, brick.Y + brick.Height / 2.0, 0);
                    }
                }
            }
        }

        return aktiv;
    }
}
