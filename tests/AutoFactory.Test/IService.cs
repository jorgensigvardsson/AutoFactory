namespace AutoFactory.Test
{
    public interface IService
    {
        string ServerName { get; }
        int X { get; }
        int Y { get; }
        ISubService Subservice { get; }
    }
}