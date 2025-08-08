using System;
using net.jradius.core.exception;
using net.jradius.core.log;
using net.jradius.core.server;
using net.jradius.core.server.@event;

namespace net.jradius.core.session
{
    public interface JRadiusSession
    {
        JRadiusLogEntry GetLogEntry(JRadiusEvent @event, string key);

        JRadiusLogEntry GetLogEntry(JRadiusRequest request);

        void AddLogMessage(JRadiusRequest request, string message);

        void CommitLogEntry(JRadiusLogEntry entry, int result);

        void CommitLogEntries(int result);

        void Lock();
        void Unlock();

        void InitSession(JRadiusRequest request);

        void SetAttribute(string name, object value);

        object GetAttribute(string name);

        void OnPostProcessing(JRadiusRequest request);

        void OnAuthorization(JRadiusRequest request);

        bool OnPreProcessing(JRadiusRequest request);

        void OnPostAuthentication(JRadiusRequest request);

        void OnAccounting(JRadiusRequest request);

        bool OnNoAccountingStatusType(JRadiusRequest request);

        void EnsureSessionState(JRadiusRequest request, int state);

        bool IsAccountingReversed();

        string Username { get; set; }
        string Realm { get; set; }
        string Password { get; set; }
        string SessionKey { get; set; }
        string SessionId { get; set; }
        long? ServiceType { get; set; }
        long? IdleTimeout { get; set; }
        long? InterimInterval { get; set; }
        long? SessionTimeout { get; set; }
        long? SessionTime { get; set; }
        DateTime? StartTime { get; set; }
        DateTime? LastInterimTime { get; set; }
        DateTime? StopTime { get; set; }
        long? GigaWordsIn { get; set; }
        long? GigaWordsOut { get; set; }
        long? OctetsIn { get; set; }
        long? OctetsOut { get; set; }
        long? TotalOctetsIn { get; }
        long? TotalOctetsOut { get; }
        long? PacketsIn { get; set; }
        long? PacketsOut { get; set; }
        long? TerminateCause { get; set; }
        byte[][] RadiusClass { get; set; }
        byte[] RadiusState { get; set; }
        int SessionState { get; set; }
        string ProxyToRealm { get; set; }
        bool Secured { get; set; }
        string CalledStationId { get; set; }
        string CallingStationId { get; set; }
        string ConnectInfo { get; set; }
        string ClientIPAddress { get; set; }
        string NasIdentifier { get; set; }
        string NasIPAddress { get; set; }
        string FramedIPAddress { get; set; }
        long TimeStamp { get; set; }
        long? MaxBandwidthDown { get; set; }
        long? MaxBandwidthUp { get; set; }
        long? MinBandwidthDown { get; set; }
        long? MinBandwidthUp { get; set; }
        long? MaxOctetsDown { get; set; }
        long? MaxOctetsUp { get; set; }
        long? MaxOctetsTotal { get; set; }
        string NasType { get; set; }
        string RedirectURL { get; set; }
        string JRadiusKey { get; set; }
        bool IsLogging { get; }
    }
}
