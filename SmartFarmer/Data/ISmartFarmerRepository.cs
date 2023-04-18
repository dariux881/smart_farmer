
using System.Threading.Tasks;

namespace SmartFarmer.Data;

public interface ISmartFarmerRepository : 
    ISmartFarmerGroundManagementRepository, 
    ISmartFarmerPlanManagementRepository,
    ISmartFarmerAlertManagementRepository,
    ISmartFarmerSecurityRepository
{
    /// <summary>
    /// Saves update on the repository.
    /// </summary>
    /// <returns><c>true</c> if some entry changes, <c>false</c> otherwise.</returns>
    Task<bool> SaveGroundUpdates();
}
