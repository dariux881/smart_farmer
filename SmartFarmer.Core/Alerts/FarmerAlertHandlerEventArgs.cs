
using System;

namespace SmartFarmer.Alerts
{
    public class FarmerAlertHandlerEventArgs : EventArgs
    {
        public string AlertId { get; }

        public FarmerAlertHandlerEventArgs(string alertId)
        {
            AlertId = alertId;
        }
    }
}