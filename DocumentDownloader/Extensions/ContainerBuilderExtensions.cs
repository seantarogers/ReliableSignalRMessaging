namespace DocumentDownloader.Extensions
{
    using Autofac;

    using MessagingInfrastructure.Services;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<CompressionService>()
                .As<ICompressionService>();
            return containerBuilder;
        }
    }
}