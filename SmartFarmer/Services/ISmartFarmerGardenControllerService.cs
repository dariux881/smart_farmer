using System.Threading.Tasks;
using SmartFarmer.Movement;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services;

public interface ISmartFarmerGardenControllerService :
    ISmartFarmerReadGardenControllerService,
    ISmartFarmerEditGardenControllerService
{
}