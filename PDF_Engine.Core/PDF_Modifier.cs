using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDF_Engine.Core
{
    public class PDF_Modifier
    {
        /// <summary>
        /// Rotates a specific page in a PDF document by a given angle (90, 180, or 270 degrees).
        /// </summary>
        public void RotatePage(string inputPath, string outputPath, int pageIndex, int degrees)
        {
            if (degrees != 90 && degrees != 180 && degrees != 270)
            {
                throw new ArgumentException("Rotation angle must be 90, 180, or 270 degrees.");
            }

            EnsureOutputDirectoryExists(outputPath);

            using (PdfDocument document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify))
            {
                if (pageIndex < 0 || pageIndex >= document.PageCount)
                {
                    throw new IndexOutOfRangeException("The specified page index does not exist.");
                }

                PdfPage page = document.Pages[pageIndex];
                int currentRotation = page.Rotate;
                page.Rotate = (currentRotation + degrees) % 360;

                document.Save(outputPath);
            }
        }

        /// <summary>
        /// Deletes a specific page from a PDF document.
        /// </summary>
        public void DeletePage(string inputPath, string outputPath, int pageIndex)
        {
            EnsureOutputDirectoryExists(outputPath);

            using (PdfDocument document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Modify))
            {
                if (pageIndex < 0 || pageIndex >= document.PageCount)
                {
                    throw new IndexOutOfRangeException("The specified page index does not exist.");
                }

                document.Pages.RemoveAt(pageIndex);
                document.Save(outputPath);
            }
        }

        /// <summary>
        /// Reorders pages based on a custom sequence of indices (e.g., [2, 0, 1] to put page 3 first).
        /// </summary>
        public void ReorderPages(string inputPath, string outputPath, List<int> newPageSequence)
        {
            EnsureOutputDirectoryExists(outputPath);

            using (PdfDocument outputDocument = new PdfDocument())
            using (PdfDocument inputDocument = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import))
            {
                foreach (int index in newPageSequence)
                {
                    if (index < 0 || index >= inputDocument.PageCount)
                    {
                        throw new IndexOutOfRangeException($"Index {index} is out of bounds for this document.");
                    }

                    // Copy the page at the specified index over to our new sequence
                    PdfPage page = inputDocument.Pages[index];
                    outputDocument.AddPage(page);
                }

                outputDocument.Save(outputPath);
            }
        }

        // Shared helper method to ensure autonomous directory creation across all tools
        private void EnsureOutputDirectoryExists(string outputPath)
        {
            string? containerDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(containerDirectory) && !Directory.Exists(containerDirectory))
            {
                Directory.CreateDirectory(containerDirectory);
            }
        }
    }
}