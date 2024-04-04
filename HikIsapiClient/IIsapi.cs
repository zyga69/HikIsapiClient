namespace HikIsapi.Client
{
    public interface IIsapi : IDisposable
    {
        bool DoorOpen(int doorNo);
    }
}