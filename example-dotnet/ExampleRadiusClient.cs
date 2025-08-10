using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using System;
using System.Net;

namespace JRadius.Example
{
    public class ExampleRadiusClient
    {
        public static void Main(string[] args)
        {
            try
            {
                // TODO: The C# version of AttributeFactory is not yet complete.
                // AttributeFactory.LoadAttributeDictionary("JRadius.Dictionary.AttributeDictionaryImpl");

                var host = Dns.GetHostEntry("localhost");
                // TODO: The RadiusClient class has not been converted yet.
                // var rc = new RadiusClient(host.AddressList[0], "test", 1812, 1813, 1000);

                var attrs = new AttributeList();
                // TODO: The attribute classes have not been converted yet.
                // attrs.Add(new Attr_UserName("test"));
                // attrs.Add(new Attr_NASPortType(Attr_NASPortType.Wireless80211));
                // attrs.Add(new Attr_NASPort(1L));

                // var request = new AccessRequest(rc, attrs);
                // request.AddAttribute(new Attr_UserPassword("test"));

                // Console.WriteLine("Sending:\n" + request.ToString());

                // TODO: The MSCHAPv2Authenticator class has not been converted yet.
                // var reply = rc.Authenticate((AccessRequest)request, new MSCHAPv2Authenticator(), 5);

                // Console.WriteLine("Received:\n" + reply.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
