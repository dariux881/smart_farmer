using SmartFarmer.Plants;

namespace SmartFarmer.Data;

public interface ISmartFarmerRepository : 
    ISmartFarmerGroundManagementRepository, 
    ISmartFarmerPlanManagementRepository,
    ISmartFarmerAlertManagementRepository,
    ISmartFarmerSecurityRepository
{
}
