
using System;

namespace SmartFarmer.Alerts
{
    public class FarmerAlertHandlerEventArgs : EventArgs
    {
        public IFarmerAlert Alert { get; }

        public FarmerAlertHandlerEventArgs(IFarmerAlert alert)
        {
            Alert = alert;
        }
    }
}