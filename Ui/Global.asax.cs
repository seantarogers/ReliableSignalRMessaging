using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Ui
{
    using Autofac;
    using Autofac.Integration.Mvc;

    using Extensions;

    using NServiceBus;

    public class MvcApplication : System.Web.HttpApplication
    {
        private static ISendOnlyBus bus;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            var busConfiguration = new BusConfiguration();
            busConfiguration.Configure(container);
            bus = Bus.CreateSendOnly(busConfiguration);
        }
    }
}
