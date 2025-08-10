using JRadius.Core.Util;
using System;

namespace JRadius.Core.Client.Auth
{
    public class EAPMSCHAPv2Authenticator : EAPAuthenticator
    {
        public const string NAME = "eap-mschapv2";

        public EAPMSCHAPv2Authenticator()
        {
            SetEAPType(EAP_MSCHAPV2);
        }

        public EAPMSCHAPv2Authenticator(bool peap)
        {
            SetEAPType(EAP_MSCHAPV2);
            _peap = peap;
        }

        public override string GetAuthName()
        {
            return NAME;
        }

        public override byte[] DoEAPType(byte id, byte[] data)
        {
            byte opCode = data[0];
            switch (opCode)
            {
                case EAP_MSCHAPV2_CHALLENGE:
                    {
                        var challenge = new byte[16];
                        Array.Copy(data, 5, challenge, 0, 16);

                        int length = 54 + GetUsername().Length;
                        var response = new byte[length];
                        response[0] = EAP_MSCHAPV2_RESPONSE;
                        response[1] = data[1];
                        response[2] = (byte)(length >> 8 & 0xFF);
                        response[3] = (byte)(length & 0xFF);
                        response[4] = 49;
                        Array.Copy(MSCHAP.DoMSCHAPv2(GetUsername(), GetPassword(), challenge), 2, response, 5, 48);
                        response[53] = 0;
                        Array.Copy(GetUsername(), 0, response, 54, GetUsername().Length);
                        return response;
                    }
                case EAP_MSCHAPV2_SUCCESS:
                    {
                        _state = STATE_AUTHENTICATED;
                        var response = new byte[1];
                        response[0] = EAP_MSCHAPV2_SUCCESS;
                        return response;
                    }
                default:
                    {
                        _state = STATE_FAILURE;
                        var response = new byte[1];
                        response[0] = EAP_MSCHAPV2_FAILURE;
                        return response;
                    }
            }
        }

        protected const byte EAP_MSCHAPV2_ACK = 0;
        protected const byte EAP_MSCHAPV2_CHALLENGE = 1;
        protected const byte EAP_MSCHAPV2_RESPONSE = 2;
        protected const byte EAP_MSCHAPV2_SUCCESS = 3;
        protected const byte EAP_MSCHAPV2_FAILURE = 4;
    }
}
