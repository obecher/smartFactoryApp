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

    // "Geschwindigkeit" in Pixeln pro Schritt (Roboter und Legosteine)
    public const int step = 3;

    // Drehwinkel pro Klick in Grad
    public const int rotationStep = 5;

    // Anzeigegröße des Roboters (robot_platform.png ist 790x502, hier verkleinert)
    public const int robotWidth = 30;
    public const int robotHeight = 19;

    // Grauer Bereich in factory_plan.png (x/y = 38..562, also 525 x 525 Pixel)
    public const int arenaLeft = 38;
    public const int arenaTop = 38;
    public const int arenaSize = 525;

    // Legosteine
    public const int bricksPerLine = 4;       // Steine pro Produktionslinie
    public const int brickSmallSize = 12;     // Kantenlänge kleiner Stein in Pixeln
    public const int brickMediumSize = 18;    // Kantenlänge mittlerer Stein in Pixeln
    public const int brickSpacing = 24;       // Abstand zwischen den Steinen einer Linie
    public const int lineOffset = 170;        // Abstand der Linien vom Mittelpunkt des Plans

    // Timer-Intervall in Millisekunden
    public const int timerInterval = 50;
}
