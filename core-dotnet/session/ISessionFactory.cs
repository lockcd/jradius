using net.jradius.core.log;
using net.jradius.core.server;
using net.jradius.core.session;
using System.Xml.Linq;

namespace JRadius.Core.Session
{
    public interface ISessionFactory
    {
        JRadiusSession GetSession(JRadiusRequest request, object key);
        JRadiusSession NewSession(JRadiusRequest request);
        JRadiusLogEntry NewSessionLogEntry(JRadiusEvent @event, JRadiusSession session, string packetId);
        void SetConfig(XElement root);
        string GetConfigValue(string name);
    }
}
