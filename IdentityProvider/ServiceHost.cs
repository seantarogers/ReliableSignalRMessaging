namespace IdentityProvider
{
    using System;

    using Microsoft.Owin.Hosting;

    using Topshelf;

    public class ServiceHost : IServiceHost
    {
        private static IDisposable webHost;

        private static HostControl topShelfHostControl;

        public bool Start(HostControl hostControl)
        {
            topShelfHostControl = hostControl;

            var httpLocalhost = "http://localhost:8095";
            webHost = WebApp.Start(httpLocalhost);
            Console.WriteLine("Successfully started the Webapi on port: {0}", httpLocalhost);

            return true;
        }

        public bool Stop()
        {
            if (webHost != null)
            {
                webHost.Dispose();
            }

            return true;
        }
    }
}