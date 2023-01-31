using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmer.DTOs.Security;
using SmartFarmer.Plants;

namespace SmartFarmer.Data;

public abstract class SmartFarmerRepository : ISmartFarmerRepository
{
    protected SmartFarmerDbContext _dbContext;

    public SmartFarmerRepository(SmartFarmerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

#region Security
    public async Task<UserLogin> GetLoggedUserByToken(string token)
    {
        return await 
            _dbContext
                .Logins
                    .FirstOrDefaultAsync(x => x.Token == token && x.LogoutDt == null);
    }

    public async Task<UserLogin> GetLoggedUserById(string userId)
    {
        return await _dbContext
                .Logins
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.LogoutDt == null);
    }

    public async Task<User> GetUser(string userName, string password)
    {
        return await 
            _dbContext
                .Users
                    .SingleAsync(x => x.UserName == userName && x.Password == password);
    }

    public async Task LogInUser(UserLogin userLogin)
    {
        _dbContext.Logins.Add(userLogin);
        await _dbContext.SaveChangesAsync();
    }

    public async Task LogOutUser(string token)
    {
        var checkLoggedUser = await GetLoggedUserByToken(token);

        if (checkLoggedUser == null)
            return;

        checkLoggedUser.LogoutDt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
#endregion

#region Ground Management
    public async Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId)
    {
        return await _dbContext.Grounds.Where(x => x.UserID == userId).ToListAsync();
    }

    public async Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId)
    {
        return await _dbContext
            .Grounds
                .Include(g => g.Plants)
                .FirstOrDefaultAsync(
                    x => x.ID == groundId && x.UserID == userId);
    }

#endregion

    public async Task<IFarmerPlantInstance> GetPlantById(string id, string userId = null)
    {
        var grounds = new List<string>();
        if (string.IsNullOrEmpty(userId))
        {
            grounds = await _dbContext
                .Grounds
                    .Where(x => x.UserID == userId)
                    .Select(g => g.ID)
                    .ToListAsync();

            if (!grounds.Any())
            {
                throw new InvalidOperationException("no grounds found for user "+ userId);
            }
        }

        return await _dbContext
            .PlantsInstance
                .Where(p => p.ID == id)
                .Where(p => !grounds.Any() || grounds.Contains(p.FarmerGroundId))
                .Include(p => p.Plant)
                .SingleAsync();
    }
}
