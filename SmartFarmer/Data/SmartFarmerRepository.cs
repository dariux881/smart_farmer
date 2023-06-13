using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmer.Alerts;
using SmartFarmer.DTOs;
using SmartFarmer.DTOs.Alerts;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Security;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
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
                .Include(g => g.Alerts)
                .FirstOrDefaultAsync(
                    x => x.ID == groundId && x.UserID == userId);
    }

    public async Task<FarmerDevicePosition> SaveDevicePosition(string userId, FarmerDevicePositionRequestData position)
    {
        var positionToStore = new FarmerDevicePosition()
        {
            GroundId = position.GroundId,
            UserId = userId,
            X = position.Position.X,
            Y = position.Position.Y,
            Z = position.Position.Z,
            Alpha = position.Position.Alpha,
            Beta = position.Position.Beta,
            PositionDt = position.PositionDt ?? DateTime.UtcNow
        };

        _dbContext.DevicePositions.Add(positionToStore);

        await _dbContext.SaveChangesAsync();

        return positionToStore;
    }

    public async Task<string[]> SaveDevicePositions(string userId, FarmerDevicePositionsRequestData positions)
    {
        var groundId = positions.GroundId;

        var posToStore = 
            positions
                .Positions
                    .Select(pos => 
                        new FarmerDevicePosition()
                        {
                            GroundId = groundId,
                            UserId = userId,
                            X = pos.X,
                            Y = pos.Y,
                            Z = pos.Z,
                            Alpha = pos.Alpha,
                            Beta = pos.Beta,
                            PositionDt = pos.PositionDt
                        })
                    .ToList();

        _dbContext.DevicePositions.AddRange(posToStore);

        await _dbContext.SaveChangesAsync();

        return posToStore.Select(x => x.ID).ToArray();
    }

    public async Task<IEnumerable<FarmerDevicePosition>> GetDevicePositionHistory(
        string userId, 
        string groundId, 
        string runId)
    {
        return await _dbContext
            .DevicePositions
                .Where(x =>
                    x.UserId == userId &&
                    x.GroundId == groundId && 
                    string.IsNullOrEmpty(runId) || x.RunId == runId)
                .OrderByDescending(x => x.PositionDt)
                .ToListAsync();
    }
    
    /// <summary>
    /// Saves update on the repository.
    /// </summary>
    /// <returns><c>true</c> if some entry changes, <c>false</c> otherwise.</returns>
    public async Task<bool> SaveGroundUpdates()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }

