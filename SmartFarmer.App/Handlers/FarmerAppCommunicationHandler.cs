using System;

namespace SmartFarmer.Handlers;

public class FarmerAppCommunicationHandler : IFarmerAppCommunicationHandler
{
    public event EventHandler NewLoggedUser;
    public event EventHandler<GroundChangedEventArgs> LocalGroundAdded;
    public event EventHandler<GroundChangedEventArgs> LocalGroundRemoved;

    public void NotifyNewGround(string groundId)
    {
        LocalGroundAdded?.Invoke(this, new GroundChangedEventArgs(groundId));
    }

    public void NotifyRemovedGround(string groundId)
    {
        LocalGroundRemoved?.Invoke(this, new GroundChangedEventArgs(groundId));
    }

    public void NotifyNewLoggedUser()
    {
        NewLoggedUser?.Invoke(this, EventArgs.Empty);
    }
}