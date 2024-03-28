using HikIsapi.Client.Model.Response;

namespace HikIsapi.Client
{
    public interface IIsapi : IDisposable
    {
        bool DoorOpen(int doorNo);
    }
}