using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

namespace JRadius.Extended.Tls
{
    public class CombinedHash : IDigest
    {
        private readonly MD5Digest _md5;
        private readonly Sha1Digest _sha1;

        public CombinedHash()
        {
            _md5 = new MD5Digest();
            _sha1 = new Sha1Digest();
        }

        public CombinedHash(CombinedHash t)
        {
            _md5 = new MD5Digest(t._md5);
            _sha1 = new Sha1Digest(t._sha1);
        }

        public string AlgorithmName => _md5.AlgorithmName + " and " + _sha1.AlgorithmName + " for TLS 1.0";

        public int GetDigestSize()
        {
            return 16 + 20;
        }

        public void Update(byte input)
        {
            _md5.Update(input);
            _sha1.Update(input);
        }

        public void BlockUpdate(byte[] input, int inOff, int len)
        {
            _md5.BlockUpdate(input, inOff, len);
            _sha1.BlockUpdate(input, inOff, len);
        }

        public int DoFinal(byte[] output, int outOff)
        {
            int i1 = _md5.DoFinal(output, outOff);
            int i2 = _sha1.DoFinal(output, outOff + 16);
            return i1 + i2;
        }

        public void Reset()
        {
            _md5.Reset();
            _sha1.Reset();
        }

        public int GetByteLength()
        {
            throw new System.NotImplementedException();
        }
    }
}
