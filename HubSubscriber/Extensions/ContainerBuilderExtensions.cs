namespace HubSubscriber.Extensions
{
    using Autofac;

    using Logger;

    using Managers;
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

            containerBuilder.RegisterType<MessageStoreService>()
                .As<IMessageStoreService>();

            containerBuilder.RegisterType<BackOfficeService>()
                .As<IBackOfficeService>();

            containerBuilder.RegisterType<MessagingLogger>()
                .As<IMessagingLogger>();

            return containerBuilder;
        }
    }
}