namespace AutoFactory.Test
{
    [AutoFactory]
    public class Service : IService
    {
        public string ServerName { get; }
        public int X { get; }
        public int Y { get; }
        public ISubService Subservice { get; }

        public Service([PerInstance] string serverName, [PerInstance] int x, [PerInstance] int y, ISubService subservice)
        {
            ServerName = serverName;
            X = x;
            Y = y;
            Subservice = subservice;
        }
    }
}
