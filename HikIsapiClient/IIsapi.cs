using HikIsapi.Client.Model.Response;

namespace HikIsapi.Client
{
    public interface IIsapi 
    {
        IIsapiAuthenticated Login();
        
    }

    public interface IIsapiAuthenticated 
    {
        IIsapiAuthenticated DoorOpen(int doorNo);
        IIsapi LogOut();
    }
}