using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JRadius.Core.Radius;
using JRadius.Core.Radius.Attributes;
using JRadius.Core.Radius.Dictionaries;
using JRadius.Core.Radius.Packet;
using JRadius.Core.Radius.Server;
using JRadius.Core.Radius.Session;
using JRadius.Core.Util;
using JRadius.Extended.Tls.BouncyCastle;
using Microsoft.Extensions.Logging;

namespace JRadius.Extended.Tls
{
    public class TlsProtocolHandler
    {
        // private static final int EXT_RenegotiationInfo = 0xFF01;
        private const int TLS_EMPTY_RENEGOTIATION_INFO_SCSV = 0x00FF;

        private const short RL_CHANGE_CIPHER_SPEC = 20;
        private const short RL_ALERT = 21;
        private const short RL_HANDSHAKE = 22;
        private const short RL_APPLICATION_DATA = 23;

        /*
         * hello_request(0), client_hello(1), server_hello(2), certificate(11),
         * server_key_exchange (12), certificate_request(13), server_hello_done(14),
         * certificate_verify(15), client_key_exchange(16), finished(20), (255)
         */
        private const short HP_HELLO_REQUEST = 0;
        private const short HP_CLIENT_HELLO = 1;
        private const short HP_SERVER_HELLO = 2;
        private const short HP_CERTIFICATE = 11;
        private const short HP_SERVER_KEY_EXCHANGE = 12;
        private const short HP_CERTIFICATE_REQUEST = 13;
        private const short HP_SERVER_HELLO_DONE = 14;
        private const short HP_CERTIFICATE_VERIFY = 15;
        private const short HP_CLIENT_KEY_EXCHANGE = 16;
        private const short HP_FINISHED = 20;

        /*
         * Our Connection states
         */
        private const short CS_CLIENT_HELLO_SEND = 1;
        private const short CS_SERVER_HELLO_RECEIVED = 2;
        private const short CS_SERVER_CERTIFICATE_RECEIVED = 3;
        private const short CS_SERVER_KEY_EXCHANGE_RECEIVED = 4;
        private const short CS_CERTIFICATE_REQUEST_RECEIVED = 5;
        private const short CS_SERVER_HELLO_DONE_RECEIVED = 6;
        private const short CS_CLIENT_KEY_EXCHANGE_SEND = 7;
        private const short CS_CERTIFICATE_VERIFY_SEND = 8;
        private const short CS_CLIENT_CHANGE_CIPHER_SPEC_SEND = 9;
        private const short CS_CLIENT_FINISHED_SEND = 10;
        private const short CS_SERVER_CHANGE_CIPHER_SPEC_RECEIVED = 11;
        public const short CS_DONE = 12;

        /*
         * AlertLevel enum (255)
         */
        // RFC 2246
        protected const short AL_warning = 1;
        protected const short AL_fatal = 2;

        /*
         * AlertDescription enum (255)
         */
        // RFC 2246
        protected const short AP_close_notify = 0;
        protected const short AP_unexpected_message = 10;
        protected const short AP_bad_record_mac = 20;
        protected const short AP_decryption_failed = 21;
        protected const short AP_record_overflow = 22;
        protected const short AP_decompression_failure = 30;
        protected const short AP_handshake_failure = 40;
        protected const short AP_bad_certificate = 42;
        protected const short AP_unsupported_certificate = 43;
        protected const short AP_certificate_revoked = 44;
        protected const short AP_certificate_expired = 45;
        protected const short AP_certificate_unknown = 46;
        protected const short AP_illegal_parameter = 47;
        protected const short AP_unknown_ca = 48;
        protected const short AP_access_denied = 49;
        protected const short AP_decode_error = 50;
        protected const short AP_decrypt_error = 51;
        protected const short AP_export_restriction = 60;
        protected const short AP_protocol_version = 70;
        protected const short AP_insufficient_security = 71;
        protected const short AP_internal_error = 80;
        protected const short AP_user_canceled = 90;
        protected const short AP_no_renegotiation = 100;

        // RFC 4279
        protected const short AP_unknown_psk_identity = 115;

        private static readonly byte[] emptybuf = new byte[0];

        private const string TLS_ERROR_MESSAGE = "Internal TLS error, this could be an attack";

