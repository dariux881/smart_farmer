
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Utils;

namespace SmartFarmer.DTOs.Security;

public class User : IFarmerService
{
    public string ID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }

    [JsonIgnore]
    public string Password { get; set; }
}