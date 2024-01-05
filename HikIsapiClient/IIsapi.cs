using HikIsapi.Client.Model;

namespace HikIsapi.Client
{
    public interface IIsapi : IDisposable
    {

        ResponseStatus DoorOpen(int doorNo);
    }
}