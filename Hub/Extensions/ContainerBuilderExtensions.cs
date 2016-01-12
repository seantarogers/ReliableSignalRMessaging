namespace Hub.Extensions
{
    using Autofac;

    using Hub.Managers.PFIntRemotePublisher.Application.Event.Managers;

    using Hubs;

    using Managers;

    using MessagingInfrastructure.Services;

    using Persistence;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<BackOfficeHub>().ExternallyOwned();
            containerBuilder.RegisterType<BrokerConnectionManager>().As<IBrokerConnectionManager>();
            containerBuilder.RegisterType<AuditContext>().As<IAuditContext>();
            containerBuilder.RegisterType<JsonSerializer>().As<IJsonSerializer>();
            
            return containerBuilder;
        }
    }
}