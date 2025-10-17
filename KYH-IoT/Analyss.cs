using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class Analyss
    {
        private readonly List<TelemetryData> _samples = new();
        private const int maxSamples = 100; //Keep last 100 samples

        public void AddSample(TelemetryData sample)
        {
            _samples.Add(sample);
            if (_samples.Count > maxSamples)
            {
                _samples.RemoveAt(0); // Remove oldest sample
            }
        }
        public string GetSummary()
        {
            if (_samples.Count == 0)
            {
                return "No samples available.";
            }
                var avgSpeed = _samples.Average(s => s.SpeedKmH);
                var avgRpm = _samples.Average(s => s.Rpm);
                var maxSpeed = _samples.Max(s => s.SpeedKmH);
                var maxRpm = _samples.Max(s => s.Rpm);
                var last = _samples.Last();

                return $"Analysis: samples={_samples.Count}  AvgSpeed={avgSpeed:F1} km/h  AvgRPM={avgRpm:F0}  MaxSpeed={maxSpeed} km/h  MaxRPM={maxRpm}  Fuel={last.FuelLiters:F2}L ({last.FuelPercent:F1}%)  Temp={last.EngineTempC}°C";
            }
        
            public void ClearSamples()
        {
            _samples.Clear();


        }
    

    }
}
