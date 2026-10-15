using Avalonia.Media;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.ViewModels;

public class LegoBrickViewModel : ViewModelBase
{
    private readonly LegoBrick brick;

    public double X => brick.Location.X;
    public double Y => brick.Location.Y;

    public double Size => brick.Size switch
    {
        Parameters.LegoSize.small => Parameters.brickSmallSize,
        Parameters.LegoSize.medium => Parameters.brickMediumSize,
        _ => Parameters.brickMediumSize * 2
    };

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

    // Nach jeder Bewegung aufrufen, damit die Oberfläche die neue Position zeichnet
    public void Refresh()
    {
        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
    }
}
