﻿
using System.Collections.Generic;
using SmartFarmer.Plants;

namespace SmartFarmer
{
    public class FarmerRow : IFarmerRow
    {
        public FarmerRow()
        {
            PlantsInRow = new Dictionary<IFarmerPlantInstance, double>();
        }

        public IDictionary<IFarmerPlantInstance, double> PlantsInRow { get; private set; }
    }
}
