using Avalonia.Media;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.ViewModels;

public class LegoBrickViewModel : ViewModelBase
{
    private readonly LegoBrick brick;

    public double X => brick.X;
    public double Y => brick.Y;
    public double Angle => brick.Angle;
    public double Width => brick.Width;
    public double Height => brick.Height;

    public IBrush Fill { get; }

    public LegoBrickViewModel(LegoBrick brick)
    {
        this.brick = brick;
        Fill = new SolidColorBrush(Color.Parse(brick.Color switch
        {
            Parameters.LegoColor.red => "#B22222",
            Parameters.LegoColor.blue => "#4169E1",
            Parameters.LegoColor.yellow => "#FFD700",
            _ => "#228B22"
        }));
    }

    // Nach jeder Bewegung aufrufen, damit die Oberfläche die neue Pose zeichnet
    public void Refresh()
    {
        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
        OnPropertyChanged(nameof(Angle));
    }
}
