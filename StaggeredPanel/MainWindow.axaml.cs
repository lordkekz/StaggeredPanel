using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace StaggeredPanel;

public partial class MainWindow : Window {
    private Orientation _orient = Orientation.Horizontal;
    public Controls Rectangles => MyPanel.Children;

    public List<Orientation> Orientations { get; } = new() { Orientation.Horizontal, Orientation.Vertical };

    public MainWindow() {
        InitializeComponent();
        DataContext = this;
    }

    private void AddButton_OnClick(object? sender, RoutedEventArgs e) {
        var b = new Border {
            Background = RandomColor(),
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8)
        };
        if (_orient == Orientation.Horizontal) {
            b.MinHeight = 0;
            b.MinWidth = Random.Shared.Next(100, 500);
        }
        else if (_orient == Orientation.Vertical) {
            b.MinWidth = 0;
            b.MinHeight = Random.Shared.Next(100, 500);
        }

        Rectangles.Add(b);
    }

    private static IBrush RandomColor() {
        var _colorsList = new IBrush[] {
            Brushes.Aqua, Brushes.Azure, Brushes.Blue, Brushes.Chocolate, Brushes.Cyan, Brushes.Fuchsia, Brushes.Gold,
            Brushes.Green, Brushes.Lavender, Brushes.Olive, Brushes.Pink, Brushes.Red, Brushes.DarkGreen
        };
        return _colorsList[Random.Shared.Next(_colorsList.Length)];
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e) {
        if (Rectangles.Count > 0)
            Rectangles.RemoveAt(0);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        _orient = (Orientation)e.AddedItems[0];
        MyPanel.Orientation = _orient;
        foreach (var rectangle in Rectangles) {
            var r2 = (rectangle as Border)!;
            if (_orient == Orientation.Horizontal) {
                r2.MinHeight = 0;
                r2.MinWidth = Random.Shared.Next(100, 500);
            }
            else if (_orient == Orientation.Vertical) {
                r2.MinWidth = 0;
                r2.MinHeight = Random.Shared.Next(100, 500);
            }
        }
    }
}