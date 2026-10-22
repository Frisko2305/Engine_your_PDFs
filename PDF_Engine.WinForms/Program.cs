using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PDF_Engine.WinForms;

static class Program
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [STAThread]
    static void Main()
    {
        EnsurePdfiumLoaded();

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }

    private static void EnsurePdfiumLoaded()
    {
        try
        {
            // 1. If pdfium.dll is already next to the .exe, load it directly
            string localDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdfium.dll");
            string localX64Dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64", "pdfium.dll");
            if (File.Exists(localDll)) { LoadLibrary(localDll); return; }
            if (File.Exists(localX64Dll)) { LoadLibrary(localX64Dll); return; }

            // 2. Otherwise, extract the embedded DLL to %LOCALAPPDATA%\PDF_Engine\
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string targetDir = Path.Combine(appData, "PDF_Engine");
            Directory.CreateDirectory(targetDir);
            string targetDll = Path.Combine(targetDir, "pdfium.dll");

            var assembly = Assembly.GetExecutingAssembly();
            string? resourceName = null;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith("pdfium.dll", StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }

            if (resourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    // Write to disk only if missing or if file size differs
                    if (!File.Exists(targetDll) || new FileInfo(targetDll).Length != stream.Length)
                    {
                        using var fileStream = new FileStream(targetDll, FileMode.Create, FileAccess.Write);
                        stream.CopyTo(fileStream);
                    }
                }
            }

            // 3. Pre-load the DLL into the current process address space
            if (File.Exists(targetDll))
            {
                LoadLibrary(targetDll);
            }
        }
        catch
        {
            // Fallback: If anything fails, allow default Windows lookup to proceed
        }
    }
}