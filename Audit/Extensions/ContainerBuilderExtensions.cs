namespace Audit.Extensions
{
    using System.Reflection;

    using Services;

    using Factories;
    
    using Autofac;
    using Autofac.Integration.WebApi;

    using MessagingInfrastructure.Services;

    using Persistence;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<AuditContext>().As<IAuditContext>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<JsonSerializer>().As<IJsonSerializer>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MessageLogFactory>().As<IMessageLogFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<CompletionService>().As<ICompletionService>().InstancePerLifetimeScope();

            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            return containerBuilder;
        }
    }
}