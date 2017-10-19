using System;

namespace FClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            AsynchronousClient.StartClient();
            Console.Read();
        }
    }
}
