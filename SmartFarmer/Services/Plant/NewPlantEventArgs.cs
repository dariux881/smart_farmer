using System;

namespace SmartFarmer.Services.Plant;

public class NewPlantEventArgs : EventArgs
{
    public string PlantInstanceId { get; }
    public string GardenId { get; }

    public NewPlantEventArgs(string gardenId, string plantInstanceId)
    {
        GardenId = gardenId;
        PlantInstanceId = plantInstanceId;
    }
}