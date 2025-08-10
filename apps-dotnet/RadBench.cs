using JRadius.Core.Client;
using JRadius.Core.Client.Auth;
using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace JRadius.Apps
{
    public class RadBench
    {
        protected static RadiusClient client;
        protected static IRadiusAuthenticator auth;

        protected static void Usage()
        {
            Console.WriteLine("RadBench Arguments: [options] server secret file");
            Console.WriteLine("\tserver\t\t= RADIUS server hostname or ip");
            Console.WriteLine("\tsecret\t\t= Shared secret to use");
            Console.WriteLine("\tfile\t\t= File containing the attribute name/value pairs");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("\t-d java-class\t= Java class name of the attribute dictionary");
            Console.WriteLine("\t                 (default: net.jradius.dictionary.RadiusDictionaryImpl)");
            Console.WriteLine("\t-a auth-mode\t= Either PAP (default), CHAP, MSCHAP, MSCHAPv2,");
            Console.WriteLine("\t                 EAP-MD5, or EAP-MSCHAPv2");
            Console.WriteLine("\t                 (always provide the plain-text password in User-Password)");
        }

        protected static bool LoadAttributes(AttributeList list, StreamReader reader)
        {
            string line;
            bool allowLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("#")) continue;

                if (string.IsNullOrEmpty(line))
                {
                    if (!allowLine) break;
                    continue;
                }

                if (line.StartsWith("sleep "))
                {
                    allowLine = true;
                    try
                    {
                        int i = int.Parse(line.Substring(6));
                        if (i > 0) Thread.Sleep(i * 1000);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Invalid sleep parameter");
                    }
                    continue;
                }

                allowLine = false;

                try
                {
                    // TODO: Implement attribute parsing from string
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid radius attribute");
                }
            }

            return (line != null);
        }

        public static void Main(string[] args)
        {
            // Manual argument parsing
            var dictClass = "JRadius.Dictionary.AttributeDictionaryImpl";
            var authPort = 1812;
            var acctPort = 1813;
            var timeout = 60;
            var requesters = 5;
            var requests = 10;
            string host = null;
            string secret = null;
            string file = null;

            var arguments = new Queue<string>(args);
            while (arguments.Count > 0)
            {
                var arg = arguments.Dequeue();
                // TODO: Implement argument parsing
            }

            if (host == null || secret == null || file == null)
            {
                Usage();
                return;
            }

            // TODO: The C# version of AttributeFactory is not yet complete.
            // AttributeFactory.LoadAttributeDictionary(dictClass);

            try
            {
                var inet = Dns.GetHostEntry(host);
                // TODO: The RadiusClient class has not been converted yet.
                // client = new RadiusClient(inet.AddressList[0], secret, authPort, acctPort, timeout);

                var threads = new BenchThread[requesters];
                int i = 0;

                Console.WriteLine("Starting Requester Threads...");
                var startTime = DateTime.Now;

                for (i = 0; i < requesters; i++)
                {
                    threads[i] = new BenchThread(requests, file);
                    threads[i].Start();
                }

                int sent = 0;
                int received = 0;

                for (i = 0; i < threads.Length; i++)
                {
                    threads[i].Join();
                    sent += threads[i].GetSent();
                    received += threads[i].GetReceived();
                }

                var endTime = DateTime.Now;
                Console.WriteLine("Completed.");
                Console.WriteLine("Results:");
                Console.WriteLine($"\tRequesters:       {requesters}");
                Console.WriteLine($"\tRequests:         {requests}");
                Console.WriteLine($"\tPackets Sent:     {sent}");
                Console.WriteLine($"\tPackets Received: {received}");
                Console.WriteLine($"\tSeconds:         {(endTime - startTime).TotalSeconds}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private class BenchThread : Thread
        {
            private readonly int _requests;
            private readonly string _file;
            private int _sent = 0;
            private int _received = 0;

            public BenchThread(int requests, string file)
            {
                _requests = requests;
                _file = file;
                IsBackground = true;
            }

            public override void Run()
            {
                try
                {
                    RunRequester();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void RunRequester()
            {
                // TODO: Implement this method
            }

            public int GetReceived()
            {
                return _received;
            }

            public int GetSent()
            {
                return _sent;
            }
        }
    }
}
