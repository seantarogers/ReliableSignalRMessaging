namespace HubSubscriber.Extensions
{
    using Autofac;

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

            containerBuilder.RegisterType<MessageStore>()
                .As<IMessageStore>();

            return containerBuilder;
        }
    }
}