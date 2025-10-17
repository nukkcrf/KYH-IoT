# KYH-IoT — Car Telemetry Simulator

Simple student project: a console app that simulates car telemetry, sends data to ThingSpeak and shows basic analysis.

What it does
- Generates samples: RPM, speed, fuel and engine temp.
- Sends samples to ThingSpeak (fields 1-5).
- Keeps last 100 local samples and can print averages and max values.
- Small menu to run the simulator or view analysis.

Quick how-to
1. Build with .NET 9 SDK.
2. Run from IDE or:
   dotnet run --project KYH-IoT
3. Choose "Run Simulation" to start. The app runs for ~1 minute and prints a final analysis.

Notes for students
- Keys are currently in ThingSpeak.cs for simplicity. Move them to env vars in real projects.
- To do 24h analysis properly, store timestamps or read feeds from ThingSpeak with channel id.
- The code is intentionally small and easy to read — tweak parameters and experiment.

Files of interest
- KYH-IoT/Program.cs — menu and simulator loop
- KYH-IoT/Car.cs — simulation model
- KYH-IoT/Analyss.cs — local analysis buffer
- KYH-IoT/ThingSpeak.cs — send/read to ThingSpeak

Have fun experimenting!