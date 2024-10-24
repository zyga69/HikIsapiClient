namespace HikIsapi.Client
{
    public interface IIsapi
    {
        IIsapiAuthenticated Login(bool force = false);

        bool IsAvailable();
    }

    public interface IIsapiAuthenticated
    {
        IIsapiAuthenticated DoorOpen(int doorNo);
        IIsapiAuthenticated SessionHeartBeat();

        int HeartBeatCount { get; }

        IIsapi LogOut();
    }
}