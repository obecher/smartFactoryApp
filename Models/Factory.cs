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

    // alle sechs Linien (Hindernisse für den Roboter) und alle Steine (greifbar)
    private List<ProductionLine> productionLines = new List<ProductionLine>();
    private List<LegoBrick> allBricks = new List<LegoBrick>();

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

    public Factory()
    {
        productionRed = new ProductionLine(ViewParameters.RectRedPose, Parameters.LegoColor.red);
        productionBlue = new ProductionLine(ViewParameters.RectBluePose, Parameters.LegoColor.blue);
        productionYellow = new ProductionLine(ViewParameters.RectYellowPose, Parameters.LegoColor.yellow);
        productionGreen = new ProductionLine(ViewParameters.RectGreenPose, Parameters.LegoColor.green);

        // Recycling- und Lieferlinie haben keine Farbe und keine Steine
        productionRecycle = new ProductionLine(ViewParameters.RectRecyclePose,
            ViewParameters.ProductionLineWidth, ViewParameters.ProductionLineHeight);
        productionDelivery = new ProductionLine(ViewParameters.RectDeliveryPose,
            ViewParameters.ProductionDeliveryWidth, ViewParameters.ProductionDeliveryHeight);

        productionLines.Add(productionRed);
        productionLines.Add(productionBlue);
        productionLines.Add(productionYellow);
        productionLines.Add(productionGreen);
        productionLines.Add(productionRecycle);
        productionLines.Add(productionDelivery);

        allBricks.AddRange(productionRed.Bricks);
        allBricks.AddRange(productionBlue.Bricks);
        allBricks.AddRange(productionYellow.Bricks);
        allBricks.AddRange(productionGreen.Bricks);

        robot = new Robot(ViewParameters.RobotPose, ViewParameters.Borders);
        robot.ProductionLines = productionLines;
        robot.Bricks = allBricks;
    }

    public void RobotBewegen(Parameters.Direction direction, double distance) => robot.Bewegen(direction, distance);
    public void RobotDrehen(Parameters.Rotation rotation, double degrees) => robot.Drehen(rotation, degrees);
    public void RobotWinkelZuruecksetzen() => robot.ResetRotation();

    // Lässt den getragenen Stein los
    public void Undock() => robot.Undock();

    // Alle vier Produktionslinien machen einen Schritt
    public void LegoBricksBewegen()
    {
        productionRed.Bewegen();
        productionBlue.Bewegen();
        productionYellow.Bewegen();
        productionGreen.Bewegen();
    }
}
