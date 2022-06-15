using System;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
#if !DEBUG
using System.IO;
using System.Runtime.InteropServices;
#endif

namespace Ultraleap.TouchFree.Service_Android
{
    public class TouchFreeLogger : ITouchFreeLogger
    {
        public TouchFreeLogger()
        {

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
