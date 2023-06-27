using System;

namespace SmartFarmer.Services.Alert;

public class NewAlertEventArgs : EventArgs
{
    public string AlertId { get; }
    public string GardenId { get; }

    public NewAlertEventArgs(string gardenId, string alertId)
    {
        GardenId = gardenId;
        AlertId = alertId;
    }
}

public class NewAlertStatusEventArgs : EventArgs
{
    public string AlertId { get; }
    public bool AlertRead { get; }
    public string GardenId { get; }

    public NewAlertStatusEventArgs(string gardenId, string alertId, bool alertRead)
    {
        GardenId = gardenId;
        AlertId = alertId;
        AlertRead = alertRead;
    }
}