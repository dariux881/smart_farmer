using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Utils;

namespace SmartFarmer.DTOs.Security;

public class User : IFarmerService
{
    public User()
    {
        Authorizations = new List<Authorization>();
    }

    public string ID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    [JsonIgnore]
    public string Password { get; set; }

    [JsonIgnore]
    public string SerializedSettings { get; set; }

    [JsonIgnore]
    public List<Authorization> Authorizations { get; set; }
    public IReadOnlyCollection<string> AuthorizationIds => 
        Authorizations?
            .Select(x => x.ID)
            .ToList()
            .AsReadOnly();
}