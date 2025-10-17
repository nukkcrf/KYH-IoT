# Rapport � Analysystem f�r telemetri (KYH-IoT)

Denna rapport beskriver l�sningen, designval, implementation och s�kerhetsanalys f�r ett enkelt C#-baserat analysystem som simulerar biltelemetri, skickar data till ThingSpeak och kan l�sa/analyssera data d�rifr�n.

## Inledning
Syfte: En studentv�nlig konsolapplikation som:
- genererar telemetriska datapunkter (RPM, hastighet, br�nsle, temp),
- skickar dem till ThingSpeak,
- samlar lokalt senaste N datapunkter f�r snabb analys,
- och kan l�sa tillbaka feeds fr�n ThingSpeak f�r 24h-/100-punkts-analys.

## Design och Arkitektur

J�mf�relse: Thingspeak vs Azure IoT Hub vs AWS IoT
- ThingSpeak
  - Enkel, billig/gratis niv�, designad f�r hobby/edu, REST/HTTP, begr�nsade funktioner.
  - L�tt att komma ig�ng men saknar avancerade s�kerhets-/skalningsfunktioner.
- Azure IoT Hub
  - Enterprise-grade: device provisioning, per-device authentication (X.509, SAS), h�g skalbarhet, integrerat med Azure Stream Analytics, Event Hub, IoT Central.
  - Mer konfigurationsbehov och kostnad.
- AWS IoT Core
  - Likt Azure: robust autentisering, MQTT/HTTP, integration mot AWS Lambda, Kinesis, IoT Analytics.
  - H�g skalbarhet och s�kerhet, mer l�mpligt i produktion.

Val f�r detta projekt: ThingSpeak � snabb prototyp/undervisning med l�g barri�r.

Arkitektur (h�gniv�)
- Simulator (Program.cs + Car) genererar TelemetryData.
- ThingSpeak-klient (ThingSpeak.cs) skickar f�lten till kanal med write key.
- Analyss (Analyss.cs) h�ller senaste 100 prover lokalt och/eller l�ser feeds fr�n ThingSpeak med read key f�r 24h-analys.
- Presentation via konsolmeny.

