using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string solarFilename = @"C:\energy\AllSolar.csv";
        string energyFilename = @"C:\energy\AllEnergy.csv";
        decimal batteryMaximum = 8.2M;
        DateTime start = new DateTime(2021,1,1);
        decimal priceperKwHinPounds = 0.2846M;
        decimal newpriceperKwHinPounds = 0.340M;
        decimal extraWinterUsage = 1.5M; //1.5M;
        Dictionary<string, decimal> pricecaps = new Dictionary<string, decimal>()
        {
            { "2010", 0.127m }, 
            { "2011", 0.137m },
            { "2012", 0.145m },
            { "2013", 0.152m },
            { "2014", 0.156m },
            { "2015", 0.154m },
            { "2016", 0.154m },
            { "2017", 0.165m },
            { "2018", 0.178m },
            { "2019", 0.194m },
            { "2020", 0.196m },
            { "2021", 0.212m },
            { "2022", 0.2846m },
            { "2022+", 0.340m }
        };

        Console.WriteLine("Reading Solar file");
        Dictionary<string,string> solar = new Dictionary<string, string>();
        solar = LoadFile(solarFilename);

        Console.WriteLine("Reading energy file");
        Dictionary<string, string> energy = new Dictionary<string, string>();
        energy = LoadFile(energyFilename);

        DateTime processingDate = start;
        decimal batteryLevelkWh = 0.0M;
        decimal usedGridkWh = 0.0M;
        decimal totalUsedWithoutBattery = 0.0M;
        decimal batterySavedkWh = 0.0M;
        decimal totalBatterySavedkWh = 0.0M;
        while (processingDate < new DateTime(2022,1,1))
        {
            batteryLevelkWh += decimal.Parse(solar[processingDate.ToString("dd/MM/yyyy")]);
            if (batteryLevelkWh > batteryMaximum) { batteryLevelkWh = batteryMaximum;  }
            usedGridkWh = decimal.Parse(energy[processingDate.ToString("dd/MM/yyyy")]);

            if (processingDate.Month >= 10 || processingDate.Month <= 4) {
                usedGridkWh += extraWinterUsage;
            };

            if (batteryLevelkWh >= usedGridkWh) { 
                batterySavedkWh = usedGridkWh; } 
            else { batterySavedkWh = batteryLevelkWh; 
            };

            batteryLevelkWh-= usedGridkWh;
            if (batteryLevelkWh < 0.0M) { batteryLevelkWh = 0.0M; }

            totalUsedWithoutBattery += usedGridkWh;
            totalBatterySavedkWh += batterySavedkWh;

            processingDate = processingDate.AddDays(1);
        }

        Console.WriteLine("Total used from grid without battery (in year) {0}.  With the current price tariff that's £{1}", totalUsedWithoutBattery.ToString("0.00"), (totalUsedWithoutBattery * priceperKwHinPounds).ToString("0.##"));
        Console.WriteLine("Total used from grid with battery (in year) {0}", (totalUsedWithoutBattery-totalBatterySavedkWh).ToString("0.00"));
        Console.WriteLine("Battery saved {0} kWh over the year", totalBatterySavedkWh.ToString("0.00"));
        Console.WriteLine("Thats £{0} with the current price of {1}", (totalBatterySavedkWh * priceperKwHinPounds).ToString("0.00"), priceperKwHinPounds.ToString("0.00") );
        Console.WriteLine("With the new tariff that would be £{0} saved.", (totalBatterySavedkWh * newpriceperKwHinPounds).ToString("0.00"));

        decimal runningTotal = 0.0m;
        foreach (var pricecap in pricecaps)
        {
            Console.WriteLine(" {0} {1} would have saved you £{2}", pricecap.Key, pricecap.Value, (pricecap.Value * totalBatterySavedkWh).ToString("0.00"));
            runningTotal += pricecap.Value * totalBatterySavedkWh;
        }
        Console.WriteLine("  That's a total of £{0}", runningTotal.ToString("0.00"));
    }   

    static public Dictionary<string, string> LoadFile(string filename)
    {
        Dictionary<string, string> filecontents = new Dictionary<string, string>();

        using (var reader = new StreamReader(filename))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                filecontents.Add(values[0], values[1]);
            }
        }
        return filecontents;
    }
}

