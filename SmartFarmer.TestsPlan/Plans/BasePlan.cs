using SmartFarmer.Tasks;

namespace SmartFarmer.Tests;

public abstract class BasePlan : FarmerPlan
{
    public BasePlan(string name)
        : base(name)
    {
        
    }
}
