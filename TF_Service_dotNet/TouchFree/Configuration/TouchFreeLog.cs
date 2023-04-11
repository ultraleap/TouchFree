using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultraleap.TouchFree.Library.Configuration;

public static class TouchFreeLog
{
    // ReSharper disable once UnusedMember.Global
    public static void SetUpLogging()
    {
        var loggingFileDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
            Path.GetFullPath("/storage/sd/ultraleap/touchfree/logs/") :
            Path.Combine(ConfigFileUtils.ConfigFileDirectory, "..\\Logs\\");

        if (loggingFileDirectory != "")
        {
            Directory.CreateDirectory(loggingFileDirectory);
        }

        var filename = loggingFileDirectory + "log.txt";

        if (File.Exists(filename))
        {
            var fileInfo = new FileInfo(filename);
            var fileSize = fileInfo.Length;
            if (fileSize > 100000)
            {
                File.Move(filename, filename.Replace("log.txt", "log_old.txt"), true);
            }
        }

        try
        {
            FileStream filestream = new FileStream(filename, FileMode.Append);

            StreamWriter streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };

            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Cannot open log file, run as administrator to enable logging. Exception message: {e.Message}");
        }

        WriteLine();
        WriteLine();
        WriteLine("Starting Service");
        WriteLine();
    }

    public static void WriteLine(string text = null) => Console.WriteLine(WithTimestamp(text));
        
    public static void ErrorWriteLine(string text) => Console.Error.WriteLine(WithTimestamp(text));

    private static string? WithTimestamp(string text) =>
        !string.IsNullOrEmpty(text)
            ? $"{DateTime.Now.ToString(new CultureInfo("en-GB"))} - {text}"
            : null;
}