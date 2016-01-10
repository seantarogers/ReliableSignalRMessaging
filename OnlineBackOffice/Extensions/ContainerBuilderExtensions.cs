namespace OnlineBackOffice.Extensions
{
    using Autofac;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            return containerBuilder;
        }
    }
}