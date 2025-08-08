namespace net.jradius.core.server
{
    public static class RadiusConstants
    {
        public const int JRADIUS_AUTHENTICATE = 1;
        public const int JRADIUS_AUTHORIZE = 2;
        public const int JRADIUS_PREACCT = 3;
        public const int JRADIUS_ACCOUNTING = 4;
        public const int JRADIUS_CHECKSIMUL = 5;
        public const int JRADIUS_PRE_PROXY = 6;
        public const int JRADIUS_POST_PROXY = 7;
        public const int JRADIUS_POST_AUTH = 8;
        public const int JRADIUS_MAX_REQUEST_TYPE = 8;

        public const int RLM_MODULE_REJECT = 0;
        public const int RLM_MODULE_FAIL = 1;
        public const int RLM_MODULE_OK = 2;
        public const int RLM_MODULE_HANDLED = 3;
        public const int RLM_MODULE_INVALID = 4;
        public const int RLM_MODULE_USERLOCK = 5;
        public const int RLM_MODULE_NOTFOUND = 6;
        public const int RLM_MODULE_NOOP = 7;
        public const int RLM_MODULE_UPDATED = 8;
        public const int RLM_MODULE_NUMCODES = 9;

        public static string ResultCodeToString(int resultCode)
        {
            switch (resultCode)
            {
                case RLM_MODULE_REJECT: return "REJECT";
                case RLM_MODULE_FAIL: return "FAIL";
                case RLM_MODULE_OK: return "OK";
                case RLM_MODULE_HANDLED: return "HANDLED";
                case RLM_MODULE_INVALID: return "INVALID";
                case RLM_MODULE_USERLOCK: return "USERLOCK";
                case RLM_MODULE_NOTFOUND: return "NOTFOUND";
                case RLM_MODULE_NOOP: return "NOOP";
                case RLM_MODULE_UPDATED: return "UPDATED";
                case RLM_MODULE_NUMCODES: return "NUMCODES";
                default: return "UNKNOWN";
            }
        }
    }
}
