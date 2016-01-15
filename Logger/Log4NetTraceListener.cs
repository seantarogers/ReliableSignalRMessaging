namespace Logger
{
    using System;
    using System.Diagnostics;

    using log4net;
    using log4net.Core;

    public class Log4NetTraceListener : TraceListener
    {
        private static ILog log;
        private static readonly object ThisLock = new object();

        public override void Write(string message)
        {
            try
            {
                InfoFormat(this, message);
            }
            catch (Exception)
            {
                // ignored - if there is a problem with the trace being written out I do not want to fault the host
            }
        }

        public override void WriteLine(string message)
        {
            try
            {
                InfoFormat(this, message);
            }
            catch (Exception)
            {
                // ignored - if there is a problem with the trace being written out I do not want to fault the host
            }
        }

        private static void InfoFormat(object source, string infoMessage)
        {
            SetUpLog(source);
            if (log.IsInfoEnabled)
            {
                log.Logger.Log(source.GetType(), Level.Info, infoMessage, null);
            }
        }

        private static void SetUpLog(object source)
        {
            lock (ThisLock)
            {
                if (log == null)
                {
                    log = LogManager.GetLogger(source.GetType());
                }
            }
        }
    }
}