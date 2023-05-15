namespace SmartFarmer.Handlers;

public class GroundChangedEventArgs 
{
    public string GroundId { get; }

    public GroundChangedEventArgs (string groundId)
    {
        GroundId = groundId;
    }
}
