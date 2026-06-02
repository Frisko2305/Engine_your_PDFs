using System;
using System.Collections.Generic;
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
                outputDocument.Save(outputPath);
            }
        }
    }
}