#endregion

    public async Task<IFarmerPlantInstance> GetFarmerPlantInstanceById(string id, string userId = null)
    {
        return (await GetFarmerPlantsInstanceById(new [] {id}, userId))?.FirstOrDefault();
    }

    public async Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantsInstanceById(string[] ids, string userId = null)
    {
        var grounds = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            grounds = (await GetGroundIdsForUser(userId))?.ToList();

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

    public async Task<IrrigationHistory> GetFarmerIrrigationHistoryByPlant(string plantId, string userId = null)
    {
        if (string.IsNullOrEmpty(plantId)) throw new ArgumentNullException(nameof(plantId));

        // check if plant can be read by userId
        if (!string.IsNullOrEmpty(userId))
        {
            var plant = await 
                _dbContext
                    .PlantsInstance
                        .Where(x => x.ID == plantId)
                        .Include(x => x.Ground)
                        .FirstAsync();
            
            if (plant.Ground.UserID != userId)
            {
                throw new InvalidOperationException("user " + userId + " cannot access plant " + plantId);
            }
        }

        var steps = 
            await _dbContext
                .IrrigationHistory
                .Where(x => x.PlantInstanceId == plantId)
                .OrderByDescending(x => x.IrrigationDt)
                .ToListAsync();

        return new IrrigationHistory { Steps = steps };
    }

    public async Task<bool> MarkIrrigationInstance(FarmerPlantIrrigationInstance irrigation, string userId = null)
    {
        if (irrigation == null || string.IsNullOrEmpty(irrigation.PlantId)) throw new ArgumentNullException("invalid irrigation information");

        // check if plant can be read by userId
        if (!string.IsNullOrEmpty(userId))
        {
            var plant = await 
                _dbContext
                    .PlantsInstance
                        .Where(x => x.ID == irrigation.PlantId)
                        .Include(x => x.Ground)
                        .FirstAsync();
            
            if (plant.Ground.UserID != userId)
            {
                throw new InvalidOperationException("user " + userId + " cannot access plant " + irrigation.PlantId);
            }
        }

        var step = new IrrigationHistoryStep()
        {
            PlantInstanceId = irrigation.PlantId,
            IrrigationDt = irrigation.When ?? DateTime.UtcNow,
            Amount = irrigation.AmountInLiters
        };

        _dbContext
            .IrrigationHistory.Add(step);
        
        var result = await _dbContext.SaveChangesAsync();
        return result == 1;
    }

    public async Task<IEnumerable<string>> GetFarmerPlansInGround(string groundId, string userId)
    {
        return await _dbContext
            .Plans
                .Where(p => p.GroundId == groundId)
                .Select(x => x.ID)
                .ToArrayAsync();
    }

    public async Task<IFarmerPlan> GetFarmerPlanByIdAsync(string id, string userId)
    {
        return (await GetFarmerPlanByIdsAsync(new [] {id}, userId))?.FirstOrDefault();
    }

    public async Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsAsync(string[] ids, string userId)
    {
        var grounds = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            grounds = (await GetGroundIdsForUser(userId))?.ToList();

            if (!grounds.Any())
            {
                throw new InvalidOperationException("no grounds found for user "+ userId);
            }
        }

        return await _dbContext
            .Plans
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

    public async Task<bool> DeleteFarmerPlan(FarmerPlan plan)
    {
        _dbContext.PlanSteps.RemoveRange(plan.Steps);
        _dbContext.Plans.Remove(plan);

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<string> SaveFarmerPlan(FarmerPlan plan)
    {
        _dbContext.Plans.Add(plan);
        _dbContext.PlanSteps.AddRange(plan.Steps);

        await _dbContext.SaveChangesAsync();

        return plan.ID;
    }

    public async Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdsAsync(string userId, string[] ids = null)
    {
        var grounds = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            grounds = (await GetGroundIdsForUser(userId))?.ToList();

            if (!grounds.Any())
            {
                throw new InvalidOperationException("no grounds found for user "+ userId);
            }
        }

        return await _dbContext
            .Alerts
                .Where(p => !grounds.Any() || grounds.Contains(p.FarmerGroundId))
                .Where(p => ids == null || ids.Contains(p.ID))
                .Include(t => t.PlantInstance)
                .Include(t => t.RaisedByTask)
                .ToArrayAsync();
    }

    public async Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGroundIdAsync(string userId, string groundId)
    {
        var grounds = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            grounds = (await GetGroundIdsForUser(userId))?.ToList();
        }

        if (!grounds.Any())
        {
            return new IFarmerAlert[] {};
        }

        if (!grounds.Contains(groundId))
        {
            throw new InvalidOperationException("Ground " + groundId + " is not held by user " + userId);
        }

        return await _dbContext
            .Alerts
                .Where(p => p.FarmerGroundId == groundId)
                .ToArrayAsync();
    }
    
    public async Task<string> CreateFarmerAlert(string userId, FarmerAlertRequestData data)
    {
        var grounds = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            grounds = (await GetGroundIdsForUser(userId))?.ToList();
        }

        if (!grounds.Any())
        {
            await Task.CompletedTask;
            return null;
        }

        var groundId = data.FarmerGroundId;
        if (!grounds.Contains(groundId))
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Ground " + groundId + " is not held by user " + userId);
        }
        
        var alert = new FarmerAlert
            {
                FarmerGroundId = groundId,
                RaisedByTaskId = data.RaisedByTaskId,
                PlantInstanceId = data.PlantInstanceId,
                Message = data.Message,
                Level = data.Level,
                Severity = data.Severity,
                Code = data.Code,
                When = DateTime.UtcNow,
                MarkedAsRead = false
            };

        try
        {
            _dbContext.Alerts.Add(alert);

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return null;
        }

        return alert.ID;
    }

    public async Task<bool> MarkFarmerAlertAsReadAsync(string userId, string alertId, bool read)
    {
        var grounds = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            grounds = (await GetGroundIdsForUser(userId))?.ToList();
        }
        
        try
        {
            var alert = await _dbContext
                .Alerts
                    .Where(p => 
                        (grounds.Any() || grounds.Contains(p.FarmerGroundId)) &&
                        p.ID == alertId)
                    .SingleAsync();

            alert.MarkedAsRead = read;

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return false;
        }

        return true;
    }

    public async Task<IFarmerGround> CreateFarmerGround(string userId, FarmerGroundRequestData data)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (data == null) throw new ArgumentNullException(nameof(data));

        var groundCount = await 
            _dbContext
                .Grounds
                .Where(g => g.UserID == userId)
                .CountAsync() + 1; // +1 for the next automatic Ground Name

        var ground = new FarmerGround
        {
            GroundName = data.GroundName ?? "FarmerGround_" + userId + "_" + groundCount,
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            WidthInMeters = data.WidthInMeters,
            LengthInMeters = data.LengthInMeters,
            UserID = userId
        };

        await _dbContext.Grounds.AddAsync(ground);
        await _dbContext.SaveChangesAsync();

        return ground;
    }

    public async Task<string> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (data == null) throw new ArgumentNullException(nameof(data));

        var ground = await _dbContext
            .Grounds
                .SingleAsync(x => x.UserID == userId && x.ID == data.FarmerGroundId);

        var plantKind = await _dbContext.Plants.FirstOrDefaultAsync(x => x.ID == data.PlantKindID);
        if (plantKind == null)
        {
            throw new InvalidOperationException("invalid plant kind");
        }

        if (data.PlantX < 0 || data.PlantX >= Helpers.Utils.GetCellsFromMeters(ground.WidthInMeters) ||
            data.PlantY < 0 || data.PlantY >= Helpers.Utils.GetCellsFromMeters(ground.LengthInMeters))
        {
            throw new InvalidOperationException("invalid position");
        }

        var existingPlantYn = 
            await 
                _dbContext
                    .PlantsInstance
                    .AnyAsync(x => x.PlantX == data.PlantX && x.PlantY == data.PlantY);

        if (existingPlantYn)
        {
            throw new InvalidOperationException("place already occupied");
        }

        var plant = new FarmerPlantInstance
        {
            FarmerGroundId = data.FarmerGroundId,
            PlantKindID = data.PlantKindID,
            PlantDepth = data.PlantDepth ?? plantKind.PlantDepth,
            PlantWidth = data.PlantWidth ?? plantKind.PlantWidth,
            PlantX = data.PlantX,
            PlantY = data.PlantY,
            PlantedWhen = data.PlantedWhen ?? DateTime.UtcNow
        };

        await _dbContext.PlantsInstance.AddAsync(plant);
        await _dbContext.SaveChangesAsync();

        return plant.ID;
    }

    public async Task<IFarmerSettings> GetUserSettings(string userId)
    {
        var userSettingsStr = (await 
            _dbContext
                .Users
                    .FirstOrDefaultAsync(u => u.ID == userId)
                    )?.SerializedSettings;

        if (string.IsNullOrEmpty(userSettingsStr))
        {
            await Task.CompletedTask;
            return null;
        }

        return JsonSerializer.Deserialize<FarmerSettings>(userSettingsStr);
    }

    public async Task<bool> SaveUserSettings(string userId, IFarmerSettings settings)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        // check user
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.ID == userId);

        if (user == null)
        {
            return false;
        }

        // save settings
        user.SerializedSettings = JsonSerializer.Serialize(settings);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<IEnumerable<string>> GetGroundIdsForUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

        return await _dbContext
            .Grounds
                .Where(x => x.UserID == userId)
                .Select(g => g.ID)
                .ToListAsync();
    }
}
