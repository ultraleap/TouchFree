using System;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
#if !DEBUG
using System.IO;
using System.Runtime.InteropServices;
#endif

namespace Ultraleap.TouchFree.Service
{
    public class TouchFreeLogger : ITouchFreeLogger
    {
        public TouchFreeLogger(IConfigFileLocator configFileLocator)
        {
#if !DEBUG
            var loggingFileDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                Path.GetFullPath("/storage/sd/ultraleap/touchfree/logs/") :
                Path.Combine(configFileLocator.ConfigFileDirectory, "..\\Logs\\");

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
                    File.Move(filename, filename.Replace("log.txt", "log_old.txt"));
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
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open log file, run as administrator to enable logging.");
            }
#endif

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now} - Starting Service");
            Console.WriteLine();
        }

        public void WriteLine(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine($"{DateTime.Now} - {text}");
            }
            else
            {
                Console.WriteLine();
            }
        }

        public void ErrorWriteLine(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                Console.Error.WriteLine($"{DateTime.Now} - {text}");
            }
            else
            {
                Console.Error.WriteLine();
            }
        }
    }
}
