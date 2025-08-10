using JRadius.Core;
using JRadius.Core.Handler;
using JRadius.Core.Server;
using JRadius.Core.Session;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;

namespace JRadius.Server
{
    public abstract class RadiusProcessor : Processor
    {
        protected RadiusProcessor(ILogger<Processor> logger) : base(logger)
        {
        }

        protected abstract void LogReturnCode(int result, IJRCommand handler);

        protected int HandleRadiusException(JRadiusRequest request, System.Exception e)
        {
            var session = request.GetSession();
            var error = e.Message;
            _logger.LogWarning(error);

            if (session != null)
            {
                try
                {
                    // TODO: session.getLogEntry(request).addMessage(error);
                }
                catch (System.Exception re)
                {
                    _logger.LogError(re, re.Message);
                }
            }

            return e is System.Security.SecurityException ? JRadiusServer.RLM_MODULE_REJECT : JRadiusServer.RLM_MODULE_FAIL;
        }

        protected int RunPacketHandlers(JRadiusRequest request)
        {
            var handlers = RequestHandlers;
            int result = JRadiusServer.RLM_MODULE_NOOP;
            bool exceptionThrown = false;

            if (handlers == null)
            {
                return result;
            }

            var sessionManager = JRadiusSessionManager.GetManager(request.GetSender());
            var session = request.GetSession();

            if (sessionManager != null)
            {
                if (session == null)
                {
                    try
                    {
                        session = sessionManager.GetSession(request);
                    }
                    catch (System.Exception e)
                    {
                        _logger.LogError(e, e.Message);
                        return JRadiusServer.RLM_MODULE_REJECT;
                    }
                }

                if (session == null)
                {
                    _logger.LogError("Unable to create session");
                    return JRadiusServer.RLM_MODULE_REJECT;
                }

                request.SetSession(session);
                // TODO: sessionManager.lock(session);
            }

            try
            {
                foreach (var handler in handlers)
                {
                    bool stop = false;
                    try
                    {
                        if (handler.DoesHandle(request))
                        {
                            stop = handler.Execute(request);
                            result = request.GetReturnValue();
                            LogReturnCode(result, handler);
                            if (stop) break;
                        }
                    }
                    catch (System.Exception e)
                    {
                        exceptionThrown = true;
                        result = HandleRadiusException(request, e);
                        LogReturnCode(result, handler);
                        break;
                    }
                }

                if (session != null && !exceptionThrown)
                {
                    try
                    {
                        // TODO: session.onPostProcessing(request);
                    }
                    catch (System.Exception e)
                    {
                        result = HandleRadiusException(request, e);
                    }
                }

                if (result == JRadiusServer.RLM_MODULE_REJECT && request.IsAccountingRequest())
                {
                    _logger.LogDebug("Ack'ing AccountingRequest that was rejected");
                    result = JRadiusServer.RLM_MODULE_OK;
                }

                if (session != null) // TODO: && session.isLogging())
                {
                    // TODO: HandlerLogEvent log = new HandlerLogEvent(request, request.getSessionKey(), result);
                    // EventDispatcher.post(log);
                }
            }
            finally
            {
                if (session != null)
                {
                    // TODO: sessionManager.unlock(session, true);
                }
            }

            return result;
        }

        protected override void ProcessRequests(CancellationToken cancellationToken)
        {
            // This method should be implemented by a concrete class
            throw new System.NotImplementedException();
        }
    }
}
