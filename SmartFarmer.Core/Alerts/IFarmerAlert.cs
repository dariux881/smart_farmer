using System;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Plants;

namespace SmartFarmer.Alerts
{
    public interface IFarmerAlert
    {
        string ID { get; }
        DateTime When { get; }
        IFarmerTask RaisedBy { get; }
        IFarmerPlantInstance Plant { get; }
        string Code { get; }
        string Message { get; }
        AlertLevel Level { get; }
        AlertSeverity Severity { get; }
        bool MarkedAsRead { get; set; }
    }
}