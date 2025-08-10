using JRadius.Core.Config;
using JRadius.Core.Handler;
using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using JRadius.Core.Server;
using net.jradius.core.server;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace JRadius.Example
{
    public class LocalUsersHandler : PacketHandlerBase
    {
        private class LocalUser
        {
            public string Username;
            public string Realm;
            public string Password;
            public string Attributes;
            public AttributeList AttrList;

            public string GetUserName()
            {
                if (Realm != null) return $"{Username}@{Realm}";
                return Username;
            }

            public AttributeList GetAttributeList()
            {
                if (AttrList == null)
                {
                    if (Attributes != null)
                    {
                        var reader = new StringReader(Attributes);
                        string line;
                        AttrList = new AttributeList();
                        while ((line = reader.ReadLine()) != null)
                        {
                            // TODO: Implement attribute parsing from string
                        }
                    }
                }
                return AttrList;
            }
        }

        private readonly Dictionary<string, LocalUser> _users = new Dictionary<string, LocalUser>();

        public override void SetConfig(ConfigurationItem cfg)
        {
            base.SetConfig(cfg);
            var root = cfg.GetRoot();
            var usersList = root.Elements("users");
            foreach (var l in usersList)
            {
                var children = l.Elements("user");
                foreach (var node in children)
                {
                    var user = new LocalUser
                    {
                        Username = node.Attribute("username")?.Value,
                        Realm = node.Attribute("realm")?.Value,
                        Password = node.Attribute("password")?.Value,
                        Attributes = node.Value
                    };
                    // TODO: Log the configured user
                    _users[user.GetUserName()] = user;
                }
            }
        }

        public override bool Handle(JRadiusRequest jRequest)
        {
            // TODO: Implement JRadiusRequest
            //try
            //{
            //    var type = jRequest.Type;
            //    var ci = jRequest.GetConfigItems();
            //    var req = jRequest.GetRequestPacket();
            //    var rep = jRequest.GetReplyPacket();

            //    var username = req.GetAttributeValue(1); // User-Name
            //    if (username == null)
            //    {
            //        return false;
            //    }

            //    if (!_users.TryGetValue(username.ToString(), out var u))
            //    {
            //        // Unknown username
            //        return false;
            //    }

            //    switch (type)
            //    {
            //        case JRadiusServer.JRADIUS_AUTHORIZE:
            //            // TODO: Add attributes to the config items
            //            break;
            //        case JRadiusServer.JRADIUS_POST_AUTH:
            //            if (rep is AccessAccept)
            //            {
            //                // TODO: Add attributes to the reply
            //            }
            //            break;
            //    }
            //}
            //catch (System.Exception)
            //{
            //    // TODO: Log exception
            //}

            //jRequest.SetReturnValue(JRadiusServer.RLM_MODULE_UPDATED);
            return false;
        }


    }
}
