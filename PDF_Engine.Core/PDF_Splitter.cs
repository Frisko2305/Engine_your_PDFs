using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDF_Engine.Core
{
    public class PDF_Splitter
    {
        /// <summary>
        /// Splits a single PDF document into multiple single-page PDF files.
        /// </summary>
        /// <param name="inputPath">The path of the source PDF.</param>
        /// <param name="outputDirectory">The folder where the extracted pages will be saved.</param>
        public void SplitIntoSinglePages(string inputPath, string outputDirectory)
        {
            // 1. Create the destination directory if it doesn't exist
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Extract the original file name to name the split pages cleanly
            string baseFileName = Path.GetFileNameWithoutExtension(inputPath);

            // 2. Open the source document in Import mode for memory efficiency
            using (PdfDocument inputDocument = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import))
            {
                int count = inputDocument.PageCount;

                // 3. Iterate through every page
                for (int idx = 0; idx < count; idx++)
                {
                    // Create a brand new document for this specific page
                    using (PdfDocument outputDocument = new PdfDocument())
                    {
                        // Extract the page from the source and add it to the new document
                        PdfPage page = inputDocument.Pages[idx];
                        outputDocument.AddPage(page);

                        // Construct the output filename (e.g., "resultat_fusion_page_1.pdf")
                        string outputPath = Path.Combine(outputDirectory, $"{baseFileName}_page_{idx + 1}.pdf");
                        // Save the single-page document
                        outputDocument.Save(outputPath);
                    }
                }
            }
        }
    }
}