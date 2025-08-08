using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
//using JRadius.Core.Packet;
//using JRadius.Core.Packet.Attribute;
//using JRadius.Core.Server;
//using JRadius.Core.Session;
//using JRadius.Core.Util;
using Org.BouncyCastle.Asn1.X509;
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

        private void ProcessHandshakeMessage(short type, byte[] buf)
        {
            MemoryStream inputStream = new MemoryStream(buf);

            switch (type)
            {
                case HP_CERTIFICATE:
                    {
                        switch (connection_state)
                        {
                            case CS_SERVER_HELLO_RECEIVED:
                                {
                                    // Parse the Certificate message and send to cipher suite

                                    Certificate serverCertificate = Certificate.parse(inputStream);

                                    AssertEmpty(inputStream);

                                    this.keyExchange.ProcessServerCertificate(serverCertificate);

                                    break;
                                }
                            default:
                                this.FailWithError(AL_fatal, AP_unexpected_message);
                                break;
                        }

                        connection_state = CS_SERVER_CERTIFICATE_RECEIVED;
                        break;
                    }
                case HP_FINISHED:
                    switch (connection_state)
                    {
                        case CS_SERVER_CHANGE_CIPHER_SPEC_RECEIVED:
                            /*
                             * Read the checksum from the finished message, it has always 12
                             * bytes.
                             */
                            byte[] serverVerifyData = new byte[12];
                            TlsUtils.ReadFully(serverVerifyData, inputStream);

                            AssertEmpty(inputStream);

                            /*
                             * Calculate our own checksum.
                             */
                            byte[] expectedServerVerifyData = TlsUtils.PRF(
                                securityParameters.masterSecret, "server finished",
                                rs.getCurrentHash(), 12);

                            /*
                             * Compare both checksums.
                             */
                            if (!Arrays.ConstantTimeAreEqual(expectedServerVerifyData, serverVerifyData))
                            {
                                /*
                                 * Wrong checksum in the finished message.
                                 */
                                this.FailWithError(AL_fatal, AP_handshake_failure);
                            }

                            connection_state = CS_DONE;

                            /*
                             * We are now ready to receive application data.
                             */
                            this.appDataReady = true;
                            break;
                        default:
                            this.FailWithError(AL_fatal, AP_unexpected_message);
                            break;
                    }
                    break;
                case HP_SERVER_HELLO:
                    switch (connection_state)
                    {
                        case CS_CLIENT_HELLO_SEND:
                            /*
                             * Read the server hello message
                             */
                            TlsUtils.CheckVersion(inputStream, this);

                            /*
                             * Read the server random
                             */
                            securityParameters.serverRandom = new byte[32];
                            TlsUtils.ReadFully(securityParameters.serverRandom, inputStream);

                            byte[] sessionID = TlsUtils.ReadOpaque8(inputStream);
                            if (sessionID.Length > 32)
                            {
                                this.FailWithError(TlsProtocolHandler.AL_fatal,
                                    TlsProtocolHandler.AP_illegal_parameter);
                            }

                            this.tlsClient.NotifySessionID(sessionID);

                            /*
                             * Find out which ciphersuite the server has chosen and check that
                             * it was one of the offered ones.
                             */
                            int selectedCipherSuite = TlsUtils.ReadUint16(inputStream);
                            if (!WasCipherSuiteOffered(selectedCipherSuite))
                            {
                                this.FailWithError(TlsProtocolHandler.AL_fatal,
                                    TlsProtocolHandler.AP_illegal_parameter);
                            }

                            this.tlsClient.NotifySelectedCipherSuite(selectedCipherSuite);

                            /*
                             * We support only the null compression which means no
                             * compression.
                             */
                            short compressionMethod = TlsUtils.ReadUint8(inputStream);
                            if (compressionMethod != 0)
                            {
                                this.FailWithError(TlsProtocolHandler.AL_fatal,
                                    TlsProtocolHandler.AP_illegal_parameter);
                            }

                            /*
                             * RFC4366 2.2 The extended server hello message format MAY be
                             * sent in place of the server hello message when the client has
                             * requested extended functionality via the extended client hello
                             * message specified in Section 2.1.
                             */
                            if (extendedClientHello)
                            {
                                // Integer -> byte[]
                                Hashtable serverExtensions = new Hashtable();

                                if (inputStream.Position < inputStream.Length)
                                {
                                    // Process extensions from extended server hello
                                    byte[] extBytes = TlsUtils.ReadOpaque16(inputStream);

                                    MemoryStream ext = new MemoryStream(extBytes);
                                    while (ext.Position < ext.Length)
                                    {
                                        int extType = TlsUtils.ReadUint16(ext);
                                        byte[] extValue = TlsUtils.ReadOpaque16(ext);

                                        serverExtensions.Add(extType, extValue);
                                    }
                                }

                                // TODO[RFC 5746] If renegotiation_info was sent in client hello, check here

                                tlsClient.ProcessServerExtensions(serverExtensions);
                            }

                            AssertEmpty(inputStream);

                            this.keyExchange = tlsClient.CreateKeyExchange();

                            connection_state = CS_SERVER_HELLO_RECEIVED;
                            break;
                        default:
                            this.FailWithError(AL_fatal, AP_unexpected_message);
                            break;
                    }
                    break;
                case HP_SERVER_HELLO_DONE:
                    switch (connection_state)
                    {
                        case CS_SERVER_CERTIFICATE_RECEIVED:

                            // There was no server key exchange message; check it's OK
                            this.keyExchange.SkipServerKeyExchange();

                            // NB: Fall through to next case label
                            break;
                        case CS_SERVER_KEY_EXCHANGE_RECEIVED:
                        case CS_CERTIFICATE_REQUEST_RECEIVED:

                            AssertEmpty(inputStream);

                            bool isClientCertificateRequested = (connection_state == CS_CERTIFICATE_REQUEST_RECEIVED || isSendCertificate);

                            connection_state = CS_SERVER_HELLO_DONE_RECEIVED;

                            if (isClientCertificateRequested)
                            {
                                SendClientCertificate(tlsClient.GetCertificate());
                            }

                            /*
                             * Send the client key exchange message, depending on the key
                             * exchange we are using in our ciphersuite.
                             */
                            SendClientKeyExchange(this.keyExchange.GenerateClientKeyExchange());

                            connection_state = CS_CLIENT_KEY_EXCHANGE_SEND;

                            if (isClientCertificateRequested)
                            {
                                byte[] clientCertificateSignature = tlsClient.GenerateCertificateSignature(rs.getCurrentHash());
                                if (clientCertificateSignature != null)
                                {
                                    SendCertificateVerify(clientCertificateSignature);

                                    connection_state = CS_CERTIFICATE_VERIFY_SEND;
                                }
                            }

                            /*
                             * Now, we send change cipher state
                             */
                            byte[] cmessage = new byte[1];
                            cmessage[0] = 1;
                            rs.WriteMessage(RL_CHANGE_CIPHER_SPEC, cmessage, 0, cmessage.Length);

                            connection_state = CS_CLIENT_CHANGE_CIPHER_SPEC_SEND;

                            /*
                             * Calculate the master_secret
                             */
                            byte[] pms = this.keyExchange.GeneratePremasterSecret();

                            securityParameters.masterSecret = TlsUtils.PRF(pms, "master secret",
                                TlsUtils.Concat(securityParameters.clientRandom,
                                    securityParameters.serverRandom), 48);

                            // TODO Is there a way to ensure the data is really overwritten?
                            /*
                             * RFC 2246 8.1. "The pre_master_secret should be deleted from
                             * memory once the master_secret has been computed."
                             */
                            Array.Clear(pms, 0, pms.Length);

                            /*
                             * Initialize our cipher suite
                             */
                            rs.clientCipherSpecDecided(tlsClient.createCipher(securityParameters));

                            /*
                             * Send our finished message.
                             */
                            byte[] clientVerifyData = TlsUtils.PRF(securityParameters.masterSecret,
                                "client finished", rs.getCurrentHash(), 12);

                            MemoryStream bos = new MemoryStream();
                            TlsUtils.WriteUint8(HP_FINISHED, bos);
                            TlsUtils.WriteOpaque24(clientVerifyData, bos);
                            byte[] message = bos.ToArray();

                            rs.WriteMessage(RL_HANDSHAKE, message, 0, message.Length);

                            this.connection_state = CS_CLIENT_FINISHED_SEND;
                            break;
                        default:
                            this.FailWithError(AL_fatal, AP_handshake_failure);
                            break;
                    }
                    break;
                case HP_SERVER_KEY_EXCHANGE:
                    {
                        switch (connection_state)
                        {
                            case CS_SERVER_HELLO_RECEIVED:

                                // There was no server certificate message; check it's OK
                                this.keyExchange.SkipServerCertificate();

                                // NB: Fall through to next case label
                                break;
                            case CS_SERVER_CERTIFICATE_RECEIVED:

                                this.keyExchange.ProcessServerKeyExchange(inputStream, securityParameters);

                                AssertEmpty(inputStream);
                                break;

                            default:
                                this.FailWithError(AL_fatal, AP_unexpected_message);
                                break;
                        }

                        this.connection_state = CS_SERVER_KEY_EXCHANGE_RECEIVED;
                        break;
                    }
                case HP_CERTIFICATE_REQUEST:
                    {
                        switch (connection_state)
                        {
                            case CS_SERVER_CERTIFICATE_RECEIVED:

                                // There was no server key exchange message; check it's OK
                                this.keyExchange.SkipServerKeyExchange();

                                // NB: Fall through to next case label
                                break;
                            case CS_SERVER_KEY_EXCHANGE_RECEIVED:
                                {
                                    byte[] types = TlsUtils.ReadOpaque8(inputStream);
                                    byte[] authorities = TlsUtils.ReadOpaque16(inputStream);

                                    AssertEmpty(inputStream);

                                    ArrayList authorityDNs = new ArrayList();

                                    MemoryStream bis = new MemoryStream(authorities);
                                    while (bis.Position < bis.Length)
                                    {
                                        byte[] dnBytes = TlsUtils.ReadOpaque16(bis);
                                        authorityDNs.Add(new X509Name(System.Text.Encoding.UTF8.GetString(dnBytes)));
                                    }

                                    this.tlsClient.ProcessServerCertificateRequest(types, authorityDNs);

                                    break;
                                }
                            default:
                                this.FailWithError(AL_fatal, AP_unexpected_message);
                                break;
                        }

                        this.connection_state = CS_CERTIFICATE_REQUEST_RECEIVED;
                        break;
                    }
                case HP_HELLO_REQUEST:
                    /*
                     * RFC 2246 7.4.1.1 Hello request
                     * "This message will be ignored by the client if the client is currently
                     * negotiating a session. This message may be ignored by the client if it
                     * does not wish to renegotiate a session, or the client may, if it wishes,
                     * respond with a no_renegotiation alert."
                     */
                    if (connection_state == CS_DONE)
                    {
                        // Renegotiation not supported yet
                        SendAlert(AL_warning, AP_no_renegotiation);
                    }
                    break;
                case HP_CLIENT_KEY_EXCHANGE:
                case HP_CERTIFICATE_VERIFY:
                case HP_CLIENT_HELLO:
                default:
                    // We do not support this!
                    this.FailWithError(AL_fatal, AP_unexpected_message);
                    break;
            }
        }

        protected void AssertEmpty(MemoryStream inputStream)
        {
            if (inputStream.Position < inputStream.Length)
            {
                this.FailWithError(AL_fatal, AP_decode_error);
            }
        }

        protected void FailWithError(short alertLevel, short alertDescription)
        {
            /*
             * Check if the connection is still open.
             */
            if (!closed)
            {
                /*
                 * Prepare the message
                 */
                this.closed = true;

                if (alertLevel == AL_fatal)
                {
                    /*
                     * This is a fatal message.
                     */
                    this.failedWithError = true;
                }
                SendAlert(alertLevel, alertDescription);
                rs.Close();
                if (alertLevel == AL_fatal)
                {
                    throw new IOException(TLS_ERROR_MESSAGE);
                }
            }
            else
            {
                throw new IOException(TLS_ERROR_MESSAGE);
            }
        }

        private void SendAlert(short alertLevel, short alertDescription)
        {
            byte[] error = new byte[2];
            error[0] = (byte)alertLevel;
            error[1] = (byte)alertDescription;

            rs.WriteMessage(RL_ALERT, error, 0, 2);
        }

        private bool WasCipherSuiteOffered(int cipherSuite)
        {
            for (int i = 0; i < offeredCipherSuites.Length; ++i)
            {
                if (offeredCipherSuites[i] == cipherSuite)
                {
                    return true;
                }
            }
            return false;
        }

        private void SendClientCertificate(Certificate clientCert)
        {
            MemoryStream bos = new MemoryStream();
            TlsUtils.WriteUint8(HP_CERTIFICATE, bos);
            clientCert.encode(bos);
            byte[] message = bos.ToArray();

            rs.WriteMessage(RL_HANDSHAKE, message, 0, message.Length);
        }

        private void SendClientKeyExchange(byte[] keData)
        {
            MemoryStream bos = new MemoryStream();
            TlsUtils.WriteUint8(HP_CLIENT_KEY_EXCHANGE, bos);
            if (keData == null)
            {
                TlsUtils.WriteUint24(0, bos);
            }
            else
            {
                TlsUtils.WriteUint24(keData.Length + 2, bos);
                TlsUtils.WriteOpaque16(keData, bos);
            }
            byte[] message = bos.ToArray();

            rs.WriteMessage(RL_HANDSHAKE, message, 0, message.Length);
        }

        private void SendCertificateVerify(byte[] data)
        {
            /*
             * Send signature of handshake messages so far to prove we are the owner of the
             * cert See RFC 2246 sections 4.7, 7.4.3 and 7.4.8
             */
            MemoryStream bos = new MemoryStream();
            TlsUtils.WriteUint8(HP_CERTIFICATE_VERIFY, bos);
            TlsUtils.WriteUint24(data.Length + 2, bos);
            TlsUtils.WriteOpaque16(data, bos);
            byte[] message = bos.ToArray();

            rs.WriteMessage(RL_HANDSHAKE, message, 0, message.Length);
        }

        private void ProcessApplicationData()
        {
            /*
             * There is nothing we need to do here.
             *
             * This function could be used for callbacks when application data arrives in the
             * future.
             */
        }

        private void ProcessAlert()
        {
            while (alertQueue.Size >= 2)
            {
                /*
                 * An alert is always 2 bytes. Read the alert.
                 */
                byte[] tmp = new byte[2];
                alertQueue.Read(tmp, 0, 2, 0);
                alertQueue.RemoveData(2);
                short level = tmp[0];
                short description = tmp[1];
                if (level == AL_fatal)
                {
                    /*
                     * This is a fatal error.
                     */
                    this.failedWithError = true;
                    this.closed = true;
                    /*
                     * Now try to close the stream, ignore errors.
                     */
                    try
                    {
                        rs.Close();
                    }
                    catch (Exception)
                    {

                    }
                    throw new IOException(TLS_ERROR_MESSAGE);
                }
                else
                {
                    /*
                     * This is just a warning.
                     */
                    if (description == AP_close_notify)
                    {
                        /*
                         * Close notify
                         */
                        this.FailWithError(AL_warning, AP_close_notify);
                    }
                    /*
                     * If it is just a warning, we continue.
                     */
                }
            }
        }

        private void ProcessChangeCipherSpec()
        {
            while (changeCipherSpecQueue.Size > 0)
            {
                /*
                 * A change cipher spec message is only one byte with the value 1.
                 */
                byte[] b = new byte[1];
                changeCipherSpecQueue.Read(b, 0, 1, 0);
                changeCipherSpecQueue.RemoveData(1);
                if (b[0] != 1)
                {
                    /*
                     * This should never happen.
                     */
                    this.FailWithError(AL_fatal, AP_unexpected_message);
                }

                /*
                 * Check if we are in the correct connection state.
                 */
                if (this.connection_state != CS_CLIENT_FINISHED_SEND)
                {
                    this.FailWithError(AL_fatal, AP_handshake_failure);
                }

                rs.serverClientSpecReceived();

                this.connection_state = CS_SERVER_CHANGE_CIPHER_SPEC_RECEIVED;
            }
        }

        public void Connect(CertificateVerifyer verifyer)
        {
            this.Connect(new DefaultTlsClient(verifyer));
        }

        public void Connect(TlsClient tlsClient)
        {
            if (tlsClient == null)
            {
                throw new ArgumentException("'tlsClient' cannot be null");
            }
            if (this.tlsClient != null)
            {
                throw new InvalidOperationException("connect can only be called once");
            }

            this.tlsClient = tlsClient;
            this.tlsClient.Init(this);

            /*
             * Send Client hello
             *
             * First, generate some random data.
             */
            securityParameters = new SecurityParameters();
            securityParameters.clientRandom = new byte[32];
            random.GetBytes(securityParameters.clientRandom);
            TlsUtils.WriteGMTUnixTime(securityParameters.clientRandom, 0);

            MemoryStream os = new MemoryStream();
            TlsUtils.WriteVersion(os);
            os.Write(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);

            /*
             * Length of Session id
             */
            TlsUtils.WriteUint8((short)0, os);
            //TlsUtils.writeUint8((short)1, os);

            /*
             * Cipher suites
             */
            this.offeredCipherSuites = this.tlsClient.GetCipherSuites();

            // Note: 1 extra slot for TLS_EMPTY_RENEGOTIATION_INFO_SCSV
            TlsUtils.WriteUint16(2 * (offeredCipherSuites.Length/* + 1*/), os);
            for (int i = 0; i < offeredCipherSuites.Length; ++i)
            {
                TlsUtils.WriteUint16(offeredCipherSuites[i], os);
            }

            // RFC 5746 3.3
            // Note: If renegotiation added, remove this (and extra slot above)
            //TlsUtils.writeUint16(TLS_EMPTY_RENEGOTIATION_INFO_SCSV, os);

            /*
             * Compression methods, just the null method.
             */
            byte[] compressionMethods = new byte[] { 0x00 };
            TlsUtils.WriteOpaque8(compressionMethods, os);

            /*
             * Extensions
             */
            // Integer -> byte[]
            IDictionary clientExtensions = this.tlsClient.generateClientExtensions();

            // RFC 5746 3.4
            // Note: If renegotiation is implemented, need to use this instead of TLS_EMPTY_RENEGOTIATION_INFO_SCSV
            //      {
            //          if (clientExtensions == null)
            //          {
            //              clientExtensions = new Hashtable();
            //          }
            //
            //          clientExtensions.put(EXT_RenegotiationInfo, createRenegotiationInfo(emptybuf));
            //      }

            this.extendedClientHello = clientExtensions != null && clientExtensions.Count > 0;

            if (extendedClientHello)
            {
                MemoryStream ext = new MemoryStream();

                foreach (int extType in clientExtensions.Keys)
                {
                    byte[] extValue = (byte[])clientExtensions[extType];

                    TlsUtils.WriteUint16(extType, ext);
                    TlsUtils.WriteOpaque16(extValue, ext);
                }

                TlsUtils.WriteOpaque16(ext.ToArray(), os);
            }

            MemoryStream bos = new MemoryStream();
            TlsUtils.WriteUint8(HP_CLIENT_HELLO, bos);
            TlsUtils.WriteUint24((int)os.Length, bos);
            bos.Write(os.ToArray(), 0, (int)os.Length);
            byte[] message = bos.ToArray();
            rs.WriteMessage(RL_HANDSHAKE, message, 0, message.Length);
            connection_state = CS_CLIENT_HELLO_SEND;

            /*
             * We will now read data, until we have completed the handshake.
            while (connection_state != CS_DONE)
            {
                // TODO Should we send fatal alerts in the event of an exception
                // (see readApplicationData)
                rs.readData();
            }

            this.tlsInputStream = new TlsInputStream(this);
            this.tlsOutputStream = new TlsOutputStream(this);
             */
        }

        public void WriteApplicationData(MemoryStream ms, byte[] b)
        {
            /*
             * We will now read data, until we have completed the handshake.
             */
            rs.SetInputStream(ms);
            rs.SetOutputStream(ms);
            WriteData(b, 0, b.Length);
            this.tlsInputStream = new TlsInputStream(this);
            this.tlsOutputStream = new TlsOutputStream(this);
        }


        public byte[] ReadApplicationData(MemoryStream ms)
        {
            /*
             * We will now read data, until we have completed the handshake.
             */
            rs.SetInputStream(ms);
            rs.SetOutputStream(ms);
            return ReadApplicationData();
        }

        protected byte[] ReadApplicationData()
        {
            while (rs.HasMore())
            {
                /*
                 * We need to read some data.
                 */
                if (this.failedWithError)
                {
                    /*
                     * Something went terribly wrong, we should throw an IOException
                     */
                    throw new IOException(TLS_ERROR_MESSAGE);
                }
                if (this.closed)
                {
                    /*
                     * Connection has been closed, there is no more data to read.
                     */
                    return null;
                }

                try
                {
                    rs.ReadData();
                }
                catch (IOException e)
                {
                    if (!this.closed)
                    {
                        this.FailWithError(AL_fatal, AP_internal_error);
                    }
                    throw e;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    if (!this.closed)
                    {
                        this.FailWithError(AL_fatal, AP_internal_error);
                    }
                    throw e;
                }
            }
            int len = applicationDataQueue.Size;
            byte[] b = new byte[len];
            applicationDataQueue.Read(b, 0, len, 0);
            applicationDataQueue.RemoveData(len);
            return b;
        }


        public short UpdateConnectState(MemoryStream ms)
        {
            rs.SetInputStream(ms);
            rs.SetOutputStream(ms);
            while (connection_state != CS_DONE && rs.HasMore())
            {
                rs.ReadData();
            }
            return connection_state;
        }

        protected internal int ReadApplicationData(byte[] buf, int offset, int len)
        {
            while (applicationDataQueue.Size == 0)
            {
                /*
                 * We need to read some data.
                 */
                if (this.closed)
                {
                    if (this.failedWithError)
                    {
                        /*
                         * Something went terribly wrong, we should throw an IOException
                         */
                        throw new IOException(TLS_ERROR_MESSAGE);
                    }

                    /*
                     * Connection has been closed, there is no more data to read.
                     */
                    return -1;
                }

                try
                {
                    rs.ReadData();
                }
                catch (IOException e)
                {
                    if (!this.closed)
                    {
                        this.FailWithError(AL_fatal, AP_internal_error);
                    }
                    throw e;
                }
                catch (Exception e)
                {
                    if (!this.closed)
                    {
                        this.FailWithError(AL_fatal, AP_internal_error);
                    }
                    throw e;
                }
            }
            len = Math.Min(len, applicationDataQueue.Size);
            applicationDataQueue.Read(buf, offset, len, 0);
            applicationDataQueue.RemoveData(len);
            return len;
        }

        protected internal void WriteData(byte[] buf, int offset, int len)
        {
            if (this.closed)
            {
                if (this.failedWithError)
                {
                    throw new IOException(TLS_ERROR_MESSAGE);
                }

                throw new IOException("Sorry, connection has been closed, you cannot write more data");
            }

            /*
             * Protect against known IV attack!
             *
             * DO NOT REMOVE THIS LINE, EXCEPT YOU KNOW EXACTLY WHAT YOU ARE DOING HERE.
             */
            rs.WriteMessage(RL_APPLICATION_DATA, emptybuf, 0, 0);

            do
            {
                /*
                 * We are only allowed to write fragments up to 2^14 bytes.
                 */
                int toWrite = Math.Min(len, 1 << 14);

                try
                {
                    rs.WriteMessage(RL_APPLICATION_DATA, buf, offset, toWrite);
                }
                catch (IOException e)
                {
                    if (!closed)
                    {
                        this.FailWithError(AL_fatal, AP_internal_error);
                    }
                    throw e;
                }
                catch (Exception e)
                {
                    if (!closed)
                    {
                        this.FailWithError(AL_fatal, AP_internal_error);
                    }
                    throw e;
                }


                offset += toWrite;
                len -= toWrite;
            }
            while (len > 0);

        }

        public Stream GetOutputStream()
        {
            return this.tlsOutputStream;
        }

        public Stream GetInputStream()
        {
            return this.tlsInputStream;
        }

        public void Close()
        {
            if (!closed)
            {
                this.FailWithError((short)1, (short)0);
            }
        }

        public void Flush()
        {
            rs.Flush();
        }

        public void setKeyManagers(X509Certificate2Collection keyManagers)
        {
            this.keyManagers = keyManagers;
        }

        public void setTrustManagers(X509Certificate2Collection trustManagers)
        {
            this.trustManagers = trustManagers;
        }
    }
}
