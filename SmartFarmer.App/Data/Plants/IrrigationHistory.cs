using System.Collections.Generic;

namespace SmartFarmer.Data.Plants;

public class IrrigationHistory
{
    public List<IrrigationHistoryStep> Steps { get; set; }
}