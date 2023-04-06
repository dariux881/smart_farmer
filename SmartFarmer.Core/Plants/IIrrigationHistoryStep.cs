using System;
using SmartFarmer.Utils;

namespace SmartFarmer.Plants;

public interface IIrrigationHistoryStep : IFarmerService
{
    string PlantInstanceId { get; }
    DateTime IrrigationDt { get; }
    double? Amount { get; }
}
