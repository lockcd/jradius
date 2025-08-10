using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;

namespace JRadius.Extended.Tls
{
    public class TlsMac
    {
        private long _seqNo;
        private readonly HMac _mac;

        public TlsMac(IDigest digest, byte[] keyBlock, int offset, int len)
        {
            _mac = new HMac(digest);
            var param = new KeyParameter(keyBlock, offset, len);
            _mac.Init(param);
            _seqNo = 0;
        }

        public int GetSize()
        {
            return _mac.GetMacSize();
        }

        public byte[] CalculateMac(short type, byte[] message, int offset, int len)
        {
            var bosMac = new MemoryStream(13);
            TlsUtils.WriteUint64(_seqNo++, bosMac);
            TlsUtils.WriteUint8(type, bosMac);
            TlsUtils.WriteVersion(bosMac);
            TlsUtils.WriteUint16(len, bosMac);

            var macHeader = bosMac.ToArray();
            _mac.BlockUpdate(macHeader, 0, macHeader.Length);
            _mac.BlockUpdate(message, offset, len);

            var result = new byte[_mac.GetMacSize()];
            _mac.DoFinal(result, 0);
            return result;
        }
    }
}
