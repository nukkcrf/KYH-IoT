# Rapport — Analysystem för telemetri (KYH-IoT)

Denna rapport beskriver lösningen, designval, implementation och säkerhetsanalys för ett enkelt C#-baserat analysystem som simulerar biltelemetri, skickar data till ThingSpeak och kan läsa/analyssera data därifrån.

## Inledning
Syfte: En studentvänlig konsolapplikation som:
- genererar telemetriska datapunkter (RPM, hastighet, bränsle, temp),
- skickar dem till ThingSpeak,
- samlar lokalt senaste N datapunkter för snabb analys,
- och kan läsa tillbaka feeds från ThingSpeak för 24h-/100-punkts-analys.

## Design och Arkitektur

Jämförelse: Thingspeak vs Azure IoT Hub vs AWS IoT
- ThingSpeak
  - Enkel, billig/gratis nivå, designad för hobby/edu, REST/HTTP, begränsade funktioner.
  - Lätt att komma igång men saknar avancerade säkerhets-/skalningsfunktioner.
- Azure IoT Hub
  - Enterprise-grade: device provisioning, per-device authentication (X.509, SAS), hög skalbarhet, integrerat med Azure Stream Analytics, Event Hub, IoT Central.
  - Mer konfigurationsbehov och kostnad.
- AWS IoT Core
  - Likt Azure: robust autentisering, MQTT/HTTP, integration mot AWS Lambda, Kinesis, IoT Analytics.
  - Hög skalbarhet och säkerhet, mer lämpligt i produktion.

Val för detta projekt: ThingSpeak — snabb prototyp/undervisning med låg barriär.

Arkitektur (högnivå)
- Simulator (Program.cs + Car) genererar TelemetryData.
- ThingSpeak-klient (ThingSpeak.cs) skickar fälten till kanal med write key.
- Analyss (Analyss.cs) håller senaste 100 prover lokalt och/eller läser feeds från ThingSpeak med read key för 24h-analys.
- Presentation via konsolmeny.

