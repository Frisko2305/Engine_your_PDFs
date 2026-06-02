using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace PDF_Engine.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Title = "PDF_Engine - Professional PDF Utilities";
        Width = 800;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var stackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 20
        };

        var titleBlock = new TextBlock
        {
            Text = "PDF_Engine",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.White
        };

        var subtitleBlock = new TextBlock
        {
            Text = "Professional PDF Manipulation Tool",
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        stackPanel.Children.Add(titleBlock);
        stackPanel.Children.Add(subtitleBlock);

        Content = stackPanel;
    }
}
