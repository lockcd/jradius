using JRadius.Core.Handler;
using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using JRadius.Core.Server;

namespace JRadius.Example
{
    public class WPACaptivePortal : PacketHandlerBase
    {
        public override bool Handle(JRadiusRequest request)
        {
            try
            {
                var ci = request.GetConfigItems();
                var req = request.GetRequestPacket();
                var rep = request.GetReplyPacket();

                var username = req.GetAttributeValue(1); // User-Name

                if (rep is AccessAccept)
                {
                    // TODO: Log info
                }
                else
                {
                    // TODO: The attribute classes have not been converted yet.
                    // if ("allow-wpa-guests".Equals(req.GetAttributeValue(Attr_ChilliSpotConfig.TYPE)))
                    // {
                    //     if (req.FindAttribute(Attr_EAPMessage.TYPE) != null)
                    //     {
                    //         if (req.FindAttribute(Attr_FreeRADIUSProxiedTo.TYPE) != null)
                    //         {
                    //             rep = new AccessAccept();
                    //             rep.AddAttribute(new Attr_ChilliSpotConfig("require-uam-auth"));
                    //             request.SetReplyPacket(rep);

                    //             ci.Add(new Attr_AuthType("Accept"));
                    //             request.SetReturnValue(JRadiusServer.RLM_MODULE_UPDATED);
                    //             return true;
                    //         }
                    //     }
                    // }
                }
            }
            catch (System.Exception)
            {
                // TODO: Log exception
            }

            request.SetReturnValue(JRadiusServer.RLM_MODULE_UPDATED);
            return false;
        }
    }
}
