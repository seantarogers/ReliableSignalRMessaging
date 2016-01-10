namespace IdentityProvider
{
    using Topshelf;

    public class Program
    {
        static void Main(string[] args)
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

                  const string Identityprovider = "IdentityProvider";
                  x.SetDescription(Identityprovider);
                  x.SetDisplayName(Identityprovider);
                  x.SetServiceName(Identityprovider);
              });
        }
    }
}
