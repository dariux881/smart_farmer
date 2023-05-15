using System;
using System.Collections.Generic;

namespace SmartFarmer.Handlers;

public class NewAlertStatusEventArgs : EventArgs
{
    public string AlertId { get; }
    public bool Status { get; }

    public NewAlertStatusEventArgs(string alertId, bool status)
    {
        AlertId = alertId;
        Status = status;
    }
}