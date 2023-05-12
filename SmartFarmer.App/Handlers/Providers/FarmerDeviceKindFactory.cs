using System;
using SmartFarmer.Communication;
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

    public IFarmerDeviceManager GetNewDeviceManager(string groundId, DeviceKindEnum kind)
    {
        switch (kind)
        {
            case DeviceKindEnum.Remote:
                {
                    var config = GetGroundConfiguration(groundId);

                    if (config == null) throw new InvalidProgramException();
                    return 
                        new ExternalDeviceProxy(
                            LocalConfiguration.Grounds[config.GroundId], 
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

    private GroundConfiguration GetGroundConfiguration(string groundId)
    {
        return _configProvider.GetGroundConfiguration(groundId);
    }
}