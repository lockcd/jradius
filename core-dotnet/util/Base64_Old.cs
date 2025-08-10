using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace JRadius.Core.Util
{

    /// <summary>
    /// Encodes and decodes to and from Base64 notation.
    /// This is a C# port of the public domain Java implementation by Robert Harder.
    /// <para>
    /// I am placing this code in the Public Domain. Do with it as you will.
    /// This software comes with no guarantees or warranties but with
    /// plenty of well-wishing instead!
    /// Please visit <a href="http://iharder.net/base64">http://iharder.net/base64</a>
    /// periodically to check for updates or to contribute improvements.
    /// </para>
    /// </summary>
    /// <author>Robert Harder</author>
    /// <author>rob@iharder.net</author>
    /// <version>2.1</version>
    public sealed class Base64_Old
    {
        #region Public Fields
        /// <summary>No options specified. Value is zero.</summary>
        public const int NO_OPTIONS = 0;
        /// <summary>Specify encoding.</summary>
        public const int ENCODE = 1;
        /// <summary>Specify decoding.</summary>
        public const int DECODE = 0;
        /// <summary>Specify that data should be gzip-compressed.</summary>
        public const int GZIP = 2;
        /// <summary>Don't break lines when encoding (violates strict Base64 specification)</summary>
        public const int DONT_BREAK_LINES = 8;
        #endregion

        #region Private Fields
        /// <summary>Maximum line length (76) of Base64 output.</summary>
        private const int MAX_LINE_LENGTH = 76;
        /// <summary>The equals sign (=) as a byte.</summary>
        private const byte EQUALS_SIGN = (byte)'=';
        /// <summary>The new line character (\n) as a byte.</summary>
        private const byte NEW_LINE = (byte)'\n';
        /// <summary>Preferred encoding.</summary>
        private const string PREFERRED_ENCODING = "UTF-8";

        /// <summary>The 64 valid Base64 values.</summary>
        private static readonly byte[] ALPHABET;

        /// <summary> 
        /// Translates a Base64 value to either its 6-bit reconstruction value
        /// or a negative number indicating some other meaning.
        /// </summary>
        private static readonly sbyte[] DECODABET =
        {
            -9,-9,-9,-9,-9,-9,-9,-9,-9,                 // Decimal 0 - 8
            -5,-5,                                      // Whitespace: Tab and Linefeed
            -9,-9,                                      // Decimal 11 - 12
            -5,                                         // Whitespace: Carriage Return
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 14 - 26
            -9,-9,-9,-9,-9,                             // Decimal 27 - 31
            -5,                                         // Whitespace: Space
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,              // Decimal 33 - 42
            62,                                         // Plus sign at decimal 43
            -9,-9,-9,                                   // Decimal 44 - 46
            63,                                         // Slash at decimal 47
            52,53,54,55,56,57,58,59,60,61,              // Numbers 0 through 9
            -9,-9,-9,                                   // Decimal 58 - 60
            -1,                                         // Equals sign at decimal 61
            -9,-9,-9,                                   // Decimal 62 - 64
            0,1,2,3,4,5,6,7,8,9,10,11,12,13,            // Letters 'A' through 'N'
            14,15,16,17,18,19,20,21,22,23,24,25,        // Letters 'O' through 'Z'
            -9,-9,-9,-9,-9,-9,                          // Decimal 91 - 96
            26,27,28,29,30,31,32,33,34,35,36,37,38,     // Letters 'a' through 'm'
            39,40,41,42,43,44,45,46,47,48,49,50,51,     // Letters 'n' through 'z'
            -9,-9,-9,-9                                 // Decimal 123 - 126
            /*,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,  // Decimal 127 - 139
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 140 - 152
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 153 - 165
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 166 - 178
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 179 - 191
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 192 - 204
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 205 - 217
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 218 - 230
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,     // Decimal 231 - 243
            -9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9,-9         // Decimal 244 - 255 */
        };

        private const sbyte WHITE_SPACE_ENC = -5; // Indicates white space in encoding
        private const sbyte EQUALS_SIGN_ENC = -1; // Indicates equals sign in encoding
        #endregion

        /// <summary>Static constructor to initialize static fields.</summary>
        static Base64_Old()
        {
            try
            {
                ALPHABET = Encoding.GetEncoding(PREFERRED_ENCODING).GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/");
            }
            catch (System.Exception)
            {
                // Fall back to ASCII if UTF-8 is not supported
                ALPHABET = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/");
            }
        }

        /// <summary>Defeats instantiation.</summary>
        private Base64_Old() { }

        #region Encoding Methods

        /// <summary>
        /// Encodes up to three bytes of the array <paramref name="source"/>
        /// and writes the resulting four Base64 bytes to <paramref name="destination"/>.
        /// </summary>
        private static byte[] Encode3to4(byte[] source, int srcOffset, int numSigBytes, byte[] destination, int destOffset)
        {
            // 1 2 3
            // 012345678901234567890123 Bit position
            // --------000000001111111122222222 Array position from threeBytes
            // --------| || || || | Six bit groups to index ALPHABET
            // >>18 >>12 >> 6 >> 0 Right shift necessary
            // 0x3f 0x3f 0x3f Additional AND

            // C# does not have an unsigned right shift operator (>>>), so we cast to uint to get the same behavior.
            uint inBuff = (uint)(numSigBytes > 0 ? (source[srcOffset] << 24) >> 8 : 0)
                        | (uint)(numSigBytes > 1 ? (source[srcOffset + 1] << 24) >> 16 : 0)
                        | (uint)(numSigBytes > 2 ? (source[srcOffset + 2] << 24) >> 24 : 0);

            switch (numSigBytes)
            {
                case 3:
                    destination[destOffset] = ALPHABET[(inBuff >> 18)];
                    destination[destOffset + 1] = ALPHABET[(inBuff >> 12) & 0x3f];
                    destination[destOffset + 2] = ALPHABET[(inBuff >> 6) & 0x3f];
                    destination[destOffset + 3] = ALPHABET[(inBuff) & 0x3f];
                    return destination;
                case 2:
                    destination[destOffset] = ALPHABET[(inBuff >> 18)];
                    destination[destOffset + 1] = ALPHABET[(inBuff >> 12) & 0x3f];
                    destination[destOffset + 2] = ALPHABET[(inBuff >> 6) & 0x3f];
                    destination[destOffset + 3] = EQUALS_SIGN;
                    return destination;
                case 1:
                    destination[destOffset] = ALPHABET[(inBuff >> 18)];
                    destination[destOffset + 1] = ALPHABET[(inBuff >> 12) & 0x3f];
                    destination[destOffset + 2] = EQUALS_SIGN;
                    destination[destOffset + 3] = EQUALS_SIGN;
                    return destination;
                default:
                    return destination;
            }
        }

        /// <summary>
        /// Serializes an object and returns the Base64-encoded version of that serialized object.
        /// </summary>
        public static string EncodeObject(object serializableObject)
        {
            return EncodeObject(serializableObject, NO_OPTIONS);
        }

        /// <summary>
        /// Serializes an object and returns the Base64-encoded version of that serialized object.
        /// </summary>
        /// <remarks>
        /// **Security Warning:** The BinaryFormatter is obsolete and insecure. It is not recommended for use in modern applications.
        /// This method is provided for compatibility with the original Java implementation.
        /// </remarks>
        public static string EncodeObject(object serializableObject, int options)
        {
            if (serializableObject == null)
                throw new ArgumentNullException(nameof(serializableObject));

            MemoryStream baos = null;
            Stream b64os = null;

            try
            {
                baos = new MemoryStream();
                b64os = new Base64_Old.OutputStream(baos, ENCODE | options);

                // GZip?
                if ((options & GZIP) == GZIP)
                {
                    using (var gzos = new GZipStream(b64os, CompressionMode.Compress, true))
                    {
#pragma warning disable SYSLIB0011
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(gzos, serializableObject);
#pragma warning restore SYSLIB0011
                    }
                }
                else
                {
#pragma warning disable SYSLIB0011
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(b64os, serializableObject);
#pragma warning restore SYSLIB0011
                }
            }
            catch (IOException e)
            {
                Trace.TraceWarning("Base64.EncodeObject: " + e.Message);
                return null;
            }
            finally
            {
                b64os?.Close();
                baos?.Close();
            }

            try
            {
                return Encoding.GetEncoding(PREFERRED_ENCODING).GetString(baos.ToArray());
            }
            catch (System.Exception)
            {
                return Encoding.ASCII.GetString(baos.ToArray());
            }
        }

        /// <summary>
        /// Encodes a byte array into Base64 notation. Does not GZip-compress data.
        /// </summary>
        public static string EncodeBytes(byte[] source)
        {
            return EncodeBytes(source, 0, source.Length, NO_OPTIONS);
        }

        /// <summary>
        /// Encodes a byte array into Base64 notation.
        /// </summary>
        public static string EncodeBytes(byte[] source, int options)
        {
            return EncodeBytes(source, 0, source.Length, options);
        }

        /// <summary>
        /// Encodes a byte array into Base64 notation.
        /// </summary>
        public static string EncodeBytes(byte[] source, int off, int len, int options)
        {
            int dontBreakLines = (options & DONT_BREAK_LINES);
            int gzip = (options & GZIP);

            if (gzip == GZIP)
            {
                MemoryStream baos = null;
                GZipStream gzos = null;
                OutputStream b64os = null;
                try
                {
                    baos = new MemoryStream();
                    b64os = new OutputStream(baos, ENCODE | dontBreakLines);
                    gzos = new GZipStream(b64os, CompressionMode.Compress, true);

                    gzos.Write(source, off, len);
                }
                catch (IOException e)
                {
                    Trace.TraceWarning("Base64.EncodeBytes: " + e.Message);
                    return null;
                }
                finally
                {
                    gzos?.Close();
                    b64os?.Close();
                    baos?.Close();
                }

                try
                {
                    return Encoding.GetEncoding(PREFERRED_ENCODING).GetString(baos.ToArray());
                }
                catch (System.Exception)
                {
                    return Encoding.ASCII.GetString(baos.ToArray());
                }
            }
            else
            {
                bool breakLines = dontBreakLines == 0;
                int len43 = len * 4 / 3;
                byte[] outBuff = new byte[len43 + ((len % 3) > 0 ? 4 : 0) + (breakLines ? (len43 / MAX_LINE_LENGTH) : 0)];
                int d = 0;
                int e = 0;
                int len2 = len - 2;
                int lineLength = 0;
                for (; d < len2; d += 3, e += 4)
                {
                    Encode3to4(source, d + off, 3, outBuff, e);
                    lineLength += 4;
                    if (breakLines && lineLength == MAX_LINE_LENGTH)
                    {
                        outBuff[e + 4] = NEW_LINE;
                        e++;
                        lineLength = 0;
                    }
                }

                if (d < len)
                {
                    Encode3to4(source, d + off, len - d, outBuff, e);
                    e += 4;
                }

                try
                {
                    return Encoding.GetEncoding(PREFERRED_ENCODING).GetString(outBuff, 0, e);
                }
                catch (System.Exception)
                {
                    return Encoding.ASCII.GetString(outBuff, 0, e);
                }
            }
        }
        #endregion

        #region Decoding Methods

        /// <summary>
        /// Decodes four bytes from array <paramref name="source"/>
        /// and writes the resulting bytes (up to three of them)
        /// to <paramref name="destination"/>.
        /// </summary>
        private static int Decode4to3(byte[] source, int srcOffset, byte[] destination, int destOffset)
        {
            if (source[srcOffset + 2] == EQUALS_SIGN)
            {
                int outBuff = ((DECODABET[source[srcOffset]] & 0xFF) << 18)
                            | ((DECODABET[source[srcOffset + 1]] & 0xFF) << 12);
                destination[destOffset] = (byte)(outBuff >> 16);
                return 1;
            }
            else if (source[srcOffset + 3] == EQUALS_SIGN)
            {
                int outBuff = ((DECODABET[source[srcOffset]] & 0xFF) << 18)
                            | ((DECODABET[source[srcOffset + 1]] & 0xFF) << 12)
                            | ((DECODABET[source[srcOffset + 2]] & 0xFF) << 6);
                destination[destOffset] = (byte)(outBuff >> 16);
                destination[destOffset + 1] = (byte)(outBuff >> 8);
                return 2;
            }
            else
            {
                try
                {
                    int outBuff = ((DECODABET[source[srcOffset]] & 0xFF) << 18)
                                | ((DECODABET[source[srcOffset + 1]] & 0xFF) << 12)
                                | ((DECODABET[source[srcOffset + 2]] & 0xFF) << 6)
                                | ((DECODABET[source[srcOffset + 3]] & 0xFF));

                    destination[destOffset] = (byte)(outBuff >> 16);
                    destination[destOffset + 1] = (byte)(outBuff >> 8);
                    destination[destOffset + 2] = (byte)(outBuff);
                    return 3;
                }
                catch (System.Exception)
                {
                    Trace.TraceError("Base64 decode error at source offset " + srcOffset);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Very low-level access to decoding ASCII characters in the form of a byte array.
        /// </summary>
        public static byte[] Decode(byte[] source, int off, int len)
        {
            int len34 = len * 3 / 4;
            byte[] outBuff = new byte[len34];
            int outBuffPosn = 0;
            byte[] b4 = new byte[4];
            int b4Posn = 0;
            byte sbiCrop;
            sbyte sbiDecode;

            for (int i = off; i < off + len; i++)
            {
                sbiCrop = (byte)(source[i] & 0x7f);
                sbiDecode = DECODABET[sbiCrop];

                if (sbiDecode >= WHITE_SPACE_ENC)
                {
                    if (sbiDecode >= EQUALS_SIGN_ENC)
                    {
                        b4[b4Posn++] = sbiCrop;
                        if (b4Posn > 3)
                        {
                            outBuffPosn += Decode4to3(b4, 0, outBuff, outBuffPosn);
                            b4Posn = 0;
                            if (sbiCrop == EQUALS_SIGN)
                                break;
                        }
                    }
                }
                else
                {
                    Trace.TraceError($"Bad Base64 input character at {i}: {source[i]} (decimal)");
                    return null;
                }
            }

            byte[] result = new byte[outBuffPosn];
            Array.Copy(outBuff, 0, result, 0, outBuffPosn);
            return result;
        }

        /// <summary>
        /// Decodes data from Base64 notation, automatically detecting gzip-compressed data.
        /// </summary>
        public static byte[] Decode(string s)
        {
            byte[] bytes;
            try
            {
                bytes = Encoding.GetEncoding(PREFERRED_ENCODING).GetBytes(s);
            }
            catch
            {
                bytes = Encoding.ASCII.GetBytes(s);
            }

            bytes = Decode(bytes, 0, bytes.Length);

            if (bytes != null && bytes.Length >= 2)
            {
                int head = (bytes[0] & 0xff) | ((bytes[1] << 8) & 0xff00);
                if (head == GZipStream_GZIP_MAGIC) // GZIP_MAGIC = 0x8b1f
                {
                    MemoryStream bais = null;
                    GZipStream gzis = null;
                    MemoryStream baos = null;
                    byte[] buffer = new byte[2048];
                    int length;

                    try
                    {
                        baos = new MemoryStream();
                        bais = new MemoryStream(bytes);
                        gzis = new GZipStream(bais, CompressionMode.Decompress);

                        while ((length = gzis.Read(buffer, 0, buffer.Length)) >= 0)
                        {
                            baos.Write(buffer, 0, length);
                        }
                        bytes = baos.ToArray();
                    }
                    catch (IOException e)
                    {
                        Trace.TraceWarning("Base64.Decode GZIP Error: " + e.Message);
                    }
                    finally
                    {
                        baos?.Close();
                        gzis?.Close();
                        bais?.Close();
                    }
                }
            }
            return bytes;
        }
        // GZIP magic number is not public in GZipStream, so we define it here.
        private const int GZipStream_GZIP_MAGIC = 0x8b1f;

        /// <summary>
        /// Attempts to decode Base64 data and deserialize a .NET Object within.
        /// </summary>
        /// <remarks>
        /// **Security Warning:** The BinaryFormatter is obsolete and insecure. It is not recommended for use in modern applications.
        /// This method is provided for compatibility with the original Java implementation.
        /// </remarks>
        public static object DecodeToObject(string encodedObject)
        {
            byte[] objBytes = Decode(encodedObject);
            MemoryStream bais = null;
            object obj = null;

            try
            {
                bais = new MemoryStream(objBytes);
#pragma warning disable SYSLIB0011
                var formatter = new BinaryFormatter();
                obj = formatter.Deserialize(bais);
#pragma warning restore SYSLIB0011
            }
            catch (IOException e)
            {
                Trace.TraceWarning("Base64.DecodeToObject (IO): " + e.Message);
                return null;
            }
            catch (SerializationException e)
            {
                Trace.TraceWarning("Base64.DecodeToObject (Serialization): " + e.Message);
                return null;
            }
            finally
            {
                bais?.Close();
            }
            return obj;
        }
        #endregion

        #region Inner Classes
        /// <summary>
        /// A <see cref="Base64.InputStream"/> will read data from another
        /// <see cref="System.IO.Stream"/>, given in the constructor,
        /// and encode/decode to/from Base64 notation on the fly.
        /// </summary>
        public class InputStream : Stream
        {
            private readonly Stream _innerStream;
            private readonly bool _encode;
            private int _position;
            private readonly byte[] _buffer;
            private readonly int _bufferLength;
            private int _numSigBytes;
            private int _lineLength;
            private readonly bool _breakLines;

            public InputStream(Stream innerStream, int options = DECODE)
            {
                _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
                _breakLines = (options & DONT_BREAK_LINES) != DONT_BREAK_LINES;
                _encode = (options & ENCODE) == ENCODE;
                _bufferLength = _encode ? 4 : 3;
                _buffer = new byte[_bufferLength];
                _position = -1;
                _lineLength = 0;
            }

            public override int Read(byte[] destination, int offset, int count)
            {
                int i;
                int b;
                for (i = 0; i < count; i++)
                {
                    b = ReadByte();
                    if (b >= 0)
                        destination[offset + i] = (byte)b;
                    else if (i == 0)
                        return -1;
                    else
                        break;
                }
                return i;
            }

            public override int ReadByte()
            {
                if (_position < 0)
                {
                    if (_encode)
                    {
                        byte[] b3 = new byte[3];
                        int numBinaryBytes = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            int b = _innerStream.ReadByte();
                            if (b >= 0)
                            {
                                b3[i] = (byte)b;
                                numBinaryBytes++;
                            }
                            else break;
                        }

                        if (numBinaryBytes > 0)
                        {
                            Encode3to4(b3, 0, numBinaryBytes, _buffer, 0);
                            _position = 0;
                            _numSigBytes = 4;
                        }
                        else return -1;
                    }
                    else // Decoding
                    {
                        byte[] b4 = new byte[4];
                        int i = 0;
                        for (i = 0; i < 4; i++)
                        {
                            int b;
                            do { b = _innerStream.ReadByte(); }
                            while (b >= 0 && DECODABET[b & 0x7f] <= WHITE_SPACE_ENC);

                            if (b < 0) break;
                            b4[i] = (byte)b;
                        }

                        if (i == 4)
                        {
                            _numSigBytes = Decode4to3(b4, 0, _buffer, 0);
                            _position = 0;
                        }
                        else if (i == 0) return -1;
                        else throw new IOException("Improperly padded Base64 input.");
                    }
                }

                if (_position >= 0)
                {
                    if (_position >= _numSigBytes) return -1;
                    if (_encode && _breakLines && _lineLength >= MAX_LINE_LENGTH)
                    {
                        _lineLength = 0;
                        return '\n';
                    }
                    else
                    {
                        _lineLength++;
                        int b = _buffer[_position++];
                        if (_position >= _bufferLength) _position = -1;
                        return b & 0xFF;
                    }
                }
                throw new IOException("Error in Base64 code reading stream.");
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override void Flush() => _innerStream.Flush();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        /// <summary>
        /// A <see cref="Base64.OutputStream"/> will write data to another
        /// <see cref="System.IO.Stream"/>, given in the constructor,
        /// and encode/decode to/from Base64 notation on the fly.
        /// </summary>
        public class OutputStream : Stream
        {
            private readonly Stream _innerStream;
            private readonly bool _encode;
            private int _position;
            private readonly byte[] _buffer;
            private readonly int _bufferLength;
            private int _lineLength;
            private readonly bool _breakLines;
            private readonly byte[] _b4; // Scratch used in a few places
            private bool _suspendEncoding;

            public OutputStream(Stream innerStream, int options = ENCODE)
            {
                _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
                _breakLines = (options & DONT_BREAK_LINES) != DONT_BREAK_LINES;
                _encode = (options & ENCODE) == ENCODE;
                _bufferLength = _encode ? 3 : 4;
                _buffer = new byte[_bufferLength];
                _position = 0;
                _lineLength = 0;
                _suspendEncoding = false;
                _b4 = new byte[4];
            }

            public override void Write(byte[] aBuffer, int offset, int count)
            {
                if (_suspendEncoding)
                {
                    _innerStream.Write(aBuffer, offset, count);
                    return;
                }
                for (int i = 0; i < count; i++)
                {
                    WriteByte(aBuffer[offset + i]);
                }
            }

            public override void WriteByte(byte theByte)
            {
                if (_suspendEncoding)
                {
                    _innerStream.WriteByte(theByte);
                    return;
                }

                if (_encode)
                {
                    _buffer[_position++] = theByte;
                    if (_position >= _bufferLength)
                    {
                        Encode3to4(_buffer, 0, _bufferLength, _b4, 0);
                        _innerStream.Write(_b4, 0, 4);

                        _lineLength += 4;
                        if (_breakLines && _lineLength >= MAX_LINE_LENGTH)
                        {
                            _innerStream.WriteByte(NEW_LINE);
                            _lineLength = 0;
                        }
                        _position = 0;
                    }
                }
                else // Decoding
                {
                    if (DECODABET[theByte & 0x7f] > WHITE_SPACE_ENC)
                    {
                        _buffer[_position++] = theByte;
                        if (_position >= _bufferLength)
                        {
                            int len = Decode4to3(_buffer, 0, _b4, 0);
                            _innerStream.Write(_b4, 0, len);
                            _position = 0;
                        }
                    }
                    else if (DECODABET[theByte & 0x7f] != WHITE_SPACE_ENC)
                    {
                        throw new IOException("Invalid character in Base64 data.");
                    }
                }
            }

            public override void Flush()
            {
                if (_position > 0)
                {
                    if (_encode)
                    {
                        Encode3to4(_buffer, 0, _position, _b4, 0);
                        _innerStream.Write(_b4, 0, 4);
                        _position = 0;
                    }
                    else
                    {
                        throw new IOException("Base64 input not properly padded.");
                    }
                }
                _innerStream.Flush();
            }

            public override void Close()
            {
                Flush();
                _innerStream.Close();
                base.Close();
            }

            public void SuspendEncoding()
            {
                Flush();
                _suspendEncoding = true;
            }

            public void ResumeEncoding()
            {
                _suspendEncoding = false;
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }
        #endregion
    }


}
