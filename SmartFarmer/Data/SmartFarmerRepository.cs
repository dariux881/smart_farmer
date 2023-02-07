using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmer.DTOs.Security;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

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

    public async Task<bool> IsUserAuthorizedTo(string userId, string authorizationId)
    {
        return await IsUserAuthorizedToAnyOf(userId, new [] { authorizationId });
    }

    public async Task<bool> IsUserAuthorizedToAnyOf(string userId, string[] authorizationIds)
    {
        var now = DateTime.UtcNow;

        return await 
            _dbContext
                .Users
                    .Where(x => x.ID == userId) // filter by userId
                    .Include(x => x.Authorizations)
                    .SelectMany(x => x.Authorizations)
                    .AnyAsync(auth => 
                        authorizationIds.Contains(auth.ID) &&
                        (auth.StartAuthorizationDt == null || auth.StartAuthorizationDt >= now) && 
                        (auth.EndAuthorizationDt == null || auth.EndAuthorizationDt <= now)
                    );
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
                .Include(g => g.Plans)
                .FirstOrDefaultAsync(
                    x => x.ID == groundId && x.UserID == userId);
    }

#endregion

    public async Task<IFarmerPlantInstance> GetFarmerPlantInstanceById(string id, string userId = null)
    {
        return (await GetFarmerPlantsInstanceById(new [] {id}, userId))?.FirstOrDefault();
    }

    public async Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantsInstanceById(string[] ids, string userId = null)
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
                .Where(p => ids.Contains(p.ID))
                .Where(p => !grounds.Any() || grounds.Contains(p.FarmerGroundId))
                .Include(p => p.Plant)
                .ToArrayAsync();
    }

    
    public async Task<IFarmerPlant> GetFarmerPlantById(string id)
    {
        return (await GetFarmerPlantsById(new [] {id}))?.FirstOrDefault();
    }

    public async Task<IEnumerable<IFarmerPlant>> GetFarmerPlantsById(string[] ids)
    {
        return await _dbContext
            .Plants
                .Where(p => ids.Contains(p.ID))
                .ToArrayAsync();
    }

    public async Task<IFarmerPlan> GetFarmerPlanByIdAsync(string id, string userId)
    {
        return (await GetFarmerPlanByIdsAsync(new [] {id}, userId))?.FirstOrDefault();
    }

    public async Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsAsync(string[] ids, string userId)
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
            .Plans
                .Include(x => x.Steps)
                .Where(p => ids.Contains(p.ID))
                .Where(p => !grounds.Any() || grounds.Contains(p.GroundId))
                .Include(p => p.Steps)
                .ToArrayAsync();
    }

    public async Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids)
    {
        
        return await _dbContext
            .PlanSteps
                .Where(p => ids.Contains(p.ID))
                .ToArrayAsync();
    }
}
