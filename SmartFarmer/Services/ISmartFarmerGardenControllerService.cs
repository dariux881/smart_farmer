using SmartFarmer.Movement;

namespace SmartFarmer.Services;

public interface ISmartFarmerGardenControllerService :
    ISmartFarmerReadGardenControllerService,
    ISmartFarmerEditGardenControllerService
{
}