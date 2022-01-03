using Arian.Core;
using System;
using System.ServiceModel;

namespace SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof(ScannerService));
            host.Open();

            Console.WriteLine("Host is running.");
            Console.ReadKey();
            host.Close();
        }
    }
}