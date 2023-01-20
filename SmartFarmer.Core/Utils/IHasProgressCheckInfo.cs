using System;
using Newtonsoft.Json;

namespace SmartFarmer.Utils
{
    public interface IHasProgressCheckInfo
    {
        [JsonIgnore]
        bool IsInProgress { get; }
        [JsonIgnore]
        Exception LastException { get; }
    }
}