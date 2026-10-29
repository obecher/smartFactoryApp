namespace MyAvaloniaApp.Models;

public class Parameters
{
    public enum Direction
    {
        stop = 0,
        up = 1,
        down = 2,
        left = 3,
        right = 4,
        upLeft = 5,
        upRight = 6,
        downLeft = 7,
        downRight = 8
    }

    public enum Rotation
    {
        left = 0,
        right = 1
    }

    public enum LegoColor
    {
        yellow = 0,
        blue = 1,
        red = 2,
        green = 3
    }

    public enum LegoSize
    {
        large = 0,
        medium = 1,
        small = 2
    }

    // Wo befindet sich ein Stein gerade? Nur Steine auf einer Produktions- oder Recyclinglinie
    // können gegriffen werden, niemals Steine auf der Lieferlinie.
    public enum BrickStatus
    {
        OnProductionLine = 0,   // liegt auf seiner Produktionslinie (greifbar)
        OnRecycleLine = 1,      // liegt auf einem Platz der Recyclinglinie (greifbar)
        Docked = 2,             // wird vom Roboter getragen
        OnDeliveryLine = 3,     // gerade auf der Lieferlinie losgelassen (nicht greifbar)
        ToCustomer = 4,         // fährt auf der Lieferlinie nach rechts zu den Kunden
        ToRecycle = 5           // fährt auf der Lieferlinie nach links zum Recycling
    }

    // Ergebnis des Undock-Buttons
    public enum UndockResult
    {
        NoBrick = 0,            // der Roboter trägt nichts
        NotOnDeliveryLine = 1,  // Undock ist nur auf der Lieferlinie erlaubt
        ToCustomer = 2,         // erfolgreich, Stein ist in Ordnung und geht zum Kunden
        ToRecycle = 3           // erfolgreich, Stein ist defekt und geht zum Recycling
    }

    // ---- Bewegung des Roboters ----
    // Dauerbewegung (Move): Pixel bzw. Grad pro Timer-Tick
    public const double step = 3;
    public const double rotationStep = 3;
    // Einzelschritt (Step): Pixel bzw. Grad pro Klick
    public const double stepMove = 10;
    public const double stepRotation = 15;
    // true: "oben" heißt "in Blickrichtung des Roboters", false: "oben" heißt "nach oben auf dem Plan"
    public const bool moveRelativeToRobot = true;

    // Größe des Roboters in Pixeln (robot_platform.png wird darauf skaliert)
    public const int robotWidth = 30;
    public const int robotHeight = 63;

    // Grauer Bereich in factory_plan.png (x/y = 38..563)
    public const int arenaLeft = 38;
    public const int arenaTop = 38;
    public const int arenaSize = 525;

    // ---- Legosteine ----
    public const int bricksPerLine = 4;       // Steine pro Produktionslinie
    public const int brickSpacing = 40;       // Abstand der Steinmitten entlang der Linie
    public const int brickSmallWidth = 14;
    public const int brickSmallHeight = 9;
    public const int brickMediumWidth = 20;
    public const int brickMediumHeight = 12;
    public const int brickLargeWidth = 28;
    public const int brickLargeHeight = 16;

    // Docking: Abstand zwischen Greifpunkt (TCP) und Stein in Pixeln
    public const double dockDistance = 10;

    // Undock: Der Greifpunkt darf höchstens so weit neben der Lieferlinie liegen (Pixel).
    // Der Roboter kann nicht in die Linie hineinfahren, deshalb braucht es eine Toleranz.
    public const double lineTolerance = 12;

    // Lieferung: Pixel pro Tick, mit denen ein Stein auf der Lieferlinie fährt
    public const double deliveryStep = 3;

    // Pendelbewegung der Steine auf ihrer Linie: Ticks pro Richtung
    public const int stepsPerDirection = 6;

    // Timer-Intervalle in Millisekunden
    public const int timerInterval = 50;        // Legosteine
    public const int robotTimerInterval = 30;   // Dauerbewegung des Roboters
    public const int deliveryTimerInterval = 30; // Steine auf der Lieferlinie
}
