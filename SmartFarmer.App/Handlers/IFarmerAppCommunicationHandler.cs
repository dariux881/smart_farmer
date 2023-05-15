using System;

namespace SmartFarmer.Handlers;

public interface IFarmerAppCommunicationHandler
{
    event EventHandler NewLoggedUser;
    event EventHandler<GroundChangedEventArgs> LocalGroundAdded;
    event EventHandler<GroundChangedEventArgs> LocalGroundRemoved;

    void NotifyNewGround(string groundId);
    void NotifyRemovedGround(string groundId);
    void NotifyNewLoggedUser();
}
