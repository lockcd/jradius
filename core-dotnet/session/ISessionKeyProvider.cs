using JRadius.Core.Server;

namespace JRadius.Core.Session
{
    public interface ISessionKeyProvider
    {
        object GetClassKey(JRadiusRequest request);
        object GetAppSessionKey(JRadiusRequest request);
        object GetRequestSessionKey(JRadiusRequest request);
    }
}
