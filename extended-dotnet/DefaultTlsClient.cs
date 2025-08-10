using System;
using System.Collections;
using System.Collections.Generic;

namespace JRadius.Extended.Tls
{
    public class DefaultTlsClient : TlsClient
    {
        private ICertificateVerifyer verifyer;

        public DefaultTlsClient(ICertificateVerifyer verifyer)
        {
            this.verifyer = verifyer;
        }

        public void Init(TlsProtocolHandler handler)
        {
            // TODO: Implement this
        }

        public int[] GetCipherSuites()
        {
            // TODO: Implement this
            return new int[0];
        }

        public void NotifySessionID(byte[] sessionID)
        {
            // TODO: Implement this
        }

        public void NotifySelectedCipherSuite(int cipherSuite)
        {
            // TODO: Implement this
        }

        public void ProcessServerExtensions(IDictionary serverExtensions)
        {
            // TODO: Implement this
        }

        public TlsKeyExchange CreateKeyExchange()
        {
            // TODO: Implement this
            return null;
        }

        public Certificate GetCertificate()
        {
            // TODO: Implement this
            return null;
        }

        public byte[] GenerateCertificateSignature(byte[] currentHash)
        {
            // TODO: Implement this
            return null;
        }

        public void ProcessServerCertificateRequest(byte[] types, IList authorityDNs)
        {
            // TODO: Implement this
        }

        public TlsCipher createCipher(SecurityParameters securityParameters)
        {
            // TODO: Implement this
            return null;
        }

        public IDictionary generateClientExtensions()
        {
            // TODO: Implement this
            return null;
        }
    }
}
