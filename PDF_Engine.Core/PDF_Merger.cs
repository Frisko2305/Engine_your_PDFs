using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDF_Engine.Core
{
    public class PDF_Merger 
    {
        public void Merge(List<string> inputPaths, string outputPath)
        {
            using (PdfDocument outputDocument = new PdfDocument())
            {
                foreach (string file in inputPaths)
                {
                    using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                    {
                        int count = inputDocument.PageCount;
                        for (int idx = 0; idx < count; idx++)
                        {
                            PdfPage page = inputDocument.Pages[idx];
                            outputDocument.AddPage(page);
                        }
                    }
                }

                // --- NEW AUTOMATED LOGIC ---
                // Extract the directory path from the final output file path
                string? containerDirectory = Path.GetDirectoryName(outputPath);
                
                // If the directory path is valid and does not exist yet, create it automatically
                if (!string.IsNullOrEmpty(containerDirectory) && !Directory.Exists(containerDirectory))
                {
                    Directory.CreateDirectory(containerDirectory);
                }
                // ---------------------------

                outputDocument.Save(outputPath);
            }
        }
    }
}