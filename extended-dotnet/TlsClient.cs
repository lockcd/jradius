using System;
using System.Collections;
using System.Collections.Generic;

namespace JRadius.Extended.Tls
{
    public interface TlsClient
    {
        void Init(TlsProtocolHandler handler);
        int[] GetCipherSuites();
        void NotifySessionID(byte[] sessionID);
        void NotifySelectedCipherSuite(int cipherSuite);
        void ProcessServerExtensions(IDictionary serverExtensions);
        TlsKeyExchange CreateKeyExchange();
        Certificate GetCertificate();
        byte[] GenerateCertificateSignature(byte[] currentHash);
        void ProcessServerCertificateRequest(byte[] types, IList authorityDNs);
        TlsCipher createCipher(SecurityParameters securityParameters);
        IDictionary generateClientExtensions();
    }
}
