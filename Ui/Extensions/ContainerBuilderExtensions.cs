namespace Ui.Extensions
{
    using System.Web.Mvc;

    using Autofac;
    using Autofac.Integration.Mvc;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterComponents(this ContainerBuilder containerBuilder)
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            return containerBuilder;
        }
    }
}