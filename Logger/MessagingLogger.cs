namespace Logger
{
    using log4net;
    using log4net.Core;

    public class MessagingLogger : IMessagingLogger
    {
        private ILog log;

        public void ErrorFormat(object source, string errorMessage, params object[] parameters)
        {
            SetUpLog(source);
            Log(source, errorMessage, Level.Error, parameters);
        }

        public void DebugFormat(object source, string debugMessage, params object[] parameters)
        {
            SetUpLog(source);
            if (log.IsDebugEnabled)
            {
                Log(source, debugMessage, Level.Debug, parameters);
            }
        }

        public void InfoFormat(object source, string infoMessage, params object[] parameters)
        {
            SetUpLog(source);
            if (log.IsInfoEnabled)
            {
                Log(source, infoMessage, Level.Info, parameters);
            }
        }

        private void SetUpLog(object source)
        {
            log = LogManager.GetLogger(source.GetType());
        }

        private void Log(object source, string logMessage, Level level, object[] logParameters)
        {
            log.Logger.Log(source.GetType(), level, logMessage, null);
        }
    }
}