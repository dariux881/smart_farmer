
using System;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Alerts
{
    public class FarmerAlertHandler
    {
        private static readonly Lazy<FarmerAlertHandler> _instance = new(() => new FarmerAlertHandler());

        public static FarmerAlertHandler Instance => _instance.Value;

        private FarmerAlertHandler()
        {
            
        }

        public event EventHandler<FarmerAlertHandlerEventArgs> NewAlertCreated;

        public void RaiseAlert(IFarmerAlert alert)
        {
            NewAlertCreated?.Invoke(this, new FarmerAlertHandlerEventArgs(alert));
        }
        
        public void RaiseAlert(
                IFarmerTask task,
                IFarmerPlantInstance plant,
                string code,
                string message,
                AlertLevel level,
                AlertSeverity severity
            )
        {
            var now = DateTime.UtcNow;

            NewAlertCreated?.Invoke(
                this, 
                new FarmerAlertHandlerEventArgs(
                    new FarmerAlert()
                    {
                        ID = 
                            "Alert_" + 
                            now.ToString("yyyyMMddHHmmss"),
                        RaisedBy = task,
                        When = now,
                        Plant = plant,
                        Code = code,
                        Message = message,
                        Level = level,
                        Severity = severity,
                        MarkedAsRead = false
                    }
                ));
        }
    }
}