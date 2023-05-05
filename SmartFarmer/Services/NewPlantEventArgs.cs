using System;

namespace SmartFarmer.Services;

public class NewPlantEventArgs : EventArgs
{
    public string PlantInstanceId { get; }
    public string FarmerGroundId { get; }

    public NewPlantEventArgs(string groundId, string plantInstanceId)
    {
        FarmerGroundId = groundId;
        PlantInstanceId = plantInstanceId;
    }
}