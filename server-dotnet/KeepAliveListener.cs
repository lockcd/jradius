using JRadius.Core.Server;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace JRadius.Server
{
    public class KeepAliveListener
    {
        private Socket _socket;
        private readonly TCPListener _listener;
        private readonly BlockingCollection<ListenerRequest> _queue;
        private readonly BufferedStream _bin;
        private readonly BufferedStream _bout;
        private readonly Thread _thread;

        public KeepAliveListener(Socket socket, TCPListener listener, BlockingCollection<ListenerRequest> queue)
        {
            _socket = socket;
            _bin = new BufferedStream(new NetworkStream(socket), 4096);
            _bout = new BufferedStream(new NetworkStream(socket), 4096);
            _listener = listener;
            _queue = queue;
            _thread = new Thread(Run);
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Run()
        {
            // TODO: Log start
            try
            {
                while (true)
                {
                    // TODO: Implement object pooling
                    var lr = new TCPListenerRequest();
                    lr.Accept(_socket, _bin, _bout, _listener, true, true);

                    if (lr == null || lr.GetEvent() == null)
                    {
                        // TODO: Log shutdown
                        break;
                    }

                    while (true)
                    {
                        try
                        {
                            _queue.Add(lr);
                            break;
                        }
                        catch (ThreadInterruptedException)
                        {
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // TODO: Log exception
            }

            Shutdown(false);
            _listener.DeadKeepAliveListener(this);
        }

        public void Shutdown(bool tryToInterrupt)
        {
            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Receive);
                }
                catch (System.Exception) { }

                try
                {
                    _socket.Shutdown(SocketShutdown.Send);
                }
                catch (System.Exception) { }

                try
                {
                    _socket.Close();
                }
                catch (System.Exception) { }

                _socket = null;

                if (tryToInterrupt)
                {
                    try
                    {
                        _thread.Interrupt();
                    }
                    catch (System.Exception) { }
                }
            }
        }
    }
}
