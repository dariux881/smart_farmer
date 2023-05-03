using System;

namespace SmartFarmer.OperationalManagement;

[Flags]
public enum AppOperationalMode {
    Console,
    Auto,
    RemoteCLI
}
