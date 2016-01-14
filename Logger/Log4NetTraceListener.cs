namespace Logger
{
    using System.Diagnostics;

    using log4net;
    using log4net.Core;

    public class Log4NetTraceListener : TraceListener
    {
        private static ILog log;
        private readonly object thisLock = new object();

        public override void Write(string message)
        {
            InfoFormat(this, message);
        }

        public override void WriteLine(string message)
        {
            InfoFormat(this, message);
        }

        private void InfoFormat(object source, string infoMessage)
        {
            SetUpLog(source);
            if (log.IsInfoEnabled)
            {
                log.Logger.Log(source.GetType(), Level.Info, infoMessage, null);
            }
        }

        private void SetUpLog(object source)
        {
            lock (thisLock)
            {
                if (log == null)
                {
                    log = LogManager.GetLogger(source.GetType());
                }
            }
        }
    }
}