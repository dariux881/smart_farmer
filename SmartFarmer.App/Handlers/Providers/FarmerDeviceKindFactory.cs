using System;
using SmartFarmer.DeviceManagers;
using SmartFarmer.Configurations;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers.Providers;

public class FarmerDeviceKindFactory : IFarmerDeviceKindFactory
{
    private readonly IFarmerConfigurationProvider _configProvider;

    public FarmerDeviceKindFactory()
    {
        _configProvider = FarmerServiceLocator.GetService<IFarmerConfigurationProvider>(true);
    }

    public IFarmerDeviceManager GetNewDeviceManager(string gardenId)
    {
        var config = GetGardenConfiguration(gardenId);
        if (config == null) throw new InvalidProgramException();
        
        var kind = config.DeviceKind;

        switch (kind)
        {
            case DeviceKindEnum.Remote:
                {
                    return 
                        new ExternalDeviceProxy(
                            FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true).Gardens[config.GardenId], 
                            config.SerialConfiguration,
                            _configProvider.GetHubConfiguration());
                }

            case DeviceKindEnum.Mock:
                {
                    return new MockedDeviceManager();
                }
            
            default:
                throw new NotSupportedException();
        }
    }

    private GardenConfiguration GetGardenConfiguration(string gardenId)
    {
        return _configProvider.GetGardenConfiguration(gardenId);
    }
}