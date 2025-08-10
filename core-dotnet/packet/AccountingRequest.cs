using JRadius.Core.Client;
using JRadius.Core.Packet.Attribute;
using JRadius.Core.Util;
using System.IO;
using System.Linq;

namespace JRadius.Core.Packet
{
    public class AccountingRequest : RadiusRequest
    {
        public const byte CODE = 4;

        public AccountingRequest()
        {
            _code = CODE;
        }

        public AccountingRequest(RadiusClient client)
            : base(client)
        {
            _code = CODE;
        }

        public AccountingRequest(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }

        public AccountingRequest(RadiusClient client, AttributeList attributes)
            : base(client, attributes)
        {
            _code = CODE;
        }

        public const int ACCT_STATUS_START = 1;
        public const int ACCT_STATUS_STOP = 2;
        public const int ACCT_STATUS_INTERIM = 3;
        public const int ACCT_STATUS_ACCOUNTING_ON = 7;
        public const int ACCT_STATUS_ACCOUNTING_OFF = 8;

        public int GetAccountingStatusType()
        {
            var i = (long)GetAttributeValue(40); // Acct-Status-Type
            return (int)i;
        }

        public void SetAccountingStatusType(int type)
        {
            // TODO: Implement attribute creation from factory
            // var a = AttributeFactory.NewAttribute(40, null, IsRecyclable());
            // var s = (NamedValue)a.GetValue();
            // s.SetValue(type);
            // OverwriteAttribute(a);
        }

        public override byte[] CreateAuthenticator(byte[] attributes, int offset, int length, string sharedSecret)
        {
            _authenticator = RadiusUtils.MakeRFC2866RequestAuthenticator(sharedSecret,
                (byte)GetCode(), (byte)GetIdentifier(),
                length + RADIUS_HEADER_LENGTH,
                attributes, offset, length);
            return _authenticator;
        }

        public override bool VerifyAuthenticator(string sharedSecret)
        {
            var buffer = new MemoryStream(4096);
            // TODO: RadiusFormat.GetInstance().PackAttributeList(GetAttributes(), buffer, true);
            var newauth = RadiusUtils.MakeRFC2866RequestAuthenticator(sharedSecret,
                (byte)GetCode(), (byte)GetIdentifier(),
                (int)buffer.Position + RADIUS_HEADER_LENGTH,
                buffer.GetBuffer(), 0, (int)buffer.Position);
            return newauth.SequenceEqual(_authenticator);
        }
    }
}
