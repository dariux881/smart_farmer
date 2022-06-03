using System.Collections.Generic;
using SmartFarmer.Plants;

namespace SmartFarmer
{
    public interface IFarmerRow
    {
        IDictionary<IFarmerPlantInstance, double> PlantsInRow { get; }

    }
}
