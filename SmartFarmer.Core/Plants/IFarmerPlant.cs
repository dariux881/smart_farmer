﻿
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Utils;

namespace SmartFarmer.Plants;

public interface IFarmerPlant : IFarmerService
{
    string BotanicalName { get; }
    string FriendlyName { get; }

    FarmerIrrigationTaskInfo IrrigationTaskInfo { get; }

    /// <summary>
    /// values in cells, where cells size depend on garden
    /// </summary>
    int PlantWidth { get; }
    int PlantDepth { get; }

    /// <summary>
    /// 1-12 value
    /// </summary>
    int MonthToPlan { get; }

    int NumberOfWeeksToHarvest { get; }
}