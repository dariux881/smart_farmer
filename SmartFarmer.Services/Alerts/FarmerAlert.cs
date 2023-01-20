using System;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Plants;

namespace SmartFarmer.Alerts
{
    public class FarmerAlert : IFarmerAlert
    {
        public string ID { get; set; }
        public DateTime When { get; set; }
        public string RaisedByTaskId { get; set; }
        public string PlantInstanceId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public AlertLevel Level { get; set; }
        public AlertSeverity Severity { get; set; }
        public bool MarkedAsRead { get; set; }
    }
}