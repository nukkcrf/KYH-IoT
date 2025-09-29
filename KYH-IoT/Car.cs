using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class Car

    {
        // These constants can be adjusted to change the simulation's behavior.
        private const double MaxFuelConsumptionPerSecond = 0.005; // liters per second
        private const int IdleRpm = 800;
        private const int MaxRpm = 6000;
        private const int MinSpeed = 0;
        private const int MaxSpeed = 120; // km/h
        private const int UpdateIntervalSeconds = 15;
        private const int TripDurationMinutes = 10;

        // Instance variables to track the car's state
        private double _currentSpeed = 0;
        private double _currentRpm = IdleRpm;
        private double _currentFuelLevel = 100.0; // Starts at 100%
        private readonly Random _random = new Random();
        private readonly HttpClient _httpClient = new HttpClient();

        // Represents a single telemetry data point
        private class TelemetryData
        {
            public double Speed { get; set; }

            public double Rpm { get; set; }

            public double FuelLevel { get; set; }

            public double EngineTemp { get; set; }
        }

        // Main simulation loop
        public async void StartSimulation()
        {
            Console.WriteLine("Starting car telemetry simulation...");
            var startTime = DateTime.Now;

            while (true)
            {
                var elapsedTime = DateTime.Now - startTime;
                if (elapsedTime.TotalMinutes >= TripDurationMinutes)
                {
                    Console.WriteLine("Simulation finished. Trip duration reached.");
                    break;
                }

                SimulateTrip();

                var data = new TelemetryData
                {
                    Speed = Math.Round(_currentSpeed, 2),
                    Rpm = Math.Round(_currentRpm, 2),
                    FuelLevel = Math.Round(_currentFuelLevel, 2),
                    EngineTemp = Math.Round(GenerateEngineTemperature(), 2)
                };

                Console.WriteLine($"Time: {elapsedTime.TotalSeconds:F0}s | Speed: {_currentSpeed:F0} km/h | RPM: {_currentRpm:F0} | Fuel: {_currentFuelLevel:F1}%");


                Thread.Sleep(TimeSpan.FromSeconds(UpdateIntervalSeconds));
            }
        }

        // Simulates the car's movement and state changes
        private void SimulateTrip()
        {
            // Simple state machine for the trip: Idle -> Accelerate -> Cruise -> Decelerate -> Idle
            string tripPhase;
            if (_currentSpeed < 30)
            {
                tripPhase = "Accelerating";
            }
            else if (_currentSpeed < 90)
            {
                tripPhase = "Cruising";
            }
            else
            {
                tripPhase = "Decelerating";
            }

            // Adjust speed and RPM based on the current phase
            switch (tripPhase)
            {
                case "Accelerating":
                    _currentSpeed += _random.Next(2, 5); // Increase speed
                    _currentRpm = CalculateRpm(_currentSpeed);
                    break;
                case "Cruising":
                    _currentSpeed += _random.NextDouble() * 2 - 1; // Slight random fluctuation
                    _currentRpm = CalculateRpm(_currentSpeed);
                    break;
                case "Decelerating":
                    _currentSpeed -= _random.Next(2, 5); // Decrease speed
                    _currentRpm = CalculateRpm(_currentSpeed);
                    break;
            }

            // Clamp speed to a realistic range
            _currentSpeed = Math.Max(MinSpeed, Math.Min(MaxSpeed, _currentSpeed));
            _currentRpm = Math.Max(IdleRpm, _currentRpm);

            // Calculate fuel consumption based on speed and RPM
            double consumptionRate = (_currentSpeed / MaxSpeed) * (_currentRpm / MaxRpm) * MaxFuelConsumptionPerSecond;
            _currentFuelLevel -= consumptionRate * UpdateIntervalSeconds;
            _currentFuelLevel = Math.Max(0, _currentFuelLevel);
        }

        // A simple function to calculate RPM based on speed for a more realistic feel
        private double CalculateRpm(double speed)
        {
            if (speed < 1)
            {
                return IdleRpm;
            }
            // A simple linear relationship for simulation purposes
            return IdleRpm + (speed / MaxSpeed) * (MaxRpm - IdleRpm);
        }

        // Generates a realistic engine temperature based on speed
        private double GenerateEngineTemperature()
        {
            // Engine runs hotter at higher RPM/speed
            double baseTemp = 85;
            double speedFactor = _currentSpeed / MaxSpeed;
            double temp = baseTemp + (speedFactor * _random.Next(0, 15));
            return Math.Max(80, Math.Min(110, temp));
        }

    }
}
