namespace Console_gsm_poc.gsm.secrets.Logger;

using System;
using System.IO;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public sealed class FileLogger
{
    private static readonly FileLogger _instance = new FileLogger();
    private readonly string _logFilePath;
    private readonly object _lock = new object();

    private FileLogger()
    {
        string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory); 
        _logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
    }

    public static FileLogger Instance => _instance;

    public void Log(LogLevel level, string message)
    {
        lock (_lock)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, append: true))
                {
                    string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] - {message}";
                    writer.WriteLine(logLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL: Could not write to log file. Error: {ex.Message}");
            }
        }
    }

    public void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    public void Warning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    public void Error(string message, Exception ex = null)
    {
        string errorMessage = ex == null ? message : $"{message}\nException: {ex}";
        Log(LogLevel.Error, errorMessage);
    }
}