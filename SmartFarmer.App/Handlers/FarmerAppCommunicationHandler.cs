using System;

namespace SmartFarmer.Handlers;

public class GroundChangedEventArgs 
{
    public string GroundId { get; }

    public GroundChangedEventArgs (string groundId)
    {
        GroundId = groundId;
    }
}

public interface IFarmerAppCommunicationHandler
{
    event EventHandler NewLoggedUser;
    event EventHandler<GroundChangedEventArgs> LocalGroundAdded;
    event EventHandler<GroundChangedEventArgs> LocalGroundRemoved;

    void NotifyNewGround(string groundId);
    void NotifyRemovedGround(string groundId);
    void NotifyNewLoggedUser();
}

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