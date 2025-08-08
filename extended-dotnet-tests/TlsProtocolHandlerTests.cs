using NUnit.Framework;
using JRadius.Extended.Tls;

namespace JRadius.Extended.Tests
{
    public class TlsProtocolHandlerTests
    {
        [Test]
        public void TestTlsProtocolHandlerInstantiation()
        {
            TlsProtocolHandler handler = new TlsProtocolHandler();
            Assert.IsNotNull(handler);
        }
    }
}
