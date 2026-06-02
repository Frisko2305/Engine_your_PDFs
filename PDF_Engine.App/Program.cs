using System;
using System.Collections.Generic;
using PDF_Engine.Core; 

namespace PDF_Engine.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== PDF_Engine : Sandbox Testing ===");
            Console.WriteLine("Testing feature: Merge (PDF_Merger)");

            List<string> filesToMerge = new List<string> 
            { 
                "test1.pdf", 
                "test2.pdf" 
            };
            string outputFile = "resultat_fusion.pdf";

            PDF_Merger merger = new PDF_Merger();
            
            try
            {
                Console.WriteLine("Merging in progress...");
                merger.Merge(filesToMerge, outputFile);
                Console.WriteLine($"-> Success! File saved as: {outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> [Error] Merge failed: {ex.Message}");
            }
        }
    }
}