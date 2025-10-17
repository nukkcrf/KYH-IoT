using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class ThingSpeak
    {
        // Write key used to publish data to your channel.
        // For security in real projects, don't hardcode keys — use env vars or secrets.
        private const string WriteKey = "D7HQNNWLRGS38LCN";
        private const string ReadKey = "IB73Q1HDVLCKC8NG";

        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.thingspeak.com")
        };

        // Send all telemetry fields to ThingSpeak (fields 1..5).
        // Returns true on success.
        public async Task<bool> SendAsync(TelemetryData data)
        {
            // URL to update (we send all available telemetry fields)
            var url = $"/update?api_key={WriteKey}" +
                      $"&field1={data.Rpm}" +
                      $"&field2={data.SpeedKmH}" +
                      $"&field3={data.FuelLiters.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&field4={data.FuelPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&field5={data.EngineTempC}";

            try
            {
                using var resp = await _http.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();

                bool ok = resp.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(body) && body != "0";
                if (!ok)
                {
                   //Console.WriteLine($"[WARN] ThingSpeak answered : {body}");
                }

                return ok;
            }
            catch (Exception ex)
            {
                // Print error so student can see network or parsing problems.
                Console.WriteLine($"[ERROR] : {ex.Message}");
                return false;
            }
        }

        // Read recent feeds from a ThingSpeak channel using the read key.
        // channelId is required (ThingSpeak API needs channel id + read key).
        // results = number of feed entries to fetch (default 100).
        public async Task<List<TelemetryData>> ReadAsync(int channelId, int results = 100)
        {
            var url = $"/channels/{channelId}/feeds.json?api_key={ReadKey}&results={results}";

            try
            {
                using var resp = await _http.GetAsync(url);
                resp.EnsureSuccessStatusCode();

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var list = new List<TelemetryData>();
                if (doc.RootElement.TryGetProperty("feeds", out var feeds))
                {
                    foreach (var feed in feeds.EnumerateArray())
                    {
                        var item = new TelemetryData();

                        if (feed.TryGetProperty("field1", out var f1) && f1.ValueKind == JsonValueKind.String)
                            item.Rpm = ParseIntSafe(f1.GetString());

                        if (feed.TryGetProperty("field2", out var f2) && f2.ValueKind == JsonValueKind.String)
                            item.SpeedKmH = ParseIntSafe(f2.GetString());

                        if (feed.TryGetProperty("field3", out var f3) && f3.ValueKind == JsonValueKind.String)
                            item.FuelLiters = ParseDoubleSafe(f3.GetString());

                        if (feed.TryGetProperty("field4", out var f4) && f4.ValueKind == JsonValueKind.String)
                            item.FuelPercent = ParseDoubleSafe(f4.GetString());

                        if (feed.TryGetProperty("field5", out var f5) && f5.ValueKind == JsonValueKind.String)
                            item.EngineTempC = ParseIntSafe(f5.GetString());

                        list.Add(item);
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                // Show error so student can debug API issues.
                Console.WriteLine($"[ERROR] Reading ThingSpeak: {ex.Message}");
                return new List<TelemetryData>();
            }
        }

        private static int ParseIntSafe(string? s)
        {
            if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v;
            return 0;
        }

        private static double ParseDoubleSafe(string? s)
        {
            if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v;
            return 0.0;
        }
    }
}
