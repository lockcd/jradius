namespace JRadius.Core.Realm
{
    public class JRadiusRealm
    {
        public bool IsLocal { get; set; }
        public int AcctPort { get; set; }
        public int AuthPort { get; set; }
        public string Realm { get; set; }
        public string Server { get; set; }
        public string SharedSecret { get; set; }
        public int Strip { get; set; }
        public string Source { get; set; }
        public int TimeStamp { get; set; }
    }
}
