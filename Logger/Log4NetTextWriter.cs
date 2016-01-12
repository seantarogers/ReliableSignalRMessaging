namespace Logger
{
    using System;
    using System.IO;
    using System.Text;

    public class Log4NetTextWriter : TextWriter
    {
        private readonly IMessagingLogger messagingLogger;
        private readonly StringBuilder stringBuilder;

        public Log4NetTextWriter(IMessagingLogger messagingLogger)
        {
            this.messagingLogger = messagingLogger;
            stringBuilder = new StringBuilder();
        }

        public override void Write(char value)
        {
            switch (value)
            {
                case '\n':
                    return;
                case '\r':
                    messagingLogger.DebugFormat(this,
                        stringBuilder.ToString());
                    stringBuilder.Clear();
                    return;
                default:
                    stringBuilder.Append(value);
                    break;
            }
        }

        public override void Write(string value)
        {
            messagingLogger.DebugFormat(this, value);
        }

        public override Encoding Encoding
        {
            //this is never used by us so no need to implement.
            get { throw new NotImplementedException(); }
        }
    }
}