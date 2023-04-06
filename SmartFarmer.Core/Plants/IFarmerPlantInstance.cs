﻿using System;
using SmartFarmer.Utils;

namespace SmartFarmer.Plants;

public interface IFarmerPlantInstance : IFarmerService
{
    string PlantName { get; }
    string PlantKindID { get; }
    int PlantX { get; }
    int PlantY { get; }
    int PlantWidth { get; }
    int PlantDepth { get; }
    DateTime PlantedWhen { get; }
    DateTime? LastIrrigation { get; }
}