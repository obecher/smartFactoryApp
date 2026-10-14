using System.Drawing;

namespace MyAvaloniaApp.Models;

internal class Robot : Beweger
{
    public Robot(Point location, Rectangle borders) : base(location, borders)
    {
    }

    public Robot(Point location) : base(location)
    {
    }

    // laut Diagramm hat Robot eine eigene Bewegen-Methode;
    // aktuell nur Weiterleitung, hier kommt später Roboter-Spezifisches hinein
    public override void Bewegen(Parameters.Direction direction)
    {
        base.Bewegen(direction);
    }
}