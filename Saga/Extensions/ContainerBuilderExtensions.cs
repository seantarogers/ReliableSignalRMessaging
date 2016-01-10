namespace Saga.Extensions
{
    using System.Reflection;

    using Autofac;

    using Saga.Strategies;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => typeof(IBackOfficeStrategy).IsAssignableFrom(t))
                .InstancePerLifetimeScope()
                .AsImplementedInterfaces();
            return containerBuilder;
        }
    }
}