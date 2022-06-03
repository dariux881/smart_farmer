using System.Collections.Generic;

namespace SmartFarmer
{
    public interface IFarmerRow
    {
        IDictionary<IFarmerPlantInstance, double> PlantsInRow { get; }

    }
}
