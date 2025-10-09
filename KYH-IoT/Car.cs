using System;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class Car
    {
        
        private const int MaxSpeed = 100;     //  [km/h]
        private const int IdleRpm = 800;     
        private const int MaxRpm = 9000;

        // Fuel tank and consumption
        private const double TankLiters = 75.0; // tank capacity [L]
        private const double IdleFuelLph = 2.0;  //  [L/h]

        // Simulation acceleration: how many times faster than real time
        private const double TimeAcceleration = 2000.0; // ex: 3600.0 =   1 real sec = 1 simulated hour(tank empties in ~70 sec).

        // Current state
        private bool _engineOn = true;
        private bool _lowFuelWarningIssued = false;
        private double _speed;          // km/h
        private double _rpm;            // rpm
        private double _fuelLiters;     // liters left
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
            _nextTargetChangeUtc = DateTime.UtcNow;
        }

        public TelemetryData NextSample(TimeSpan step)
        {
            // If engine is OFF: keep car stopped, cool down slowly, return zeros
            if (!_engineOn)
            {
                _speed = 0;
                _rpm = 0;
                if (_engineTempC > 80) _engineTempC = Math.Max(80, _engineTempC - 1);
                return BuildTelemetry();
            }

            MaybeChangeTarget();

            // Limit acceleration/deceleration for realism
            double maxDeltaPerSec = 3.0;
            double maxStep = maxDeltaPerSec * step.TotalSeconds;

            double delta = _targetSpeed - _speed;
            double change = Math.Clamp(delta, -maxStep, maxStep);

            // Add small random noise
            change += (_rng.NextDouble() - 0.5) * 0.6;
            _speed = Math.Clamp(_speed + change, 0, MaxSpeed);

            // Calculate RPM from current speed
            _rpm = CalculateRpm(_speed);

            // Consume fuel (may shut down the engine if empty)
            ConsumeFuel(step);

            if (!_engineOn)
            {
                _speed = 0;
                _rpm = 0;
                if (_engineTempC > 80) _engineTempC = Math.Max(80, _engineTempC - 1);
                return BuildTelemetry();
            }

            // Engine temperature drift
            int tempDrift = (_rpm > 4000 ? 2 : 0) + _rng.Next(-1, 2);
            _engineTempC = Math.Clamp(_engineTempC + tempDrift, 80, 99);

            return BuildTelemetry();
        }

        private TelemetryData BuildTelemetry()
        {
            double fuelPercent = (_fuelLiters / TankLiters) * 100.0;

            return new TelemetryData
            {
                Rpm = (int)Math.Round(_rpm),
                SpeedKmH = (int)Math.Round(_speed),
                FuelLiters = Math.Round(_fuelLiters, 2),
                FuelPercent = Math.Round(fuelPercent, 1),
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

            // Scale simulated time by TimeAcceleration
            double hours = step.TotalHours * TimeAcceleration;
            double usedLiters;

            double fuelPercent = (_fuelLiters / TankLiters) * 100.0;
            if (fuelPercent <= 30.0 && !_lowFuelWarningIssued && _fuelLiters > 0)
            {
                _lowFuelWarningIssued = true;
                Console.WriteLine("!!!!!!!!!!   WARNING: Low fuel.");
            }

            if (_fuelLiters <= 0)
            {
                StopEngineDueToFuel();
                return;
            }

            if (_speed < 1)
            {
                usedLiters = IdleFuelLph * hours;
            }
            else
            {
                double baseLPer100 = 6.2 + 4.0 * Math.Pow((_speed - 60) / 60.0, 2);
                double km = _speed * hours;
                usedLiters = baseLPer100 * (km / 100.0);
            }

            usedLiters *= (0.9 + 0.2 * (_rpm / MaxRpm));
            _fuelLiters = Math.Max(0, _fuelLiters - usedLiters);

            if (_fuelLiters <= 0)
            {
                StopEngineDueToFuel();
            }
        }

        private void StopEngineDueToFuel()
        {
            _fuelLiters = 0;
            _engineOn = false;
            _speed = 0;
            _rpm = 0;
            Console.WriteLine();
            Console.WriteLine("=========Fuel tank empty. Car stopped.=========");
            Console.WriteLine();
            Console.WriteLine("---------Engine cooling initiated------------");
        }
    }

    internal class TelemetryData
    {
        public int Rpm { get; set; }
        public int SpeedKmH { get; set; }
        public double FuelLiters { get; set; }
        public double FuelPercent { get; set; }
        public int EngineTempC { get; set; }
    }
}
