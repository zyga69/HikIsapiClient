namespace HikIsapi.Client
{
    public interface IIsapi
    {
        /// <summary>
        /// Login to Hik Device - without checking current state
        /// </summary>
        /// <returns></returns>
        IIsapiAuthenticated Login();

        /// <summary>
        /// Will login to hik if state indicates that it is not logged in
        /// </summary>
        /// <returns></returns>
        IIsapiAuthenticated EnsureLoggedIn();

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