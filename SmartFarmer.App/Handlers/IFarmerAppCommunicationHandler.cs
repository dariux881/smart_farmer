using System;

namespace SmartFarmer.Handlers;

public interface IFarmerAppCommunicationHandler
{
    event EventHandler NewLoggedUser;
    event EventHandler<GardenChangedEventArgs> LocalGardenAdded;
    event EventHandler<GardenChangedEventArgs> LocalGardenRemoved;

    void NotifyNewGarden(string gardenId);
    void NotifyRemovedGarden(string gardenId);
    void NotifyNewLoggedUser();
}
