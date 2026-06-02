using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace PDF_Engine.App.ViewModels;

public class PdfPageViewModel : ReactiveObject
{
    private int _pageIndex;
    private int _rotation;

    public int PageIndex
    {
        get => _pageIndex;
        set => this.RaiseAndSetIfChanged(ref _pageIndex, value);
    }

    public int Rotation
    {
        get => _rotation;
        set => this.RaiseAndSetIfChanged(ref _rotation, value);
    }

    public PdfPageViewModel(int pageIndex)
    {
        _pageIndex = pageIndex;
        _rotation = 0;
    }
}

public class AppViewModel : ReactiveObject
{
    private string? _currentPdfPath;
    private bool _isPdfLoaded;
    private ObservableCollection<PdfPageViewModel> _pages = new();

    public string? CurrentPdfPath
    {
        get => _currentPdfPath;
        set => this.RaiseAndSetIfChanged(ref _currentPdfPath, value);
    }

    public bool IsPdfLoaded
    {
        get => _isPdfLoaded;
        set => this.RaiseAndSetIfChanged(ref _isPdfLoaded, value);
    }

    public ObservableCollection<PdfPageViewModel> Pages
    {
        get => _pages;
        set => this.RaiseAndSetIfChanged(ref _pages, value);
    }

    public AppViewModel()
    {
        IsPdfLoaded = false;
    }

    public void LoadPdfPages(int pageCount)
    {
        Pages.Clear();
        for (int i = 0; i < pageCount; i++)
        {
            Pages.Add(new PdfPageViewModel(i));
        }
        IsPdfLoaded = true;
    }

    public void RotatePageLeft(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < Pages.Count)
        {
            var page = Pages[pageIndex];
            page.Rotation = (page.Rotation - 90 + 360) % 360;
        }
    }

    public void RotatePageRight(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < Pages.Count)
        {
            var page = Pages[pageIndex];
            page.Rotation = (page.Rotation + 90) % 360;
        }
    }

    public void DeletePage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < Pages.Count)
        {
            Pages.RemoveAt(pageIndex);

            // Update page indices for remaining pages
            for (int i = pageIndex; i < Pages.Count; i++)
            {
                Pages[i].PageIndex = i;
            }
        }
    }

    public void AddBlankPage()
    {
        var newPage = new PdfPageViewModel(Pages.Count);
        Pages.Add(newPage);
    }

    public void MovePageUp(int pageIndex)
    {
        if (pageIndex > 0 && pageIndex < Pages.Count)
        {
            var temp = Pages[pageIndex];
            Pages[pageIndex] = Pages[pageIndex - 1];
            Pages[pageIndex - 1] = temp;

            // Update page indices
            Pages[pageIndex].PageIndex = pageIndex;
            Pages[pageIndex - 1].PageIndex = pageIndex - 1;
        }
    }

    public void MovePageDown(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < Pages.Count - 1)
        {
            var temp = Pages[pageIndex];
            Pages[pageIndex] = Pages[pageIndex + 1];
            Pages[pageIndex + 1] = temp;

            // Update page indices
            Pages[pageIndex].PageIndex = pageIndex;
            Pages[pageIndex + 1].PageIndex = pageIndex + 1;
        }
    }
}
