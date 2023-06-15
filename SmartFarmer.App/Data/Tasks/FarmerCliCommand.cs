using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFarmer.Tasks;

public class FarmerCliCommand : IFarmerCliCommand
{
    public string UserId { get; set; }
    public string GardenId { get; set; }
    public string Command { get; set; }
    public FarmerCliCommandArgs Args { get; set; }

    public override string ToString()
    {
        var args = new StringBuilder();
        if (Args != null && Args.Any())
        {
            foreach (var arg in Args)
            {
                args.Append(" ");
                args.Append(arg.Key);

                if (arg.Value != null && arg.Value.Any())
                {
                    args.Append(" ");
                    args.Append(arg.Value.Aggregate((a1, a2) => a1 + " " + a2));
                }
            }
        }

        return $"{Command} {args}\n\tUser {UserId} on garden {GardenId}";
    }
}