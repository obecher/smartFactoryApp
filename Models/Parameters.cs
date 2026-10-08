namespace MyAvaloniaApp.Models;

internal class Parameters
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

    // "Geschwindigkeit" des Roboters in Pixeln pro Schritt
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
}