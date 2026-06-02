using Avalonia;
using Avalonia.ReactiveUI;
using PDF_Engine.Core;

namespace PDF_Engine.App;

class Program
{
    // Preserving your dedicated test directory path
    static readonly string TestDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "tests"));

    [STAThread]
    public static void Main(string[] args)
    {
        // ROUTING LOGIC: If you pass "--test" in the terminal, run your switchboard
        if (args.Length > 0 && args[0] == "--test")
        {
            RunTestSwitchboard();
        }
        else
        {
            // Otherwise, launch the full graphic Avalonia application
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }

    // Initializing and configuring the Avalonia GUI engine
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

    // =========================================================================
    // PERSISTED TEST SUITE (Your recorded manual tests)
    // =========================================================================
    
    static void RunTestSwitchboard()
    {
        Console.WriteLine("=== PDF_Engine : Sandbox Testing Switchboard ===");
        Console.WriteLine($"Base test directory: {TestDirectory}");
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("1: Test PDF_Merger");
        Console.WriteLine("2: Test PDF_Splitter");
        Console.WriteLine("3: Test PDF_Modifier (Rotation, Deletion & Reordering)");
        Console.Write("Select a test to run (1, 2, or 3): ");
        
        string? choice = Console.ReadLine();
        Console.WriteLine("\n--------------------------------------------------");

        if (choice == "1") RunMergerTest();
        else if (choice == "2") RunSplitterTest();
        else if (choice == "3") RunModifierTest();
        else Console.WriteLine("Invalid choice. Exiting test environment.");
    }

    static void RunMergerTest()
    {
        Console.WriteLine("[Running Merger Test...]");
        string mergerDir = Path.Combine(TestDirectory, "merger_test");
        
        string file1 = Path.Combine(mergerDir, "PDF_sample_page_1.pdf");
        string file2 = Path.Combine(mergerDir, "PDF_sample_page_2.pdf");
        string file3 = Path.Combine(mergerDir, "PDF_sample_page_3.pdf");
        string file4 = Path.Combine(mergerDir, "PDF_sample_page_4.pdf");
        string file5 = Path.Combine(mergerDir, "PDF_sample_page_5.pdf");
        string file6 = Path.Combine(mergerDir, "PDF_sample_page_6.pdf");

        string outputFile = Path.Combine(mergerDir, "Merged_result", "resultat_fusion.pdf");

        if (!File.Exists(file1) || !File.Exists(file2) || !File.Exists(file3) || 
            !File.Exists(file4) || !File.Exists(file5) || !File.Exists(file6))
        {
            if (!Directory.Exists(mergerDir)) { Directory.CreateDirectory(mergerDir); }
            Console.WriteLine($"[Error] Missing input files inside: {mergerDir}");
            return;
        }

        List<string> filesToMerge = new List<string> { file1, file2, file3, file4, file5, file6 };
        PDF_Merger merger = new PDF_Merger();
        
        try
        {
            Console.WriteLine("Merging 6 files...");
            merger.Merge(filesToMerge, outputFile);
            Console.WriteLine($"-> Success! File safely saved at: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"-> [Error] Merge failed: {ex.Message}");
        }
    }

    static void RunSplitterTest()
    {
        Console.WriteLine("[Running Splitter Test...]");
        string splitterDir = Path.Combine(TestDirectory, "splitter_test");
        string fileToSplit = Path.Combine(splitterDir, "test_to_split.pdf"); 
        string outputFolder = Path.Combine(splitterDir, "Split_Results"); 

        if (!File.Exists(fileToSplit))
        {
            if (!Directory.Exists(splitterDir)) { Directory.CreateDirectory(splitterDir); }
            Console.WriteLine($"[Error] Missing input file 'test_to_split.pdf' inside: {splitterDir}");
            return;
        }

        PDF_Splitter splitter = new PDF_Splitter();
        try
        {
            Console.WriteLine("Splitting file...");
            splitter.SplitIntoSinglePages(fileToSplit, outputFolder);
            Console.WriteLine($"-> Success! Check the '{outputFolder}' directory.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"-> [Error] Split failed: {ex.Message}");
        }
    }

    static void RunModifierTest()
    {
        Console.WriteLine("[Running Modifier Test...]");
        string modifierDir = Path.Combine(TestDirectory, "modifier_test");
        string inputFile = Path.Combine(modifierDir, "test_to_modify.pdf");

        string resultDir = Path.Combine(modifierDir, "Modified_result");
        string rotationOutput = Path.Combine(resultDir, "resultat_rotation.pdf");
        string deletionOutput = Path.Combine(resultDir, "resultat_suppression.pdf");
        string reorderOutput = Path.Combine(resultDir, "resultat_reorganisation.pdf");

        if (!File.Exists(inputFile))
        {
            if (!Directory.Exists(modifierDir)) { Directory.CreateDirectory(modifierDir); }
            Console.WriteLine($"[Error] Missing input file 'test_to_modify.pdf' inside: {modifierDir}");
            return;
        }

        PDF_Modifier modifier = new PDF_Modifier();
        try
        {
            Console.WriteLine("Executing Page Rotation (180° on Page 1)...");
            modifier.RotatePage(inputFile, rotationOutput, pageIndex: 0, degrees: 180);

            Console.WriteLine("Executing Page Deletion (Removing Page 2)...");
            modifier.DeletePage(inputFile, deletionOutput, pageIndex: 1);

            Console.WriteLine("Executing Page Reordering (Sequence: [2, 1, 0])...");
            List<int> customSequence = new List<int> { 2, 1, 0 };
            modifier.ReorderPages(inputFile, reorderOutput, customSequence);

            Console.WriteLine($"\n-> All tasks completed successfully! Check the outputs inside: {resultDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"-> [Error] Modification failed: {ex.Message}");
        }
    }
}