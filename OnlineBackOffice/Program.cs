namespace OnlineBackOffice
{
    using Topshelf;

    public class Program
    {
        public static void Main(string[] args)
        {
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

                        var onlinebackoffice = "OnlineBackOffice";
                        x.SetDescription(onlinebackoffice);
                        x.SetDisplayName(onlinebackoffice);
                        x.SetServiceName(onlinebackoffice);
                    });
        }
    }
}
