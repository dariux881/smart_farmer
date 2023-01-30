using SmartFarmer.Plants;

namespace SmartFarmer.Data;

public interface ISmartFarmerRepository : 
    ISmartFarmerGroundManagementRepository, 
    ISmartFarmerSecurityRepository
{
}
