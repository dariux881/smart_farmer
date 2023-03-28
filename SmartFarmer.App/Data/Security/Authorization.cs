
using System;
//using Newtonsoft.Json;
using SmartFarmer.Utils;

namespace SmartFarmer.Data.Security;

public class Authorization : IFarmerService
{
    public string ID { get; set; }
    public string Name { get; set; }
    public DateTime? StartAuthorizationDt { get; set; }
    public DateTime? EndAuthorizationDt { get; set; }
}