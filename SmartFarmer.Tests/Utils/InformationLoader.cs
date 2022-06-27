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
                        NumberOfWeeksToHarvest = int.Parse(tokens[4], CultureInfo.InvariantCulture)
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

                plantsInstance.Add(new FarmerPlantInstance()
                {
                    PlantName = plantName,
                    Plant = plants.FirstOrDefault(x => x.Name == tokens[1]),
                    PlantedWhen = string.IsNullOrEmpty(tokens[2]) ? DateTime.UtcNow : Convert.ToDateTime(tokens[2])
                });
            }

            return plantsInstance;
        }

        public static IEnumerable<IFarmerRow> LoadFarmerRowsFromCsvFile(string filename, IEnumerable<IFarmerPlantInstance> plants)
        {
            List<IFarmerRow> plantsInRows = new List<IFarmerRow>();

            using var reader = new StreamReader(filename);

            var rows = new Dictionary<string, IFarmerRow>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;

                var tokens = line.Split(",");

                if (!rows.ContainsKey(tokens[0]))
                {
                    rows.Add(tokens[0], new FarmerRow());
                }

                rows[tokens[0]].PlantsInRow.Add(
                    plants.First(x => x.PlantName == tokens[1]), 
                    double.Parse(tokens[2], CultureInfo.InvariantCulture));
            }

            plantsInRows = rows.Select(x => x.Value).ToList();

            return plantsInRows;
        }
    }
}
