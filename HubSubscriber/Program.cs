namespace HubSubscriber
{
    using System;
    using System.Threading;

    using Topshelf;

    public class Program
    {
        private static void Main(string[] args)
        {
            WaitUntilTheHubHasStarted();
            HostFactory.Run(
                x =>
                    {
                        x.Service<IServiceHost>(
                            s =>
                                {
                                    s.ConstructUsing(pc => new ServiceHost());
                                    s.WhenStarted((pc, hostControl) => pc.Start(hostControl));
                                    s.WhenStopped(pc => pc.Stop());
                                });
                        x.RunAsLocalSystem();

                        x.SetDescription("HubSubscriber");
                        x.SetDisplayName("HubSubscriber");
                        x.SetServiceName("HubSubscriber");
                    });

        }

        private static void WaitUntilTheHubHasStarted()
        {
            Thread.Sleep(new TimeSpan(0, 0, 10));
        }
    }
}
