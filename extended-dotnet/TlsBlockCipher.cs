using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.IO;
using System.Security.Cryptography;

namespace JRadius.Extended.Tls
{
    public class TlsBlockCipher : TlsCipher
    {
        private readonly TlsProtocolHandler _handler;
        private readonly IBlockCipher _encryptCipher;
        private readonly IBlockCipher _decryptCipher;
        private readonly TlsMac _writeMac;
        private readonly TlsMac _readMac;

        public TlsBlockCipher(TlsProtocolHandler handler, IBlockCipher encryptCipher, IBlockCipher decryptCipher, IDigest writeDigest, IDigest readDigest, int cipherKeySize, SecurityParameters securityParameters)
        {
            _handler = handler;
            _encryptCipher = encryptCipher;
            _decryptCipher = decryptCipher;

            int prfSize = (2 * cipherKeySize) + writeDigest.GetDigestSize() + readDigest.GetDigestSize() + encryptCipher.GetBlockSize() + decryptCipher.GetBlockSize();

            byte[] keyBlock = TlsUtils.PRF(securityParameters.MasterSecret, "key expansion", TlsUtils.Concat(securityParameters.ServerRandom, securityParameters.ClientRandom), prfSize);

            int offset = 0;

            _writeMac = new TlsMac(writeDigest, keyBlock, offset, writeDigest.GetDigestSize());
            offset += writeDigest.GetDigestSize();
            _readMac = new TlsMac(readDigest, keyBlock, offset, readDigest.GetDigestSize());
            offset += readDigest.GetDigestSize();

            InitCipher(true, encryptCipher, keyBlock, cipherKeySize, offset, offset + (cipherKeySize * 2));
            offset += cipherKeySize;
            InitCipher(false, decryptCipher, keyBlock, cipherKeySize, offset, offset + cipherKeySize + encryptCipher.GetBlockSize());
        }

        private void InitCipher(bool forEncryption, IBlockCipher cipher, byte[] keyBlock, int keySize, int keyOffset, int ivOffset)
        {
            var keyParameter = new KeyParameter(keyBlock, keyOffset, keySize);
            var parametersWithIv = new ParametersWithIV(keyParameter, keyBlock, ivOffset, cipher.GetBlockSize());
            cipher.Init(forEncryption, parametersWithIv);
        }

        public byte[] EncodePlaintext(short type, byte[] plaintext, int offset, int len)
        {
            int blocksize = _encryptCipher.GetBlockSize();
            int minPaddingSize = blocksize - ((len + _writeMac.GetSize() + 1) % blocksize);
            int maxExtraPadBlocks = (255 - minPaddingSize) / blocksize;
            int actualExtraPadBlocks = ChooseExtraPadBlocks(new SecureRandom(), maxExtraPadBlocks);
            int paddingsize = minPaddingSize + (actualExtraPadBlocks * blocksize);

            int totalsize = len + _writeMac.GetSize() + paddingsize + 1;
            byte[] outbuf = new byte[totalsize];
            Array.Copy(plaintext, offset, outbuf, 0, len);
            byte[] mac = _writeMac.CalculateMac(type, plaintext, offset, len);
            Array.Copy(mac, 0, outbuf, len, mac.Length);
            int paddoffset = len + mac.Length;
            for (int i = 0; i <= paddingsize; i++)
            {
                outbuf[i + paddoffset] = (byte)paddingsize;
            }
            for (int i = 0; i < totalsize; i += blocksize)
            {
                _encryptCipher.ProcessBlock(outbuf, i, outbuf, i);
            }
            return outbuf;
        }

        public byte[] DecodeCiphertext(short type, byte[] ciphertext, int offset, int len)
        {
            int minLength = _readMac.GetSize() + 1;
            int blocksize = _decryptCipher.GetBlockSize();
            bool decrypterror = false;

            if (len < minLength)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_decode_error);
            }

            if (len % blocksize != 0)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_decryption_failed);
            }

            for (int i = 0; i < len; i += blocksize)
            {
                _decryptCipher.ProcessBlock(ciphertext, i + offset, ciphertext, i + offset);
            }

            int lastByteOffset = offset + len - 1;
            byte paddingsizebyte = ciphertext[lastByteOffset];
            int paddingsize = paddingsizebyte & 0xff;

            int maxPaddingSize = len - minLength;
            if (paddingsize > maxPaddingSize)
            {
                decrypterror = true;
                paddingsize = 0;
            }
            else
            {
                byte diff = 0;
                for (int i = lastByteOffset - paddingsize; i < lastByteOffset; ++i)
                {
                    diff |= (byte)(ciphertext[i] ^ paddingsizebyte);
                }
                if (diff != 0)
                {
                    decrypterror = true;
                    paddingsize = 0;
                }
            }

            int plaintextlength = len - minLength - paddingsize;
            byte[] calculatedMac = _readMac.CalculateMac(type, ciphertext, offset, plaintextlength);
            byte[] decryptedMac = new byte[calculatedMac.Length];
            Array.Copy(ciphertext, offset + plaintextlength, decryptedMac, 0, calculatedMac.Length);

            if (!Arrays.ConstantTimeAreEqual(calculatedMac, decryptedMac))
            {
                decrypterror = true;
            }

            if (decrypterror)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_bad_record_mac);
            }

            byte[] plaintext = new byte[plaintextlength];
            Array.Copy(ciphertext, offset, plaintext, 0, plaintextlength);
            return plaintext;
        }

        private int ChooseExtraPadBlocks(Random r, int max)
        {
            int x = r.Next();
            int n = LowestBitSet(x);
            return Math.Min(n, max);
        }

        private int LowestBitSet(int x)
        {
            if (x == 0)
            {
                return 32;
            }

            int n = 0;
            while ((x & 1) == 0)
            {
                ++n;
                x >>= 1;
            }
            return n;
        }
    }
}
