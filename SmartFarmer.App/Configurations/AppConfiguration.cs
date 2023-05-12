using System;
using System.Collections.Generic;
using SmartFarmer.OperationalManagement;

namespace SmartFarmer.Configurations;

public class AppConfiguration
{
    private string _operationalMode;

    public string OperationalMode 
    {
        get => _operationalMode;
        set {
            _operationalMode = value;
            if (value == null)
            {
                AppOperationalMode = null;
                return;
            }

            var modes = value.Split('|');
            foreach (var mode in modes)
            {
                if (!Enum.TryParse<AppOperationalMode>(mode, out var modeEnum))
                {
                    AppOperationalMode = null;
                    throw new InvalidOperationException(mode + " is not a valid operational mode");
                }

                AppOperationalMode = AppOperationalMode|modeEnum ?? modeEnum;
            }
        }
    }

    public List<string> LocalGroundIds { get; set; }

    public string PlanCheckCronSchedule { get; set; }

    public AppOperationalMode? AppOperationalMode { get; private set; }
}