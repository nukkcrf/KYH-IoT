using System;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Car Telemetry Simulator (ThingSpeak) ===");
            Console.WriteLine("Press ENTER to start, ESC to stop.");
            Console.ReadLine();

            var car = new Car();
            var thingspeak = new ThingSpeak();
            var analyzer = new Analyss();

            var start = DateTime.UtcNow;
            var simulationDuration = TimeSpan.FromMinutes(1);
            var interval = TimeSpan.FromSeconds(4); // ThingSpeak free limit

            var lastStatsPrinted = DateTime.MinValue;
            var statsInterval = TimeSpan.FromSeconds(15);

            while (DateTime.UtcNow - start < simulationDuration)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    break;

                var sample = car.NextSample(interval);
                analyzer.AddSample(sample);


                Console.WriteLine(
                    $"RPM:{sample.Rpm}  Speed:{sample.SpeedKmH} km/h FuelLiters:{sample.FuelLiters}L Fuel:{sample.FuelPercent}%  Temp:{sample.EngineTempC}°C");

                bool sent = await thingspeak.SendAsync(sample);
                if (DateTime.UtcNow - lastStatsPrinted >= statsInterval)
                {
                    //Console.WriteLine(analyzer.GetSummary());
                    lastStatsPrinted = DateTime.UtcNow;
                }

                await Task.Delay(interval);
            }
            Console.WriteLine("\n********************************");
            Console.WriteLine();
            Console.WriteLine("\n====Final analysis====");
            Console.WriteLine();
            Console.WriteLine(analyzer.GetSummary());
            Console.WriteLine("Simulation finished.");
        }
    }
}
