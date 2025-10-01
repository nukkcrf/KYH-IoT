using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class ThingSpeak
    {
       
        private const string WriteKey = "D7HQNNWLRGS38LCN";

        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.thingspeak.com")
        };

        public async Task<bool> SendAsync(TelemetryData data)
        {
            // URL to  update
            var url = $"/update?api_key={WriteKey}" +
                      $"&field1={data.Rpm}" +
                      $"&field2={data.SpeedKmH}" +
                      $"&field3={data.FuelPercent}" +
                      $"&field4={data.EngineTempC}";

            try
            {
                using var resp = await _http.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();

                bool ok = resp.IsSuccessStatusCode && body != "New data : ";
                if (!ok)
                {
                   Console.WriteLine($"[WARN] ThingSpeak aswers : {body}");
                }

                return ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] : {ex.Message}");
                return false;
            }
        }
    }
}
