using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Platform.Storage;
using PDF_Engine.App.Services;
using PDF_Engine.App.ViewModels;
using System.Collections.ObjectModel;
using System;
using System.IO;

namespace PDF_Engine.App;

public partial class MainWindow : Window
{
    private AppViewModel _viewModel;
    private PdfService _pdfService;
    private Control? _initialView;
    private Control? _editorView;
    private ScrollViewer? _scrollViewer;
    private StackPanel? _pagesPanel;

    public MainWindow()
    {
        _viewModel = new AppViewModel();
        _pdfService = new PdfService();
        DataContext = _viewModel;

        Title = "PDF_Engine - Professional PDF Utilities";
        Width = 1200;
        Height = 800;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _initialView = CreateInitialView();
        _editorView = CreateEditorView();

        Content = _initialView;
    }

    private Control CreateMainLayout()
    {
        var mainPanel = new Grid();
        mainPanel.Children.Add(CreateInitialView());
        mainPanel.Children.Add(CreateEditorView());

        return mainPanel;
    }

    private Control CreateInitialView()
    {
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 20,
            Name = "InitialView"
        };

        var titleBlock = new TextBlock
        {
            Text = "PDF_Engine",
            FontSize = 48,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.White
        };

        var subtitleBlock = new TextBlock
        {
            Text = "Professional PDF Manipulation Tool",
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 10, 0, 30)
        };

        var addButton = new Button
        {
            Content = "📁 Add PDF",
            Padding = new Avalonia.Thickness(30, 15),
            FontSize = 18,
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = Brushes.DodgerBlue
        };
        addButton.Click += (s, e) => LoadPdfFile();

        panel.Children.Add(titleBlock);
        panel.Children.Add(subtitleBlock);
        panel.Children.Add(addButton);

        return panel;
    }

    private Control CreateEditorView()
    {
        var mainPanel = new DockPanel();

        // Top toolbar - wrap in border for padding
        var toolbarBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
            Padding = new Avalonia.Thickness(10)
        };

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var addPdfButton = new Button
        {
            Content = "➕ Add Another PDF",
            Padding = new Avalonia.Thickness(10, 8),
            FontSize = 12
        };
        addPdfButton.Click += async (s, e) =>
        {
            await MergePdfFile();
        };

        var addBlankPageButton = new Button
        {
            Content = "📄 Add Blank Page",
            Padding = new Avalonia.Thickness(10, 8),
            FontSize = 12
        };
        addBlankPageButton.Click += (s, e) =>
        {
            _viewModel.AddBlankPage();
            if (_pagesPanel != null)
            {
                UpdatePagesUI(_pagesPanel);
            }
        };

        var saveButton = new Button
        {
            Content = "💾 Save & Export",
            Padding = new Avalonia.Thickness(10, 8),
            FontSize = 12,
            Background = Brushes.Green
        };

        toolbar.Children.Add(addPdfButton);
        toolbar.Children.Add(addBlankPageButton);
        toolbar.Children.Add(new Separator { Background = Brushes.Gray });
        toolbar.Children.Add(saveButton);

        toolbarBorder.Child = toolbar;
        DockPanel.SetDock(toolbarBorder, Dock.Top);
        mainPanel.Children.Add(toolbarBorder);

        // Pages area
        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Avalonia.Thickness(10)
        };

        _pagesPanel = new StackPanel
        {
            Spacing = 15,
            Orientation = Orientation.Vertical
        };

        // Bind pages to UI
        _viewModel.Pages.CollectionChanged += (s, e) => UpdatePagesUI(_pagesPanel);
        UpdatePagesUI(_pagesPanel);

        _scrollViewer.Content = _pagesPanel;
        mainPanel.Children.Add(_scrollViewer);

        return mainPanel;
    }

    private void UpdatePagesUI(StackPanel pagesPanel)
    {
        pagesPanel.Children.Clear();

        foreach (var page in _viewModel.Pages)
        {
            var pageContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(15),
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };

            var pagePanel = new StackPanel { Spacing = 10 };

            // Page number and preview placeholder
            var pageHeader = new TextBlock
            {
                Text = $"Page {page.PageIndex + 1}",
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White
            };

            var previewBox = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Height = 200,
                CornerRadius = new Avalonia.CornerRadius(4),
                RenderTransform = new RotateTransform(page.Rotation)
            };

            var previewText = new TextBlock
            {
                Text = $"Page {page.PageIndex + 1}\n(Rotation: {page.Rotation}°)",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center
            };
            previewBox.Child = previewText;

            // Control buttons
            var controlsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };

            var moveUpBtn = new Button { Content = "⬆️ Up", Padding = new Avalonia.Thickness(6, 6), FontSize = 10, Tag = page.PageIndex, IsEnabled = page.PageIndex > 0 };
            var moveDownBtn = new Button { Content = "⬇️ Down", Padding = new Avalonia.Thickness(6, 6), FontSize = 10, Tag = page.PageIndex, IsEnabled = page.PageIndex < _viewModel.Pages.Count - 1 };
            var rotateLeftBtn = new Button { Content = "⟲ Rotate Left", Padding = new Avalonia.Thickness(8, 6), FontSize = 11, Tag = page.PageIndex };
            var rotateRightBtn = new Button { Content = "⟳ Rotate Right", Padding = new Avalonia.Thickness(8, 6), FontSize = 11, Tag = page.PageIndex };
            var deleteBtn = new Button { Content = "🗑️ Delete", Padding = new Avalonia.Thickness(8, 6), FontSize = 11, Background = Brushes.IndianRed, Tag = page.PageIndex };

            moveUpBtn.Click += (s, e) =>
            {
                var pageIdx = (int)((Button)s!).Tag;
                _viewModel.MovePageUp(pageIdx);
                if (_pagesPanel != null)
                {
                    UpdatePagesUI(_pagesPanel);
                }
            };

            moveDownBtn.Click += (s, e) =>
            {
                var pageIdx = (int)((Button)s!).Tag;
                _viewModel.MovePageDown(pageIdx);
                if (_pagesPanel != null)
                {
                    UpdatePagesUI(_pagesPanel);
                }
            };

            rotateLeftBtn.Click += (s, e) =>
            {
                var pageIdx = (int)((Button)s!).Tag;
                _viewModel.RotatePageLeft(pageIdx);
                if (_pagesPanel != null)
                {
                    UpdatePagesUI(_pagesPanel);
                }
            };

            rotateRightBtn.Click += (s, e) =>
            {
                var pageIdx = (int)((Button)s!).Tag;
                _viewModel.RotatePageRight(pageIdx);
                if (_pagesPanel != null)
                {
                    UpdatePagesUI(_pagesPanel);
                }
            };

            deleteBtn.Click += (s, e) =>
            {
                var pageIdx = (int)((Button)s!).Tag;
                _viewModel.DeletePage(pageIdx);
                if (_pagesPanel != null)
                {
                    UpdatePagesUI(_pagesPanel);
                }
            };

            controlsPanel.Children.Add(moveUpBtn);
            controlsPanel.Children.Add(moveDownBtn);
            controlsPanel.Children.Add(rotateLeftBtn);
            controlsPanel.Children.Add(rotateRightBtn);
            controlsPanel.Children.Add(deleteBtn);

            pagePanel.Children.Add(pageHeader);
            pagePanel.Children.Add(previewBox);
            pagePanel.Children.Add(controlsPanel);

            pageContainer.Child = pagePanel;
            pagesPanel.Children.Add(pageContainer);

            // Split button after each page (except the last)
            if (page.PageIndex < _viewModel.Pages.Count - 1)
            {
                var splitBtn = new Button
                {
                    Content = "✂️ Split Here",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Avalonia.Thickness(15, 8),
                    Background = Brushes.Orange,
                    Margin = new Avalonia.Thickness(0, -5, 0, 5)
                };
                pagesPanel.Children.Add(splitBtn);
            }
        }
    }

    private async void LoadPdfFile()
    {
        try
        {
            // Create a simple dialog to get file path
            var inputPanel = new StackPanel
            {
                Spacing = 10,
                Margin = new Avalonia.Thickness(20)
            };

            var label = new TextBlock { Text = "Enter PDF file path:", Margin = new Avalonia.Thickness(0, 0, 0, 5) };
            var textBox = new TextBox { PlaceholderText = "C:\\path\\to\\file.pdf" };
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Avalonia.Thickness(0, 10, 0, 0) };
            var okBtn = new Button { Content = "Open", Padding = new Avalonia.Thickness(20, 8) };
            var cancelBtn = new Button { Content = "Cancel", Padding = new Avalonia.Thickness(20, 8) };

            buttonPanel.Children.Add(okBtn);
            buttonPanel.Children.Add(cancelBtn);

            inputPanel.Children.Add(label);
            inputPanel.Children.Add(textBox);
            inputPanel.Children.Add(buttonPanel);

            var dialog = new Window
            {
                Title = "Open PDF",
                Width = 400,
                Height = 150,
                Content = inputPanel
            };

            var filePath = "";

            okBtn.Click += (s, e) =>
            {
                filePath = textBox.Text ?? "";
                dialog.Close();
            };

            cancelBtn.Click += (s, e) => dialog.Close();

            await dialog.ShowDialog(this);

            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                var pageCount = _pdfService.LoadPdf(filePath);

                if (pageCount.HasValue && pageCount > 0)
                {
                    _viewModel.CurrentPdfPath = filePath;
                    _viewModel.LoadPdfPages(pageCount.Value);

                    if (_editorView != null)
                    {
                        Content = _editorView;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
                await ShowErrorDialog($"File not found: {filePath}");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Error loading PDF: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task ShowErrorDialog(string message)
    {
        var errorPanel = new StackPanel
        {
            Spacing = 15,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(20)
        };

        var textBlock = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
        var okButton = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Right };

        errorPanel.Children.Add(textBlock);
        errorPanel.Children.Add(okButton);

        var dialog = new Window
        {
            Title = "Error",
            Width = 400,
            Height = 150,
            Content = errorPanel
        };

        okButton.Click += (s, e) => dialog.Close();

        await dialog.ShowDialog(this);
    }

    private async System.Threading.Tasks.Task MergePdfFile()
    {
        try
        {
            var inputPanel = new StackPanel
            {
                Spacing = 10,
                Margin = new Avalonia.Thickness(20)
            };

            var label = new TextBlock { Text = "Enter PDF file path to merge:", Margin = new Avalonia.Thickness(0, 0, 0, 5) };
            var textBox = new TextBox { PlaceholderText = "C:\\path\\to\\file.pdf" };
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Avalonia.Thickness(0, 10, 0, 0) };
            var okBtn = new Button { Content = "Merge", Padding = new Avalonia.Thickness(20, 8) };
            var cancelBtn = new Button { Content = "Cancel", Padding = new Avalonia.Thickness(20, 8) };

            buttonPanel.Children.Add(okBtn);
            buttonPanel.Children.Add(cancelBtn);

            inputPanel.Children.Add(label);
            inputPanel.Children.Add(textBox);
            inputPanel.Children.Add(buttonPanel);

            var dialog = new Window
            {
                Title = "Merge PDF",
                Width = 400,
                Height = 150,
                Content = inputPanel
            };

            var filePath = "";

            okBtn.Click += (s, e) =>
            {
                filePath = textBox.Text ?? "";
                dialog.Close();
            };

            cancelBtn.Click += (s, e) => dialog.Close();

            await dialog.ShowDialog(this);

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                _pdfService.MergePdf(filePath);
                var pageCount = _pdfService.GetPageCount();
                _viewModel.LoadPdfPages(pageCount);

                if (_pagesPanel != null)
                {
                    UpdatePagesUI(_pagesPanel);
                }

                await ShowErrorDialog($"Successfully merged! Total pages: {pageCount}");
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
                await ShowErrorDialog($"File not found: {filePath}");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Error merging PDF: {ex.Message}");
        }
    }
}
