namespace HubSubscriber.Extensions
{
    using Autofac;

    using Logger;

    using Managers;

    using MessageStore;

    using Services;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<HubConnectionManager>()
                .As<IHubConnectionManager>()
                .SingleInstance();

            containerBuilder.RegisterType<AccessTokenService>()
                .As<IAccessTokenService>()
                .SingleInstance();

            containerBuilder.RegisterType<MessageStore>()
                .As<IMessageStore>();

            containerBuilder.RegisterType<BackOfficeService>()
                .As<IBackOfficeService>();

            containerBuilder.RegisterType<MessagingLogger>()
                .As<IMessagingLogger>();

            return containerBuilder;
        }
    }
}