using System.Threading;

namespace JRadius.Server
{
    public class JRadiusThread
    {
        private static int _threadCount = 0;
        private static readonly object _lock = new object();

        private static int GetThreadNumber()
        {
            lock (_lock)
            {
                return ++_threadCount;
            }
        }

        private Thread _thread;

        public JRadiusThread(ThreadStart start)
        {
            _thread = new Thread(start);
            _thread.Name = $"{GetType().FullName}({GetThreadNumber()})";
        }

        public void Start()
        {
            _thread.Start();
        }
    }
}
