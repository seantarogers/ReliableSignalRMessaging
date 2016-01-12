namespace Logger
{
    public interface IMessagingLogger
    {
        void ErrorFormat(object source, string errorMessage, params object[] parameters);

        void DebugFormat(object source, string debugMessage, params object[] parameters);

        void InfoFormat(object source, string infoMessage, params object[] parameters);
    }
}