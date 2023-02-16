using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;
using SmartFarmer.Utils;

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

                    var irrigationInfo = new FarmerIrrigationTaskInfo()
                        {
                            AmountOfWaterInLitersPerTime = double.Parse(waterAmount, CultureInfo.InvariantCulture),
                            TimesPerWeek = int.Parse(waterTimes, CultureInfo.InvariantCulture)
                        };

                    FarmerIrrigationInfoProvider.Instance.AddFarmerService(irrigationInfo);

                    plants.Add(new FarmerPlant(
                        id + "", 
                        code, 
                        name,
                        irrigationInfo)
                    {
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

        public static IEnumerable<IFarmerPlantInstance> LoadPlantInstanceFromCsvFile(
            string filename)
        {
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

                var plantKind = FarmerPlantProvider.Instance.GetFarmerService(kind);

                if (!numberByPlant.ContainsKey(plantKind.ID))
                {
                    numberByPlant.Add(plantKind.ID, 0);
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = plantKind.FriendlyName + numberByPlant[kind];
                    numberByPlant[kind]++;
                }

                plantsInstance.Add(new FarmerPlantInstance(id + "", kind, name)
                {
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
