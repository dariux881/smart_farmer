using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.DTOs;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.Movement;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services;

public class SmartFarmerGardenControllerService : ISmartFarmerGardenControllerService 
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerGardenControllerService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public event EventHandler<DevicePositionEventArgs> NewDevicePosition;
    public event EventHandler<DevicePositionsEventArgs> NewDevicePositions;

    public async Task<IEnumerable<IFarmerGarden>> GetFarmerGardenByUserIdAsync(string userId)
    {
        return await _repository.GetFarmerGardenByUserIdAsync(userId);
    }
    
    public async Task<IFarmerGarden> GetFarmerGardenByIdForUserAsync(string userId, string gardenId)
    {
        return await _repository.GetFarmerGardenByIdForUserAsync(userId, gardenId);
    }

    public async Task<IFarmerGarden> CreateFarmerGarden(string userId, FarmerGardenRequestData data)
    {
        return await _repository.CreateFarmerGarden(userId, data);
    }

    public IFarmerCliCommand BuildAndCheckCliCommand(string userId, string gardenId, string commandStr)
    {
        IFarmerCliCommand command = BuildCliCommand(userId, gardenId, commandStr);

        // check command
        if (!IsCliCommandValid(command))
        {
            return null;
        }

        return command;
    }

    public async Task<FarmerDevicePosition> NotifyDevicePosition(string userId, FarmerDevicePositionRequestData position)
    {
        if (position == null) throw new ArgumentNullException(nameof(position));

        var garden = await GetFarmerGardenByIdForUserAsync(userId, position.GardenId) as FarmerGarden;
        if (garden == null) return null; // no valid garden

        var storedPosition = await _repository.SaveDevicePosition(userId, position);

        if (storedPosition != null)
        {
            NewDevicePosition?.Invoke(this, new DevicePositionEventArgs(storedPosition));
        }

        return storedPosition;
    }

    public async Task<bool> NotifyDevicePositions(string userId, FarmerDevicePositionsRequestData positions)
    {
        if (positions == null) throw new ArgumentNullException(nameof(positions));

        var garden = await GetFarmerGardenByIdForUserAsync(userId, positions.GardenId) as FarmerGarden;
        if (garden == null) return false; // no valid garden

        var ids = await _repository.SaveDevicePositions(userId, positions);

        if (ids != null)
        {
            NewDevicePositions?.Invoke(this, new DevicePositionsEventArgs(ids));
        }

        return ids != null && ids.Any();
    }

    public async Task<IEnumerable<FarmerDevicePosition>> GetDeviceDevicePositionHistory(string userId, string gardenId, string runId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (gardenId == null) throw new ArgumentNullException(nameof(gardenId));

        var garden = await GetFarmerGardenByIdForUserAsync(userId, gardenId) as FarmerGarden;
        if (garden == null) return null; // no valid garden

        return await _repository.GetDevicePositionHistory(userId, gardenId, runId);
    }

    private static IFarmerCliCommand BuildCliCommand(string userId, string gardenId, string commandStr)
    {
        if (!ExtractCliCommandParts(commandStr, out var command, out var args))
        {
            return null;
        }

        IFarmerCliCommand cliCommand = new FarmerCliCommand()
        {
            UserId = userId,
            GardenId = gardenId,
            Command = command,
            Args = args
        };
        return cliCommand;
    }

    private static bool ExtractCliCommandParts(string commandStr, out string command, out FarmerCliCommandArgs args)
    {
        command = null;
        args = null;

        if (string.IsNullOrEmpty(commandStr)) return false;

        var commandParts = commandStr.Split(" ");
        command = commandParts[0].Trim();

        if (commandParts.Length == 1)
        {
            args = null;
            return true;
        }

        args = new FarmerCliCommandArgs();
        var commandIndex = 1;

        List<string> referencePartDetails = null;
        while (commandIndex < commandParts.Length)
        {
            var part = commandParts[commandIndex].Trim();
            
            if (part.StartsWith("-"))
            {
                referencePartDetails = new List<string>();
                args.Add(new KeyValuePair<string, List<string>>(part, referencePartDetails));
            }
            else if (referencePartDetails != null)
            {
                referencePartDetails.Add(part);
            }
            else
            {
                // invalid pattern found
                command = null;
                args = null;

                return false;
            }

            commandIndex++;
        }

        return true;
    }

    private bool IsCliCommandValid(IFarmerCliCommand command)
    {
        return true;
    }
}