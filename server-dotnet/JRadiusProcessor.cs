using JRadius.Core;
using JRadius.Core.Handler;
using JRadius.Core.Server;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace JRadius.Server
{
    public class JRadiusProcessor : RadiusProcessor
    {
        public JRadiusProcessor(ILogger<Processor> logger) : base(logger)
        {
        }

        protected override void LogReturnCode(int result, IJRCommand handler)
        {
            _logger.LogDebug($"Handler {handler.Name} returned {result}");
        }

        protected override void ProcessRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var request = RequestQueue.Take(cancellationToken);
                    if (request != null)
                    {
                        var jRequest = (JRadiusRequest)request.GetEvent();
                        var result = RunPacketHandlers(jRequest);
                        // TODO: Do something with the result
                    }
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }
                catch (System.Exception e)
                {
                    _logger.LogError(e, "Error processing request");
                }
            }
        }
    }
}
