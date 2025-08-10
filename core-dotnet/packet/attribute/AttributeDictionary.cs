using System;
using System.Collections.Generic;

namespace JRadius.Core.Packet.Attribute
{
    public interface IAttributeDictionary
    {
        // Some commonly used standard RADIUS Attribute types.
        const int USER_NAME = 1;    // User-Name
        const int USER_PASSWORD = 2;    // User-Password
        const int STATE = 24;   // State
        const int CLASS = 25;   // Class
        const int NAS_IDENTIFIER = 32;  // NAS-Identifier
        const int ACCT_STATUS_TYPE = 40;    // Acct-Status-Type
        const int EAP_MESSAGE = 79; // EAP-Message
        const int MESSAGE_AUTHENTICATOR = 80;   // Message-Authenticator
        const int CHARGEABLE_USER_IDENTITY = 89;    // Message-Authenticator

        void LoadVendorCodes(Dictionary<long, Type> map);
        void LoadAttributes(Dictionary<long, Type> map);
        void LoadAttributesNames(Dictionary<string, Type> map);
    }
}
