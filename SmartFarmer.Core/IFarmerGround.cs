using SmartFarmer.Tasks;
using System.Collections.Generic;

namespace SmartFarmer
{
    public interface IFarmerGround
    {
        IReadOnlyList<IFarmerRow> Rows { get; }
        ICollection<IFarmerPlan> Plans { get; }
        IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; }
        double MetersAmongRows { get; }
        double WidthInMeters { get; }
        double LengthInMeters { get; }

        void AddRows(IFarmerRow[] rows);
        void AddRow(IFarmerRow row);
        void DeleteRow(IFarmerRow row);
    }
}
