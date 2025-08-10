namespace JRadius.Core.Util
{
    public sealed partial class MSCHAP_Old
    {
        #region MD4 Implementation
        /// <summary>
        /// A self-contained implementation of the MD4 hashing algorithm.
        /// .NET does not provide a built-in MD4 class.
        /// </summary>
        private sealed class MD4 : IDisposable
        {
            private readonly uint[] _state = { 0x67452301, 0xefcdab89, 0x98badcfe, 0x10325476 };
            private readonly uint[] _count = new uint[2];
            private readonly byte[] _buffer = new byte[64];
            private readonly uint[] _x = new uint[16];

            public byte[] ComputeHash(byte[] data)
            {
                Update(data, 0, data.Length);
                return Digest();
            }

            private void Update(byte[] input, int index, int length)
            {
                int bufferIndex = (int)((_count[0] >> 3) & 0x3F);
                _count[0] += (uint)length << 3;
                if (_count[0] < ((uint)length << 3)) _count[1]++;
                _count[1] += (uint)length >> 29;

                int partLen = 64 - bufferIndex;
                int i;
                if (length >= partLen)
                {
                    Buffer.BlockCopy(input, index, _buffer, bufferIndex, partLen);
                    Transform(_buffer, 0);
                    for (i = partLen; i + 63 < length; i += 64)
                    {
                        Transform(input, index + i);
                    }
                    bufferIndex = 0;
                }
                else
                {
                    i = 0;
                }
                Buffer.BlockCopy(input, index + i, _buffer, bufferIndex, length - i);
            }

            private byte[] Digest()
            {
                byte[] bits = new byte[8];
                Encode(bits, _count, 8);
                int index = (int)((_count[0] >> 3) & 0x3f);
                int padLen = (index < 56) ? (56 - index) : (120 - index);
                byte[] padding = new byte[padLen];
                padding[0] = 0x80;
                Update(padding, 0, padLen);
                Update(bits, 0, 8);
                byte[] result = new byte[16];
                Encode(result, _state, 16);
                return result;
            }

            private void Transform(byte[] block, int offset)
            {
                for (int i = 0; i < 16; i++)
                    _x[i] = (uint)(block[offset + i * 4] | (block[offset + i * 4 + 1] << 8) | (block[offset + i * 4 + 2] << 16) | (block[offset + i * 4 + 3] << 24));

                uint a = _state[0], b = _state[1], c = _state[2], d = _state[3];

                a = FF(a, b, c, d, _x[0], 3); d = FF(d, a, b, c, _x[1], 7); c = FF(c, d, a, b, _x[2], 11); b = FF(b, c, d, a, _x[3], 19);
                a = FF(a, b, c, d, _x[4], 3); d = FF(d, a, b, c, _x[5], 7); c = FF(c, d, a, b, _x[6], 11); b = FF(b, c, d, a, _x[7], 19);
                a = FF(a, b, c, d, _x[8], 3); d = FF(d, a, b, c, _x[9], 7); c = FF(c, d, a, b, _x[10], 11); b = FF(b, c, d, a, _x[11], 19);
                a = FF(a, b, c, d, _x[12], 3); d = FF(d, a, b, c, _x[13], 7); c = FF(c, d, a, b, _x[14], 11); b = FF(b, c, d, a, _x[15], 19);

                a = GG(a, b, c, d, _x[0], 3); d = GG(d, a, b, c, _x[4], 5); c = GG(c, d, a, b, _x[8], 9); b = GG(b, c, d, a, _x[12], 13);
                a = GG(a, b, c, d, _x[1], 3); d = GG(d, a, b, c, _x[5], 5); c = GG(c, d, a, b, _x[9], 9); b = GG(b, c, d, a, _x[13], 13);
                a = GG(a, b, c, d, _x[2], 3); d = GG(d, a, b, c, _x[6], 5); c = GG(c, d, a, b, _x[10], 9); b = GG(b, c, d, a, _x[14], 13);
                a = GG(a, b, c, d, _x[3], 3); d = GG(d, a, b, c, _x[7], 5); c = GG(c, d, a, b, _x[11], 9); b = GG(b, c, d, a, _x[15], 13);

                a = HH(a, b, c, d, _x[0], 3); d = HH(d, a, b, c, _x[8], 9); c = HH(c, d, a, b, _x[4], 11); b = HH(b, c, d, a, _x[12], 15);
                a = HH(a, b, c, d, _x[2], 3); d = HH(d, a, b, c, _x[10], 9); c = HH(c, d, a, b, _x[6], 11); b = HH(b, c, d, a, _x[14], 15);
                a = HH(a, b, c, d, _x[1], 3); d = HH(d, a, b, c, _x[9], 9); c = HH(c, d, a, b, _x[5], 11); b = HH(b, c, d, a, _x[13], 15);
                a = HH(a, b, c, d, _x[3], 3); d = HH(d, a, b, c, _x[11], 9); c = HH(c, d, a, b, _x[7], 11); b = HH(b, c, d, a, _x[15], 15);

                _state[0] += a; _state[1] += b; _state[2] += c; _state[3] += d;
            }

            private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));
            private static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);
            private static uint G(uint x, uint y, uint z) => (x & y) | (x & z) | (y & z);
            private static uint H(uint x, uint y, uint z) => x ^ y ^ z;
            private static uint FF(uint a, uint b, uint c, uint d, uint x, int s) => RotateLeft(a + F(b, c, d) + x, s);
            private static uint GG(uint a, uint b, uint c, uint d, uint x, int s) => RotateLeft(a + G(b, c, d) + x + 0x5a827999, s);
            private static uint HH(uint a, uint b, uint c, uint d, uint x, int s) => RotateLeft(a + H(b, c, d) + x + 0x6ed9eba1, s);

            private static void Encode(byte[] output, uint[] input, int len)
            {
                for (int i = 0, j = 0; j < len; i++, j += 4)
                {
                    output[j] = (byte)(input[i] & 0xff);
                    output[j + 1] = (byte)((input[i] >> 8) & 0xff);
                    output[j + 2] = (byte)((input[i] >> 16) & 0xff);
                    output[j + 3] = (byte)((input[i] >> 24) & 0xff);
                }
            }

            public void Dispose()
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                Array.Clear(_state, 0, _state.Length);
                Array.Clear(_count, 0, _count.Length);
                Array.Clear(_x, 0, _x.Length);
            }
        }
        #endregion
    }

}
