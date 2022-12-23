using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.Tests.Utils
{
    public class InformationLoader
    {
        public static IEnumerable<IFarmerPlant> LoadPlantsFromCsvFile(string filename)
        {
            List<IFarmerPlant> plants = new List<IFarmerPlant>();

            using (var reader = new StreamReader(filename))
            {
                int id = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (line == null) break;
                    if (line == string.Empty || line.Trim().StartsWith("#")) continue;

                    var tokens = line.Split(",");

                    var code = tokens[0].Trim();
                    var name = tokens[1].Trim();
                    var waterAmount = tokens[2].Trim();
                    var waterTimes = tokens[3].Trim();
                    var month = tokens[4].Trim();
                    var weeksToHarvest = tokens[5].Trim();
                    var width = tokens[6].Trim();
                    var depth = tokens[7].Trim();

                    plants.Add(new FarmerPlant()
                    {
                        ID = id + "",
                        Code = code,
                        FriendlyName = name,
                        IrrigationInfo = new FarmerIrrigationTaskInfo()
                        {
                            AmountOfWaterInLitersPerTime = double.Parse(waterAmount, CultureInfo.InvariantCulture),
                            TimesPerWeek = int.Parse(waterTimes, CultureInfo.InvariantCulture)
                        },
                        MonthToPlan = int.Parse(month, CultureInfo.InvariantCulture),
                        NumberOfWeeksToHarvest = int.Parse(weeksToHarvest, CultureInfo.InvariantCulture),
                        PlantWidth = int.Parse(width, CultureInfo.InvariantCulture),
                        PlantDepth = int.Parse(depth, CultureInfo.InvariantCulture)
                    });

                    id++;
                }
            }

            return plants;
        }

        public static IEnumerable<IFarmerPlantInstance> LoadPlantInstanceFromCsvFile(string filename, IEnumerable<IFarmerPlant> plants)
        {
            if (plants == null) throw new ArgumentNullException(nameof(plants));
            List<IFarmerPlantInstance> plantsInstance = new List<IFarmerPlantInstance>();

            using var reader = new StreamReader(filename);

            Dictionary<string, int> numberByPlant = new Dictionary<string, int>();
            var id = 0;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                
                if (line == null) break;
                if (line == string.Empty || line.Trim().StartsWith("#")) continue;

                var tokens = line.Split(",");

                var name = tokens[0].Trim();
                var kind = tokens[1].Trim();
                var plantedWhen = tokens[2].Trim();
                var x = tokens[3].Trim();
                var y = tokens[4].Trim();

                var plantKind = plants.First(x => x.ID == kind);

                if (!numberByPlant.ContainsKey(plantKind.ID))
                {
                    numberByPlant.Add(plantKind.ID, 0);
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = plantKind.FriendlyName + numberByPlant[plantKind.ID];
                    numberByPlant[plantKind.ID]++;
                }

                plantsInstance.Add(new FarmerPlantInstance(plantKind)
                {
                    ID = id + "",
                    PlantName = name,
                    PlantedWhen = string.IsNullOrEmpty(plantedWhen) ? DateTime.UtcNow : Convert.ToDateTime(plantedWhen),
                    PlantX = int.Parse(x, CultureInfo.InvariantCulture),
                    PlantY = int.Parse(y, CultureInfo.InvariantCulture)
                });

                id++;
            }

            return plantsInstance;
        }
    }
}