        /*
         * Queues for data from some protocols.
         */
        private ByteQueue applicationDataQueue = new ByteQueue();
        private ByteQueue changeCipherSpecQueue = new ByteQueue();
        private ByteQueue alertQueue = new ByteQueue();
        private ByteQueue handshakeQueue = new ByteQueue();

        /*
         * The Record Stream we use
         */
        private RecordStream rs;
        private RandomNumberGenerator random;

        private TlsInputStream tlsInputStream = null;
        private TlsOutputStream tlsOutputStream = null;

        private bool closed = false;
        private bool failedWithError = false;
        private bool appDataReady = false;
        private bool extendedClientHello;

        private SecurityParameters securityParameters = null;

        private TlsClient tlsClient = null;
        private int[] offeredCipherSuites = null;
        private TlsKeyExchange keyExchange = null;

        private short connection_state = 0;

        private X509Certificate2Collection keyManagers = null;
        private X509Certificate2Collection trustManagers = null;

        private bool isSendCertificate = false;

        private static RandomNumberGenerator CreateSecureRandom()
        {
            return RandomNumberGenerator.Create();
        }

        public TlsProtocolHandler(Stream inputStream, Stream outputStream)
            : this(inputStream, outputStream, CreateSecureRandom())
        {
        }

        public TlsProtocolHandler(Stream inputStream, Stream outputStream, RandomNumberGenerator sr)
        {
            this.rs = new RecordStream(this, inputStream, outputStream);
            this.random = sr;
        }

        public TlsProtocolHandler()
        {
            this.rs = new RecordStream(this);
            this.random = CreateSecureRandom();
        }

        internal RandomNumberGenerator Random
        {
            get { return random; }
        }

        public void SetSendCertificate(bool b)
        {
            this.isSendCertificate = b;
        }

        protected void ProcessData(short protocol, byte[] buf, int offset, int len)
        {
            /*
             * Have a look at the protocol type, and add it to the correct queue.
             */
            switch (protocol)
            {
                case RL_CHANGE_CIPHER_SPEC:
                    changeCipherSpecQueue.AddData(buf, offset, len);
                    ProcessChangeCipherSpec();
                    break;
                case RL_ALERT:
                    alertQueue.AddData(buf, offset, len);
                    ProcessAlert();
                    break;
                case RL_HANDSHAKE:
                    handshakeQueue.AddData(buf, offset, len);
                    ProcessHandshake();
                    break;
                case RL_APPLICATION_DATA:
                    if (!appDataReady)
                    {
                        this.FailWithError(AL_fatal, AP_unexpected_message);
                    }
                    applicationDataQueue.AddData(buf, offset, len);
                    ProcessApplicationData();
                    break;
                default:
                    /*
                     * Uh, we don't know this protocol.
                     *
                     * RFC2246 defines on page 13, that we should ignore this.
                     */
                    break;
            }
        }

        private void ProcessHandshake()
        {
            bool read;
            do
            {
                read = false;
                /*
                 * We need the first 4 bytes, they contain type and length of the message.
                 */
                if (handshakeQueue.Size >= 4)
                {
                    byte[] beginning = new byte[4];
                    handshakeQueue.Read(beginning, 0, 4, 0);
                    MemoryStream bis = new MemoryStream(beginning);
                    short type = TlsUtils.ReadUint8(bis);
                    int len = TlsUtils.ReadUint24(bis);

                    /*
                     * Check if we have enough bytes in the buffer to read the full message.
                     */
                    if (handshakeQueue.Size >= (len + 4))
                    {
                        /*
                         * Read the message.
                         */
                        byte[] buf = new byte[len];
                        handshakeQueue.Read(buf, 0, len, 4);
                        handshakeQueue.RemoveData(len + 4);

                        /*
                         * RFC 2246 7.4.9. "The value handshake_messages includes all
                         * handshake messages starting at client hello up to, but not
                         * including, this finished message. [..] Note: [Also,] Hello Request
                         * messages are omitted from handshake hashes."
                         */
                        switch (type)
                        {
                            case HP_HELLO_REQUEST:
                            case HP_FINISHED:
                                break;
                            default:
                                rs.UpdateHandshakeData(beginning, 0, 4);
                                rs.UpdateHandshakeData(buf, 0, len);
                                break;
                        }

                        /*
                         * Now, parse the message.
                         */
                        ProcessHandshakeMessage(type, buf);
                        read = true;
                    }
                }
            }
            while (read);
        }
    }
}
