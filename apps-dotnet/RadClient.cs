using JRadius.Core.Client;
using JRadius.Core.Client.Auth;
using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
//using JRadius.Core.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace JRadius.Apps
{
    public class RadClient
    {
        private static RadiusClient client;

        private static void Usage()
        {
            Console.WriteLine("\nRadClient Arguments: [options] server secret file");
            Console.WriteLine("\tserver\t\t= RADIUS server hostname or ip");
            Console.WriteLine("\tsecret\t\t= Shared secret to use");
            Console.WriteLine("\tfile\t\t= File containing the attribute name/value pairs");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("\t-d java-class\t= Java class name of the attribute dictionary");
            Console.WriteLine("\t                 (default: net.jradius.dictionary.RadiusDictionaryImpl)");
            Console.WriteLine("\t-s java-class\t= Java class name of the attribute checker");
            Console.WriteLine("\t                 (e.g net.jradius.standard.WISPrStandard)");
            Console.WriteLine("\t-a auth-mode\t= Either PAP (default), CHAP, MSCHAP, MSCHAPv2,");
            Console.WriteLine("\t                 EAP-MD5, EAP-MSCHAPv2, or EAP-TLS (see below for format)");
            Console.WriteLine("\t                 (provide the plain-text password in User-Password)");
            Console.WriteLine("\t-T tunnel-mode\t= Only EAP-TTLS is currently supported (see below for info)");
            Console.WriteLine("\t-A\t\t= Generate a unique Acct-Session-Id in Accounting Requests");
            Console.WriteLine("\t-C\t\t= Turn OFF Class attribute support");
            Console.WriteLine("\nUsing EAP-TLS and EAP-TTLS:");
            Console.WriteLine("\nMore information at http://jradius.net/\n");
        }

        private static bool LoadAttributes(AttributeList list, StreamReader reader)
        {
            // Same as in RadBench
            return false;
        }

        public static void Main(string[] args)
        {
            // Manual argument parsing
            var dictClass = "JRadius.Dictionary.AttributeDictionaryImpl";
            string check = null;
            IRadiusAuthenticator auth = null;
            var authPort = 1812;
            var acctPort = 1813;
            var timeout = 60;
            bool tunneledRequest = false;
            bool generateSessionId = false;
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
                var active = true;
                var inet = Dns.GetHostEntry(host);
                var reader = new StreamReader(file);
                IRadiusStandard standard = null;

                // TODO: The RadiusClient class has not been converted yet.
                // client = new RadiusClient(inet.AddressList[0], secret, authPort, acctPort, timeout);

                if (check != null)
                {
                    var c = Type.GetType(check);
                    try
                    {
                        standard = (IRadiusStandard)Activator.CreateInstance(c);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                RadiusAttribute classAttr = null;

                while (active)
                {
                    // TODO: Implement the main loop
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
