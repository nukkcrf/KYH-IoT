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

            var start = DateTime.UtcNow;
            var simulationDuration = TimeSpan.FromMinutes(10);
            var interval = TimeSpan.FromSeconds(4); // ThingSpeak free limit

            while (DateTime.UtcNow - start < simulationDuration)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    break;

                var sample = car.NextSample(interval);

                Console.WriteLine(
                    $"RPM:{sample.Rpm}  Speed:{sample.SpeedKmH} km/h FuelLiters:{sample.FuelLiters}L Fuel:{sample.FuelPercent}%  Temp:{sample.EngineTempC}°C");

                bool sent = await thingspeak.SendAsync(sample);
                //if (!sent)
                 //   Console.WriteLine("[WARN] ThingSpeak did not accept the data point.");

                await Task.Delay(interval);
            }

            Console.WriteLine("Simulation finished.");
        }
    }
}
