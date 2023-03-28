using System;
using SmartFarmer.Plants;

namespace SmartFarmer.Data.Plants;

public class IrrigationHistoryStep : IIrrigationHistoryStep
{
    public string ID  { get; set; }
    public DateTime IrrigationDt { get; set; }
    public double? Amount { get; set; }
    public string PlantInstanceId { get; set; }
}