using System;

namespace SmartFarmer.Handlers;

public class FarmerAppCommunicationHandler : IFarmerAppCommunicationHandler
{
    public event EventHandler NewLoggedUser;
    public event EventHandler<GardenChangedEventArgs> LocalGardenAdded;
    public event EventHandler<GardenChangedEventArgs> LocalGardenRemoved;

    public void NotifyNewGarden(string gardenId)
    {
        LocalGardenAdded?.Invoke(this, new GardenChangedEventArgs(gardenId));
    }

    public void NotifyRemovedGarden(string gardenId)
    {
        LocalGardenRemoved?.Invoke(this, new GardenChangedEventArgs(gardenId));
    }

    public void NotifyNewLoggedUser()
    {
        NewLoggedUser?.Invoke(this, EventArgs.Empty);
    }
}