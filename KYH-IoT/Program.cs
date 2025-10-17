using System;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class Program
    {
        // Small console menu to run simulation or view analysis.
        // Student-level UI: choose option, press keys as prompted.
        static async Task Main(string[] args)
        {
            var analyzer = new Analyss();
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("=== Car Telemetry Analyzer ===");
                Console.WriteLine("1. Run Simulation");
                Console.WriteLine("2. View Analysis (Last 100 samples)");
                Console.WriteLine("3. View Analysis (Last 24h) -- requires channel id");
                Console.WriteLine("4. Exit");
                Console.Write("\nSelect option (1-4): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await RunSimulation(analyzer);
                        break;
                    case "2":
                        ViewAnalysis(analyzer, AnalysisPeriod.Last100);
                        break;
                    case "3":
                        ViewAnalysis(analyzer, AnalysisPeriod.Last24h);
                        break;
                    case "4":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("\nInvalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Run the simulator for a short time and send samples to ThingSpeak.
        private static async Task RunSimulation(Analyss analyzer)
        {
            Console.Clear();
            Console.WriteLine("=== Car Telemetry Simulator ===");
            Console.WriteLine("Press ENTER to start, ESC to stop.");
            Console.ReadLine();

            var car = new Car();
            var thingspeak = new ThingSpeak();

            var start = DateTime.UtcNow;
            var simulationDuration = TimeSpan.FromMinutes(1);
            var interval = TimeSpan.FromSeconds(4);

            while (DateTime.UtcNow - start < simulationDuration)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    break;

                var sample = car.NextSample(interval);
                analyzer.AddSample(sample);

                Console.WriteLine(
                    $"RPM:{sample.Rpm}  Speed:{sample.SpeedKmH} km/h  FuelLiters:{sample.FuelLiters}L  Fuel:{sample.FuelPercent}%  Temp:{sample.EngineTempC}°C");

                await thingspeak.SendAsync(sample);
                await Task.Delay(interval);
            }

            Console.WriteLine("\n=== Simulation finished ===");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        // Show analysis summary. For Last24h this is a placeholder that in a real setup
        // should call ThingSpeak.ReadAsync(channelId) and then compute averages.
        private static void ViewAnalysis(Analyss analyzer, AnalysisPeriod period)
        {
            Console.Clear();
            Console.WriteLine($"=== Analysis ({(period == AnalysisPeriod.Last100 ? "Last 100 samples" : "Last 24h")}) ===\n");
            
            var summary = analyzer.GetSummary();
            Console.WriteLine(summary);
            
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }

    enum AnalysisPeriod
    {
        Last100,
        Last24h
    }
}
