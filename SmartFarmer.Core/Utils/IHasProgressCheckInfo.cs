using System;

namespace SmartFarmer.Utils
{
    public interface IHasProgressCheckInfo
    {
        bool IsInProgress { get; }
        Exception LastException { get; }
    }
}