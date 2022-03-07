using System;
using System.Threading.Tasks;
using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Tooling.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using var connectionManager = new ConnectionManager();

            MessageReceiver.TransmitInputAction += TransmitInputAction;
            ConnectionManager.HandFound += ConnectionManager_HandFound;
            ConnectionManager.HandsLost += ConnectionManager_HandsLost;

            while (true)
            {
                await Task.Delay(1);
                ConnectionManager.messageReceiver.CheckQueues();
            }
        }

        private static DateTime LastUpdate;

        private static void TransmitInputAction(ClientInputAction _inputData)
        {
            var currentTime = DateTime.Now;

            var time = currentTime - LastUpdate;

            Console.WriteLine($"{_inputData.InputType.ToString()} - {_inputData.InteractionType.ToString()} - {_inputData.CursorPosition.x} - {_inputData.CursorPosition.y} - {time}");
            LastUpdate = currentTime;
        }

        private static void ConnectionManager_HandFound()
        {
            Console.WriteLine("Hand Found");
        }

        private static void ConnectionManager_HandsLost()
        {
            Console.WriteLine("Hands Lost");
        }
    }
}
