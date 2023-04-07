using System;
using SmartFarmer.OperationalManagement;

namespace SmartFarmer.Helpers;

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

    public string PlanCheckCronSchedule { get; set; }

    public AppOperationalMode? AppOperationalMode { get; private set; }
}