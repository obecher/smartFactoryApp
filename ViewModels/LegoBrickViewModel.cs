using Avalonia.Media;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.ViewModels;

public class LegoBrickViewModel : ViewModelBase
{
    // Die vier Pinsel in der Reihenfolge des Enums LegoColor: yellow, blue, red, green
    private static readonly IBrush[] pinsel =
    {
        new SolidColorBrush(Color.Parse("#FFD700")),
        new SolidColorBrush(Color.Parse("#4169E1")),
        new SolidColorBrush(Color.Parse("#B22222")),
        new SolidColorBrush(Color.Parse("#228B22"))
    };

    private readonly LegoBrick brick;

    public double X => brick.X;
    public double Y => brick.Y;
    public double Angle => brick.Angle;
    public double Width => brick.Width;
    public double Height => brick.Height;

    // Die Farbe wird bei jedem Zugriff aus dem Stein gelesen: ein Ersatzstein
    // bekommt eine neue Zufallsfarbe und muss auch in der Anzeige umspringen
    public IBrush Fill => pinsel[(int)brick.Color];

    public LegoBrickViewModel(LegoBrick brick)
    {
        this.brick = brick;
    }

    // Nach jeder Bewegung aufrufen, damit die Oberfläche die neue Pose zeichnet
    public void Refresh()
    {
        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
        OnPropertyChanged(nameof(Angle));
        OnPropertyChanged(nameof(Fill));
    }
}
