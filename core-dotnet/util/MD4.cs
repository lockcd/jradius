using System;
using System.Security.Cryptography;

namespace JRadius.Core.Util
{
    public class MD4 : HashAlgorithm
    {
        private uint _a, _b, _c, _d;
        private uint[] _x;
        private byte[] _buffer;
        private int _bufferOffset;

        public MD4()
        {
            _x = new uint[16];
            _buffer = new byte[64];
            Initialize();
        }

        public override void Initialize()
        {
            _a = 0x67452301;
            _b = 0xefcdab89;
            _c = 0x98badcfe;
            _d = 0x10325476;
            _bufferOffset = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            int n = cbSize;
            int i = ibStart;

            while (n > 0)
            {
                int copyLen = Math.Min(n, 64 - _bufferOffset);
                Array.Copy(array, i, _buffer, _bufferOffset, copyLen);
                _bufferOffset += copyLen;
                i += copyLen;
                n -= copyLen;

                if (_bufferOffset == 64)
                {
                    ProcessBlock(_buffer, 0);
                    _bufferOffset = 0;
                }
            }
        }

        protected override byte[] HashFinal()
        {
            long bitCount = (long)_bufferOffset * 8;
            _buffer[_bufferOffset++] = 0x80;

            if (_bufferOffset > 56)
            {
                Array.Clear(_buffer, _bufferOffset, 64 - _bufferOffset);
                ProcessBlock(_buffer, 0);
                Array.Clear(_buffer, 0, 56);
            }
            else
            {
                Array.Clear(_buffer, _bufferOffset, 56 - _bufferOffset);
            }

            _buffer[56] = (byte)bitCount;
            _buffer[57] = (byte)(bitCount >> 8);
            _buffer[58] = (byte)(bitCount >> 16);
            _buffer[59] = (byte)(bitCount >> 24);
            _buffer[60] = (byte)(bitCount >> 32);
            _buffer[61] = (byte)(bitCount >> 40);
            _buffer[62] = (byte)(bitCount >> 48);
            _buffer[63] = (byte)(bitCount >> 56);

            ProcessBlock(_buffer, 0);

            var hash = new byte[16];
            ToBytes(_a, hash, 0);
            ToBytes(_b, hash, 4);
            ToBytes(_c, hash, 8);
            ToBytes(_d, hash, 12);

            return hash;
        }

        private void ProcessBlock(byte[] block, int offset)
        {
            for (int i = 0; i < 16; i++)
            {
                _x[i] = (uint)(block[offset + i * 4] | (block[offset + i * 4 + 1] << 8) | (block[offset + i * 4 + 2] << 16) | (block[offset + i * 4 + 3] << 24));
            }

            uint aa = _a;
            uint bb = _b;
            uint cc = _c;
            uint dd = _d;

            // Round 1
            aa = FF(aa, bb, cc, dd, _x[0], 3);
            dd = FF(dd, aa, bb, cc, _x[1], 7);
            cc = FF(cc, dd, aa, bb, _x[2], 11);
            bb = FF(bb, cc, dd, aa, _x[3], 19);
            aa = FF(aa, bb, cc, dd, _x[4], 3);
            dd = FF(dd, aa, bb, cc, _x[5], 7);
            cc = FF(cc, dd, aa, bb, _x[6], 11);
            bb = FF(bb, cc, dd, aa, _x[7], 19);
            aa = FF(aa, bb, cc, dd, _x[8], 3);
            dd = FF(dd, aa, bb, cc, _x[9], 7);
            cc = FF(cc, dd, aa, bb, _x[10], 11);
            bb = FF(bb, cc, dd, aa, _x[11], 19);
            aa = FF(aa, bb, cc, dd, _x[12], 3);
            dd = FF(dd, aa, bb, cc, _x[13], 7);
            cc = FF(cc, dd, aa, bb, _x[14], 11);
            bb = FF(bb, cc, dd, aa, _x[15], 19);

            // Round 2
            aa = GG(aa, bb, cc, dd, _x[0], 3);
            dd = GG(dd, aa, bb, cc, _x[4], 5);
            cc = GG(cc, dd, aa, bb, _x[8], 9);
            bb = GG(bb, cc, dd, aa, _x[12], 13);
            aa = GG(aa, bb, cc, dd, _x[1], 3);
            dd = GG(dd, aa, bb, cc, _x[5], 5);
            cc = GG(cc, dd, aa, bb, _x[9], 9);
            bb = GG(bb, cc, dd, aa, _x[13], 13);
            aa = GG(aa, bb, cc, dd, _x[2], 3);
            dd = GG(dd, aa, bb, cc, _x[6], 5);
            cc = GG(cc, dd, aa, bb, _x[10], 9);
            bb = GG(bb, cc, dd, aa, _x[14], 13);
            aa = GG(aa, bb, cc, dd, _x[3], 3);
            dd = GG(dd, aa, bb, cc, _x[7], 5);
            cc = GG(cc, dd, aa, bb, _x[11], 9);
            bb = GG(bb, cc, dd, aa, _x[15], 13);

            // Round 3
            aa = HH(aa, bb, cc, dd, _x[0], 3);
            dd = HH(dd, aa, bb, cc, _x[8], 9);
            cc = HH(cc, dd, aa, bb, _x[4], 11);
            bb = HH(bb, cc, dd, aa, _x[12], 15);
            aa = HH(aa, bb, cc, dd, _x[2], 3);
            dd = HH(dd, aa, bb, cc, _x[10], 9);
            cc = HH(cc, dd, aa, bb, _x[6], 11);
            bb = HH(bb, cc, dd, aa, _x[14], 15);
            aa = HH(aa, bb, cc, dd, _x[1], 3);
            dd = HH(dd, aa, bb, cc, _x[9], 9);
            cc = HH(cc, dd, aa, bb, _x[5], 11);
            bb = HH(bb, cc, dd, aa, _x[13], 15);
            aa = HH(aa, bb, cc, dd, _x[3], 3);
            dd = HH(dd, aa, bb, cc, _x[11], 9);
            cc = HH(cc, dd, aa, bb, _x[7], 11);
            bb = HH(bb, cc, dd, aa, _x[15], 15);

            _a += aa;
            _b += bb;
            _c += cc;
            _d += dd;
        }

        private uint FF(uint a, uint b, uint c, uint d, uint x, int s)
        {
            uint t = a + ((b & c) | (~b & d)) + x;
            return (t << s) | (t >> (32 - s));
        }

        private uint GG(uint a, uint b, uint c, uint d, uint x, int s)
        {
            uint t = a + ((b & c) | (b & d) | (c & d)) + x + 0x5a827999;
            return (t << s) | (t >> (32 - s));
        }

        private uint HH(uint a, uint b, uint c, uint d, uint x, int s)
        {
            uint t = a + (b ^ c ^ d) + x + 0x6ed9eba1;
            return (t << s) | (t >> (32 - s));
        }

        private void ToBytes(uint val, byte[] bytes, int offset)
        {
            bytes[offset] = (byte)val;
            bytes[offset + 1] = (byte)(val >> 8);
            bytes[offset + 2] = (byte)(val >> 16);
            bytes[offset + 3] = (byte)(val >> 24);
        }
    }
}
