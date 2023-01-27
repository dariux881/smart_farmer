using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmer.Data;

namespace SmartFarmer.Services;

public class SmartFarmerGroundControllerService : ISmartFarmerGroundControllerService 
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerGroundControllerService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId)
    {
        return await _repository.GetFarmerGroundByUserIdAsync(userId);
    }
    
    public async Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId)
    {
        return await _repository.GetFarmerGroundByIdForUserAsync(userId, groundId);
    }
}