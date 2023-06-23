using SmartFarmer.Data.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.MockedTasks
{
    public class BaseFarmerPlan : FarmerPlan
    {
        public BaseFarmerPlan(string id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}