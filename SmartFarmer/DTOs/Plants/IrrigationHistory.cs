using System.Collections.Generic;

namespace SmartFarmer.DTOs.Plants;

public class IrrigationHistory
{
    public List<IrrigationHistoryStep> Steps { get; set; }
}