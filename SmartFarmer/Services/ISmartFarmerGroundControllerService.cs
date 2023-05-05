using System.Threading.Tasks;
using SmartFarmer.Movement;

namespace SmartFarmer.Services;

public interface ISmartFarmerGroundControllerService :
    ISmartFarmerReadGroundControllerService,
    ISmartFarmerEditGroundControllerService
{
}