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
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var tokens = line.Split(",");

                    plants.Add(new FarmerPlant()
                    {
                        Name = tokens[0],
                        IrrigationInfo = new FarmerIrrigationTaskInfo()
                        {
                            AmountOfWaterInLitersPerTime = double.Parse(tokens[1], CultureInfo.InvariantCulture),
                            TimesPerWeek = int.Parse(tokens[2], CultureInfo.InvariantCulture)
                        },
                        MonthToPlan = int.Parse(tokens[3], CultureInfo.InvariantCulture),
                        NumberOfWeeksToHarvest = int.Parse(tokens[4], CultureInfo.InvariantCulture),
                        PlantWidth = int.Parse(tokens[5], CultureInfo.InvariantCulture),
                        PlantDepth = int.Parse(tokens[6], CultureInfo.InvariantCulture)
                    });
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
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;

                var tokens = line.Split(",");

                var plantKind = plants.First(x => x.Name == tokens[1]);

                if (!numberByPlant.ContainsKey(plantKind.Name))
                {
                    numberByPlant.Add(plantKind.Name, 0);
                }

                var plantName = tokens[0];
                if (string.IsNullOrEmpty(plantName))
                {
                    plantName = plantKind.Name + numberByPlant[plantKind.Name];
                    numberByPlant[plantKind.Name]++;
                }

                plantsInstance.Add(new FarmerPlantInstance(plants.First(x => x.Name == tokens[1]))
                {
                    PlantName = plantName,
                    PlantedWhen = string.IsNullOrEmpty(tokens[2]) ? DateTime.UtcNow : Convert.ToDateTime(tokens[2]),
                    PlantX = int.Parse(tokens[3], CultureInfo.InvariantCulture),
                    PlantY = int.Parse(tokens[4], CultureInfo.InvariantCulture)
                });
            }

            return plantsInstance;
        }
    }
}
