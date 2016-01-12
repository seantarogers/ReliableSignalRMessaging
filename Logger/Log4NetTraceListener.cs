namespace Logger
{
    using System.Diagnostics;

    public class Log4NetTraceListener : TraceListener
    {
        private readonly IMessagingLogger messagingLogger;

        public Log4NetTraceListener()
        {
            messagingLogger = new MessagingLogger();
        }

        public override void Write(string message)
        {
            messagingLogger.InfoFormat(this, message);
        }

        public override void WriteLine(string message)
        {
            messagingLogger.InfoFormat(this, message);
        }
    }
}