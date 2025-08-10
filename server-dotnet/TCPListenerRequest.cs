using JRadius.Core.Server;
using System.IO;
using System.Net.Sockets;

namespace JRadius.Server
{
    public class TCPListenerRequest : ListenerRequest
    {
        public void SetBorrowedFromPool(object requestObjectPool)
        {
            throw new System.NotImplementedException();
        }

        public void Accept(Socket socket, BufferedStream bin, BufferedStream bout, TCPListener listener, bool v1, bool v2)
        {
            throw new System.NotImplementedException();
        }
    }
}
