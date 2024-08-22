namespace HikIsapi.Client
{
    public interface IIsapi
    {
        IIsapiAuthenticated Login();

        bool IsAvailable();
    }

    public interface IIsapiAuthenticated
    {
        IIsapiAuthenticated DoorOpen(int doorNo);

        IIsapi LogOut();
    }
}