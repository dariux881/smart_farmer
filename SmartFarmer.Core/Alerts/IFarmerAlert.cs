using System;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Plants;
using SmartFarmer.Utils;

namespace SmartFarmer.Alerts
{
    public interface IFarmerAlert : IFarmerService
    {
        DateTime When { get; }
        string RaisedByTaskId { get; }
        string PlantInstanceId { get; }
        AlertCode Code { get; }
        string Message { get; }
        AlertLevel Level { get; }
        AlertSeverity Severity { get; }
        bool MarkedAsRead { get; set; }
    }
}