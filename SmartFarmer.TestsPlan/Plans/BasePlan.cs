using SmartFarmer.Tasks;

namespace SmartFarmer.Tests;

public abstract class BasePlan : FarmerPlan
{
    public BasePlan(string id, string name)
        : base(id, name)
    {
        
    }
}
