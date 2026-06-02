using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;

namespace PDF_Engine.App.Services;

public class PdfService
{
    private PdfDocument? _currentDocument;

    public int? LoadPdf(string filePath)
    {
        try
        {
            if (_currentDocument != null)
            {
                _currentDocument.Dispose();
            }

            _currentDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);
            return _currentDocument.PageCount;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load PDF: {ex.Message}", ex);
        }
    }

    public void MergePdf(string filePath)
    {
        try
        {
            if (_currentDocument == null)
                throw new Exception("No PDF currently loaded");

            using (var newDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import))
            {
                for (int i = 0; i < newDocument.PageCount; i++)
                {
                    var page = newDocument.Pages[i];
                    _currentDocument.AddPage(page);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to merge PDF: {ex.Message}", ex);
        }
    }

    public void SavePdf(string outputPath)
    {
        try
        {
            if (_currentDocument == null)
                throw new Exception("No PDF to save");

            _currentDocument.Save(outputPath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save PDF: {ex.Message}", ex);
        }
    }

    public int GetPageCount() => _currentDocument?.PageCount ?? 0;

    public void Dispose()
    {
        _currentDocument?.Dispose();
    }
}
