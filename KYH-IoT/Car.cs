using System;

namespace KYH_IoT
{
    internal class Car
    {
        private bool _engineOn = true;
        private bool _lowFuelWarningIssued = false;

        // Simulation parameters
        private const int MaxSpeed = 120;
        private const int IdleRpm = 800;
        private const int MaxRpm = 6000;

        // Fuel parameters
        private const double TankLiters = 100.0;   // big tank => slow % drop; adjust if needed
        private const double IdleFuelLph = 1.0;    // liters per hour at idle

        // Current state
        private double _speed;
        private double _rpm;
        private double _fuelLiters;
        private int _engineTempC = 90;

        // Target speed management
        private double _targetSpeed;
        private DateTime _nextTargetChangeUtc;

        private readonly Random _rng = new();

        public Car()
        {
            _speed = 0;
            _rpm = IdleRpm;
            _fuelLiters = TankLiters;
            SetNewTargetSpeed();
        }

        public TelemetryData NextSample(TimeSpan step)
        {
            // If engine is OFF, keep car stopped, cool down and report zeros
            if (!_engineOn)
            {
                _speed = 0;
                _rpm = 0;
                if (_engineTempC > 80) _engineTempC = Math.Max(80, _engineTempC - 1);

                return new TelemetryData
                {
                    Rpm = 0,
                    SpeedKmH = 0,
                    FuelPercent = (int)Math.Round((_fuelLiters / TankLiters) * 100.0),
                    EngineTempC = _engineTempC
                };
            }

            MaybeChangeTarget();

            // Limit acceleration for realism
            double maxDeltaPerSec = 3.0;
            double maxStep = maxDeltaPerSec * step.TotalSeconds;

            double delta = _targetSpeed - _speed;
            double change = Math.Clamp(delta, -maxStep, maxStep);

            // Add small noise
            change += (_rng.NextDouble() - 0.5) * 0.6;

            _speed = Math.Clamp(_speed + change, 0, MaxSpeed);

            // Consume fuel BEFORE we decide final RPM; may shut engine off
            ConsumeFuel(step);

            // If engine shut down due to no fuel, return zeros (no CalculateRpm!)
            if (!_engineOn)
            {
                _speed = 0;
                _rpm = 0;
                if (_engineTempC > 80) _engineTempC = Math.Max(80, _engineTempC - 1);

                return new TelemetryData
                {
                    Rpm = 0,
                    SpeedKmH = 0,
                    FuelPercent = (int)Math.Round((_fuelLiters / TankLiters) * 100.0),
                    EngineTempC = _engineTempC
                };
            }

            // RPM derived from speed only if engine is ON
            _rpm = CalculateRpm(_speed);

            // Engine temp variation
            int tempDrift = (_rpm > 4000 ? 2 : 0) + _rng.Next(-1, 2);
            _engineTempC = Math.Clamp(_engineTempC + tempDrift, 80, 99);

            return new TelemetryData
            {
                Rpm = (int)Math.Round(_rpm),
                SpeedKmH = (int)Math.Round(_speed),
                FuelPercent = (int)Math.Round((_fuelLiters / TankLiters) * 100.0),
                EngineTempC = _engineTempC
            };
        }

        private void MaybeChangeTarget()
        {
            var now = DateTime.UtcNow;
            if (now >= _nextTargetChangeUtc)
            {
                SetNewTargetSpeed();
                _nextTargetChangeUtc = now.AddSeconds(_rng.Next(30, 61));
            }
        }

        private void SetNewTargetSpeed()
        {
            _targetSpeed = _rng.Next(0, MaxSpeed + 1);
        }

        private static double CalculateRpm(double speed)
        {
            if (speed < 1) return IdleRpm;
            return IdleRpm + (speed / MaxSpeed) * (MaxRpm - IdleRpm);
        }

        private void ConsumeFuel(TimeSpan step)
        {
            if (!_engineOn) return;

            // Use HOURS for fuel math or 
            double hours = step.TotalSeconds;
            double usedLiters;

            // Percent, not liters absolute, for warnings
            double fuelPercent = (_fuelLiters / TankLiters) * 100.0;

            // One-time low fuel warning at < 25%
            if (fuelPercent <= 25.0 && !_lowFuelWarningIssued && _fuelLiters > 0)
            {
                _lowFuelWarningIssued = true;
                Console.WriteLine("  WARNING!! Low fuel!");
            }

            // If already empty -> shut down and return
            if (_fuelLiters <= 0)
            {
                _fuelLiters = 0;
                _engineOn = false;
                _speed = 0;
                _rpm = 0;
                Console.WriteLine(" Fuel tank empty! Car stopped...");
                return;
            }

            // Normal consumption
            if (_speed < 1)
            {
                usedLiters = IdleFuelLph * hours;
            }
            else
            {
                // Base consumption curve (L/100km)
                double baseLPer100 = 6.2 + 4.0 * Math.Pow((_speed - 60) / 60.0, 2);
                double km = _speed * hours;
                usedLiters = baseLPer100 * (km / 100.0);
            }

            // Small RPM factor
            usedLiters *= (0.9 + 0.2 * (_rpm / MaxRpm));

            _fuelLiters = Math.Max(0, _fuelLiters - usedLiters);

            if (_fuelLiters <= 0)
            {
                _fuelLiters = 0;
                _engineOn = false;
                _speed = 0;
                _rpm = 0;
                Console.WriteLine(" Fuel tank empty! Car stopped...");
            }
        }
    }

    internal class TelemetryData
    {
        public int Rpm { get; set; }
        public int SpeedKmH { get; set; }
        public int FuelPercent { get; set; }
        public int EngineTempC { get; set; }
    }
}
