namespace SmartFarmer.Handlers;

public class GardenChangedEventArgs 
{
    public string GardenId { get; }

    public GardenChangedEventArgs (string gardenId)
    {
        GardenId = gardenId;
    }
}
