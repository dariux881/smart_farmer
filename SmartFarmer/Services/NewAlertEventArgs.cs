using System;

namespace SmartFarmer.Services;

public class NewAlertEventArgs : EventArgs
{
    public string AlertId { get; }
    public string FarmerGroundId { get; }

    public NewAlertEventArgs(string groundId, string alertId)
    {
        FarmerGroundId = groundId;
        AlertId = alertId;
    }
}

public class NewAlertStatusEventArgs : EventArgs
{
    public string AlertId { get; }
    public bool AlertRead { get; }
    public string FarmerGroundId { get; }

    public NewAlertStatusEventArgs(string groundId, string alertId, bool alertRead)
    {
        FarmerGroundId = groundId;
        AlertId = alertId;
        AlertRead = alertRead;
    }
}