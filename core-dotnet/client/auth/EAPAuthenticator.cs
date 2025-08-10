using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using JRadius.Dictionary;
using System;
using System.IO;
using System.Net;

namespace JRadius.Core.Client.Auth
{
    public abstract class EAPAuthenticator : RadiusAuthenticator
    {
        protected bool _peap = false;
        private bool _startWithIdentity = true;
        private byte _eapType;
        protected int _state = 0;

        public const int STATE_CHALLENGE = 0;
        public const int STATE_AUTHENTICATED = 1;
        public const int STATE_REJECTED = 2;
        public const int STATE_SUCCESS = 3;
        public const int STATE_FAILURE = 4;

        public override void ProcessRequest(RadiusPacket p)
        {
            p.RemoveAttribute(2); // User-Password
            var data = _startWithIdentity ? EapResponse(EAP_IDENTITY, 0, GetUsername()) : null;
            var a = new Attr_EAPMessage();
            a.SetValue(data);
            p.OverwriteAttribute(a);
        }

        public override void ProcessChallenge(RadiusPacket request, RadiusPacket challenge)
        {
            base.ProcessChallenge(request, challenge);
            request.SetIdentifier(-1);
            var eapReply = challenge.GetAttributeValue(79); // EAP-Message
            var eapMessage = DoEAP((byte[])eapReply);
            var a = request.FindAttribute(79); // EAP-Message
            if (a != null)
            {
                request.RemoveAttribute(a);
            }
            // TODO: AttributeFactory.addToAttributeList
        }

        public byte GetEAPType()
        {
            return _eapType;
        }

        public void SetEAPType(int eapType)
        {
            _eapType = (byte)eapType;
        }

        public abstract byte[] DoEAPType(byte id, byte[] data);

        public virtual byte[] DoEAPType(byte id, byte[] data, byte[] fullEAPPacket)
        {
            return DoEAPType(id, data);
        }

        protected bool SuedoEAPType(byte[] eap)
        {
            if (_peap)
            {
                if (eap.Length > 4 && (eap[0] == EAP_REQUEST || eap[0] == EAP_RESPONSE) && eap[4] == EAP_TLV)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public byte[] DoEAP(byte[] eapReply)
        {
            if (eapReply != null)
            {
                byte rtype = EAP_REQUEST;
                byte id = 0;
                int dlen = 0;

                using (var ms = new MemoryStream(eapReply))
                using (var br = new BinaryReader(ms))
                {
                    byte codeOrType = br.ReadByte();
                    if (SuedoEAPType(eapReply))
                    {
                        dlen = (int)ms.Length - 1;
                    }
                    else
                    {
                        rtype = codeOrType;
                        id = br.ReadByte();
                        dlen = IPAddress.NetworkToHostOrder(br.ReadInt16()) - EAP_HEADERLEN - 1;
                        codeOrType = br.ReadByte();
                    }

                    if (rtype != EAP_REQUEST)
                    {
                        // TODO: Log error
                        return null;
                    }

                    byte eapcode = codeOrType;
                    byte[] data = null;

                    if (dlen > 0)
                    {
                        data = br.ReadBytes(dlen);
                    }

                    if (_peap && eapcode == EAP_TLV)
                    {
                        return TlvSuccess(id);
                    }

                    if (eapcode == EAP_IDENTITY)
                    {
                        return EapResponse(EAP_IDENTITY, id, GetUsername());
                    }

                    if (eapcode != _eapType)
                    {
                        return NegotiateEAPType(id, _eapType);
                    }

                    return EapResponse(_eapType, id, DoEAPType(id, data, eapReply));
                }
            }
            return null;
        }

        protected byte[] NegotiateEAPType(byte id, byte eapType)
        {
            return EapResponse(EAP_NAK, id, new byte[] { eapType });
        }

        protected byte[] EapResponse(int type, byte id, byte[] data)
        {
            int offset, length;
            byte[] response;

            if (!_peap || type == EAP_TLV)
            {
                length = 1 + EAP_HEADERLEN + (data?.Length ?? 0);
                response = new byte[length];
                response[0] = EAP_RESPONSE;
                response[1] = id;
                response[2] = (byte)(length >> 8 & 0xFF);
                response[3] = (byte)(length & 0xFF);
                offset = 4;
            }
            else
            {
                length = 1 + (data?.Length ?? 0);
                response = new byte[length];
                offset = 0;
            }
            response[offset] = (byte)(type & 0xFF);
            if (data != null)
            {
                Array.Copy(data, 0, response, offset + 1, data.Length);
            }
            return response;
        }

        public byte[] TlvSuccess(byte id)
        {
            byte[] b = { 0x80, 0x03, 0x00, 0x02, 0x00, 0x01 };
            return EapResponse(EAP_TLV, id, b);
        }

        public const int EAP_HEADERLEN = 4;
        public const int EAP_REQUEST = 1;
        public const int EAP_RESPONSE = 2;
        public const int EAP_SUCCESS = 3;
        public const int EAP_FAILURE = 4;
        public const int EAP_IDENTITY = 1;
        public const int EAP_NOTIFICATION = 2;
        public const int EAP_NAK = 3;
        public const int EAP_MD5 = 4;
        public const int EAP_OTP = 5;
        public const int EAP_GTC = 6;
        public const int EAP_TLS = 13;
        public const int EAP_LEAP = 17;
        public const int EAP_SIM = 18;
        public const int EAP_TTLS = 21;
        public const int EAP_AKA = 23;
        public const int EAP_PEAP = 25;
        public const int EAP_MSCHAPV2 = 26;
        public const int EAP_CISCO_MSCHAPV2 = 29;
        public const int EAP_TLV = 33;
    }
}
