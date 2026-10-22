using Avalonia.Controls;
using Avalonia.Interactivity;
using MyAvaloniaApp.ViewModels;

namespace MyAvaloniaApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel? ViewModel
    {
        get { return DataContext as MainViewModel; }
    }

    private void MoveUp_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveUp(); }
    private void MoveDown_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveDown(); }
    private void MoveLeft_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveLeft(); }
    private void MoveRight_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveRight(); }
    private void MoveUpLeft_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveUpLeft(); }
    private void MoveUpRight_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveUpRight(); }
    private void MoveDownLeft_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveDownLeft(); }
    private void MoveDownRight_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveDownRight(); }

    private void MoveRotateLeft_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveRotateLeft(); }
    private void MoveRotateRight_Click(object? sender, RoutedEventArgs e) { ViewModel?.MoveRotateRight(); }
    private void ResetRotation_Click(object? sender, RoutedEventArgs e) { ViewModel?.ResetRotation(); }

    // Stop-Taste (■) des Roboters
    private void Stop_Click(object? sender, RoutedEventArgs e) { ViewModel?.Stop(); }

    private void UndockButton_Click(object? sender, RoutedEventArgs e) { ViewModel?.Undock(); }

    // Start/Stop der Pendelbewegung der Legosteine
    private void StartTimer_Click(object? sender, RoutedEventArgs e) { ViewModel?.StartTimer(); }
    private void StopTimer_Click(object? sender, RoutedEventArgs e) { ViewModel?.StopTimer(); }
}
