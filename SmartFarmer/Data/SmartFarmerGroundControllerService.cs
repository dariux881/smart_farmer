using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmer.Data;

public class SmartFarmerGroundControllerService : ISmartFarmerGroundControllerService 
{
    private readonly SmartFarmerDbContext _db;

    public SmartFarmerGroundControllerService(SmartFarmerDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId)
    {
        return await _db.Grounds.Where(x => x.UserID == userId).ToListAsync();
    }
    
    public async Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId)
    {
        return await _db.Grounds.FirstOrDefaultAsync(x => x.ID == groundId && x.UserID == userId);
    }
}