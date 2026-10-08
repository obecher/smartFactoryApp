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

    private void MoveUp_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUp();
    }

    private void MoveDown_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDown();
    }

    private void MoveLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveLeft();
    }

    private void MoveRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveRight();
    }

    private void MoveUpLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUpLeft();
    }

    private void MoveUpRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUpRight();
    }

    private void MoveDownLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDownLeft();
    }

    private void MoveDownRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDownRight();
    }

    private void RotateLeft_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RotateLeft();
    }

    private void RotateRight_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RotateRight();
    }
}