
using SmartFarmer.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SmartFarmer
{
    public class FarmerGround : IFarmerGround
    {
        private List<IFarmerRow> _rows;

        public FarmerGround()
        {
            _rows = new List<IFarmerRow>();
            Plans = new List<IFarmerPlan>();

            MetersAmongRows = 1.0;
        }

        public IReadOnlyList<IFarmerRow> Rows => _rows.AsReadOnly();
        public ICollection<IFarmerPlan> Plans { get; private set; }
        public IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; private set; }

        public double MetersAmongRows { get; set; }
        public double WidthInMeters => Rows.Count * MetersAmongRows;

        public double LengthInMeters =>
            Rows.Max(x => x.PlantsInRow.Select(y => y.Value).Max());

        public void AddRows(IFarmerRow[] rows)
        {
            foreach (var row in rows)
            {
                _rows.Add(row);
            }
            BuildAutoGroundIrrigationPlan();
        }

        public void AddRow(IFarmerRow row)
        {
            _rows.Add(row);
            BuildAutoGroundIrrigationPlan();
        }

        public void DeleteRow(IFarmerRow row)
        {
            _rows.Remove(row);
            BuildAutoGroundIrrigationPlan();
        }

        private void BuildAutoGroundIrrigationPlan()
        {
            var plantIrrigationInfo = 
                _rows
                    .SelectMany(x => x.PlantsInRow);

            //TODO build plan considering plant position
        }
    }
}
