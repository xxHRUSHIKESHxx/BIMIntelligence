using System;
using System.IO;
namespace BIMIntelligence.AI;

public static class GroqLogger
{
    private static readonly string LogDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "BIMIntelligence");

    private static readonly string LogFile =
        Path.Combine(
            LogDirectory,
            "groq.log");

    public static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);

            string logLine =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";

            File.AppendAllText(
                LogFile,
                logLine + Environment.NewLine);
        }
        catch
        {
            // Logging must never break the Revit plugin.
        }
    }

    public static string GetLogFilePath()
    {
        return LogFile;
    }
}