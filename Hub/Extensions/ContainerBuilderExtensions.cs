namespace Hub.Extensions
{
    using Autofac;

    using Managers.PFIntRemotePublisher.Application.Event.Managers;

    using Hubs;

    using Logger;

    using Managers;

    using MessagingInfrastructure.Services;

    using Persistence;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<BackOfficeHub>().ExternallyOwned();
            containerBuilder.RegisterType<BrokerConnectionManager>().As<IBrokerConnectionManager>().SingleInstance(); ;
            containerBuilder.RegisterType<AuditContext>().As<IAuditContext>(); //not a singleton, uses a lifetime scope to dispose in the hub.
            containerBuilder.RegisterType<JsonSerializer>().As<IJsonSerializer>().SingleInstance();
            containerBuilder.RegisterType<MessagingLogger>().As<IMessagingLogger>().SingleInstance();

            return containerBuilder;
        }
    }
}